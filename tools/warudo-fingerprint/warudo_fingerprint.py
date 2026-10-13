"""Record which WARUDO build a live test ran against.

A result measured on the Steam beta branch says nothing about the default branch most customers run, so every live-test
receipt carries the branch, the build id and the hashes of the DLLs that decide mod loading and cloth data. Run this before
and after each live test and paste the line into the receipt (or pass --append).

  python tools/warudo-fingerprint/warudo_fingerprint.py [--steam-root DIR] [--player-log [FILE] [--since N:HASH]]
                                                        [--json] [--append FILE]

--player-log counts the lines that show a mod's scripts being loaded. Current WARUDO builds no longer print "Load mod:";
a mod assembly loaded at runtime shows as "Fallback handler could not load library .../data-<address>.dll". Each run
prints since=N:HASH (the line count and a hash of those lines); pass it as --since on the run after the test to count only
what the test added. A different hash means WARUDO restarted in between (it starts a new Player.log on every launch), and
the whole new log is counted.

Exit codes: 0 = recorded, 2 = no install found or a DLL is missing (the line is still printed with MISSING),
3 = --player-log was requested and the log could not be read (the line is still printed, without a script load count).
A missing DLL wins over an unreadable log.
"""
import argparse
import datetime
import errno
import hashlib
import json
import os
import re
import sys
from pathlib import Path

APP_ID = "2079120"
DLLS = ("MagicaClothV2.dll", "UMod.dll", "UMod-Shared.dll", "Warudo.Core.dll")
SCRIPT_LOAD = re.compile(r"Fallback handler could not load library .*[\\/]data-[0-9A-Fa-f]+\.dll")
KNOWN = Path(__file__).with_name("known_builds.json")
MANIFEST = "appmanifest_%s.acf" % APP_ID
HASH_CHARS = 12


def default_steam_root():
    base = os.environ.get("ProgramFiles(x86)") or "C:/Program Files (x86)"
    return Path(base) / "Steam"


def default_player_log():
    profile = os.environ.get("USERPROFILE")
    return Path(profile) / "AppData" / "LocalLow" / "HakuyaLabs" / "Warudo" / "Player.log" if profile else None


def find_library(steam_root):
    """Return the Steam library folder that holds WARUDO: the given root, or one listed in its libraryfolders.vdf."""
    if (steam_root / "steamapps" / MANIFEST).exists():
        return steam_root
    folders = steam_root / "steamapps" / "libraryfolders.vdf"
    if folders.exists():
        for raw in re.findall(r'"path"\s+"([^"]*)"', folders.read_text(encoding="utf-8", errors="replace")):
            library = Path(raw.replace("\\\\", "\\"))
            if (library / "steamapps" / MANIFEST).exists():
                return library
    return None


def read_manifest(library):
    text = (library / "steamapps" / MANIFEST).read_text(encoding="utf-8", errors="replace")
    build = re.search(r'"buildid"\s+"(\d+)"', text)
    # BetaKey appears under UserConfig (what the user picked) and MountedConfig (what is installed); both are reported,
    # in that order. Steam can leave the key out of a section on the default branch, so each section is read on its own
    # and a missing key counts as "": picking beta before the download finishes then reads as a mismatch, not as beta.
    betas = [section_beta(text, "UserConfig"), section_beta(text, "MountedConfig")]
    # Steam installs into steamapps/common/<installdir>; it is "Warudo" unless the manifest says otherwise.
    installdir = re.search(r'"installdir"\s+"([^"]*)"', text)
    return (build.group(1) if build else ""), betas, ((installdir.group(1) if installdir else "") or "Warudo")


def section_beta(text, section):
    block = re.search(r'"%s"\s*\{([^{}]*)\}' % section, text)
    key = re.search(r'"BetaKey"\s+"([^"]*)"', block.group(1)) if block else None
    return key.group(1) if key else ""


def branch_of(betas):
    picked = set(betas)
    if len(picked) > 1:
        return "mismatch"  # picked a branch in Steam but the download has not finished
    return "beta" if picked and next(iter(picked)) else "default"


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def match_known(build_id, hashes):
    try:
        entries = json.loads(KNOWN.read_text(encoding="utf-8"))
    except (OSError, ValueError):
        return "unknown (known_builds.json missing or unreadable)"
    if not isinstance(entries, list) or not all(
            isinstance(entry, dict) and isinstance(entry.get("buildid"), str) and isinstance(entry.get("label"), str)
            and isinstance(entry.get("sha256"), dict) for entry in entries):
        return "unknown (known_builds.json has an unexpected shape)"
    for entry in entries:
        if entry.get("buildid") == build_id and all(entry.get("sha256", {}).get(name) == value for name, value in hashes.items()):
            return entry["label"]
    return "unknown (add it to known_builds.json once confirmed)"


