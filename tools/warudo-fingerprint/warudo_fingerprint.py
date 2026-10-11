"""Record which WARUDO build a live test ran against.

A result measured on the Steam beta branch says nothing about the default branch most customers run, so every live-test
receipt carries the branch, the build id and the hashes of the DLLs that decide mod loading and cloth data. Run this before
and after each live test and paste the line into the receipt (or pass --append).

  python tools/warudo-fingerprint/warudo_fingerprint.py [--steam-root DIR] [--player-log FILE [--since-line N]]
                                                        [--json] [--append FILE]

--player-log counts the lines that show a mod's scripts being loaded. Current WARUDO builds no longer print "Load mod:";
a mod assembly loaded at runtime shows as "Fallback handler could not load library .../data-<address>.dll". Pass the
line count printed by the run before the test as --since-line to count only what the test added.
"""
import argparse
import datetime
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


def default_steam_root():
    base = os.environ.get("ProgramFiles(x86)") or "C:/Program Files (x86)"
    return Path(base) / "Steam"


def default_player_log():
    profile = os.environ.get("USERPROFILE")
    return Path(profile) / "AppData" / "LocalLow" / "HakuyaLabs" / "Warudo" / "Player.log" if profile else None


def read_manifest(steam_root):
    manifest = steam_root / "steamapps" / ("appmanifest_%s.acf" % APP_ID)
    text = manifest.read_text(encoding="utf-8", errors="replace")
    build = re.search(r'"buildid"\s+"(\d+)"', text)
    # BetaKey appears under UserConfig (what the user picked) and MountedConfig (what is installed); both are reported.
    betas = re.findall(r'"BetaKey"\s+"([^"]*)"', text)
    return manifest, (build.group(1) if build else ""), betas


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def match_known(build_id, hashes):
    if not KNOWN.exists():
        return "unknown"
    for entry in json.loads(KNOWN.read_text(encoding="utf-8")):
        if entry.get("buildid") == build_id and all(entry.get("sha256", {}).get(name) == value for name, value in hashes.items()):
            return entry["label"]
    return "unknown (add it to known_builds.json once confirmed)"


def count_script_loads(log, since_line):
    total = loads = 0
    with open(log, "r", encoding="utf-8", errors="replace") as handle:
        for total, line in enumerate(handle, 1):
            if total > since_line and SCRIPT_LOAD.search(line):
                loads += 1
    return total, loads


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--steam-root", type=Path, default=default_steam_root())
    parser.add_argument("--player-log", nargs="?", const="", default=None, help="count script loads (no value = default path)")
    parser.add_argument("--since-line", type=int, default=0)
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--append", type=Path)
    args = parser.parse_args()

    try:
        manifest, build_id, betas = read_manifest(args.steam_root)
    except FileNotFoundError as error:
        print("ERROR no WARUDO install found (%s); pass --steam-root" % error.filename, file=sys.stderr)
        return 2
    managed = args.steam_root / "steamapps" / "common" / "Warudo" / "Warudo_Data" / "Managed"
    hashes = {name: sha256(managed / name) for name in DLLS}
    record = {
        "observedAt": datetime.datetime.now().astimezone().isoformat(timespec="seconds"),
        "buildid": build_id,
        "betaKey": betas,
        "branch": "beta" if any(b for b in betas) else "default",
        "known": match_known(build_id, hashes),
        "sha256": hashes,
    }
    if args.player_log is not None:
        log = Path(args.player_log) if args.player_log else default_player_log()
        lines, loads = count_script_loads(log, args.since_line)
        # WARUDO starts a new Player.log on every launch, so a shorter log than --since-line means it restarted in between.
        record["playerLog"] = {"lines": lines, "sinceLine": args.since_line, "scriptLoads": loads, "restarted": lines < args.since_line}

    if args.json:
        out = json.dumps(record, ensure_ascii=False)
    else:
        out = "WARUDO %s buildid=%s BetaKey=%s known=%s | %s" % (
            record["branch"], build_id, "/".join(betas) or "-", record["known"],
            " ".join("%s=%s" % (name, value[:16]) for name, value in hashes.items()))
        if "playerLog" in record:
            log_info = record["playerLog"]
            out += " | Player.log lines=%d scriptLoads(since %d)=%d" % (log_info["lines"], log_info["sinceLine"], log_info["scriptLoads"])
            if log_info["restarted"]:
                out += " (log is shorter than --since-line: WARUDO restarted, see Player-prev.log)"
    print(out)
    if args.append:
        with open(args.append, "a", encoding="utf-8") as handle:
            handle.write(out + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