def parse_since(token):
    count, _, prefix = token.partition(":")
    if not count.isdigit() or not re.fullmatch(r"[0-9A-Fa-f]*", prefix):
        raise argparse.ArgumentTypeError("--since takes N or N:HASH as printed after since=, not %r" % token)
    return int(count), prefix.lower()


def count_script_loads(log, since_lines, since_hash):
    """Count script loads after the first since_lines lines, or in the whole log when those lines are not the same."""
    total = 0
    loads_after = loads_all = 0
    prefix = hashlib.sha256()
    prefix_hex = ""
    with open(log, "rb") as handle:
        for raw in handle:
            if not raw.endswith(b"\n"):
                break  # WARUDO is still writing this line; hashing it now would read as a restart on the next run
            total += 1
            prefix.update(raw)
            if total == since_lines:
                prefix_hex = prefix.hexdigest()[:HASH_CHARS]
            if SCRIPT_LOAD.search(raw.decode("utf-8", errors="replace")):
                loads_all += 1
                if total > since_lines:
                    loads_after += 1
    restarted = since_lines > 0 and (total < since_lines or (since_hash and prefix_hex != since_hash))
    return {
        "lines": total,
        "since": since_lines,
        "scriptLoads": loads_all if restarted else loads_after,
        "restarted": bool(restarted),
        "next": "%d:%s" % (total, prefix.hexdigest()[:HASH_CHARS]),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--steam-root", type=Path, default=default_steam_root())
    parser.add_argument("--player-log", nargs="?", const="", default=None, help="count script loads (no value = default path)")
    parser.add_argument("--since", type=parse_since, default=(0, ""), help="N:HASH printed as since= by the run before the test")
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--append", type=Path)
    args = parser.parse_args()

    library = find_library(args.steam_root)
    if library is None:
        print("ERROR no %s under the Steam root or its libraryfolders.vdf; pass --steam-root <library>" % MANIFEST, file=sys.stderr)
        return 2
    build_id, betas, installdir = read_manifest(library)
    managed = library / "steamapps" / "common" / installdir / "Warudo_Data" / "Managed"
    hashes = {}
    missing = []
    for name in DLLS:
        try:
            hashes[name] = sha256(managed / name)
        except OSError:
            hashes[name] = "MISSING"
            missing.append(name)
    record = {
        "observedAt": datetime.datetime.now().astimezone().isoformat(timespec="seconds"),
        "buildid": build_id,
        "installdir": installdir,
        "betaKey": betas,
        "branch": branch_of(betas),
        "known": match_known(build_id, hashes),
        "sha256": hashes,
    }
    if args.player_log is not None:
        log = Path(args.player_log) if args.player_log else default_player_log()
        since_lines, since_hash = args.since
        try:
            if log is None:
                raise FileNotFoundError("USERPROFILE is not set")
            record["playerLog"] = count_script_loads(log, since_lines, since_hash)
        except FileNotFoundError:
            record["playerLog"] = {"error": "Player.log not found"}
        except OSError as e:
            # WARUDO holding the file open, or no permission: the log exists, so say why it could not be read.
            name = errno.errorcode.get(e.errno, type(e).__name__) if e.errno else type(e).__name__
            record["playerLog"] = {"error": "Player.log unreadable: %s" % name}

    if args.json:
        out = json.dumps(record, ensure_ascii=False)
    else:
        out = "WARUDO %s buildid=%s BetaKey=%s known=%s | %s" % (
            record["branch"], build_id, "/".join(betas) or "-", record["known"],
            " ".join("%s=%s" % (name, value[:16]) for name, value in hashes.items()))
        log_info = record.get("playerLog")
        if log_info and "error" in log_info:
            out += " | %s" % log_info["error"]
        elif log_info:
            out += " | Player.log lines=%d scriptLoads(after %d)=%d since=%s" % (
                log_info["lines"], log_info["since"], log_info["scriptLoads"], log_info["next"])
            if log_info["restarted"]:
                out += " (WARUDO restarted after --since, whole new log counted; the old one is Player-prev.log)"
    print(out)
    if args.append:
        with open(args.append, "a", encoding="utf-8") as handle:
            handle.write(out + "\n")
    if missing:
        print("ERROR missing in Warudo_Data/Managed: %s" % ", ".join(missing), file=sys.stderr)
        return 2
    if "error" in record.get("playerLog", {}):
        print("ERROR %s; this receipt has no script load count" % record["playerLog"]["error"], file=sys.stderr)
        return 3
    return 0


if __name__ == "__main__":
    sys.exit(main())
