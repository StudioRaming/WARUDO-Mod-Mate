"""Black-box checks for warudo_fingerprint.py against fake Steam trees and logs. Run: python test_warudo_fingerprint.py"""
import importlib.util
import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

TOOL = Path(__file__).with_name("warudo_fingerprint.py")
HERE = Path(tempfile.mkdtemp(prefix="warudo-fingerprint-"))

LOAD = "Fallback handler could not load library C:/x/Warudo_Data/Mono/data-0123ABCD.dll\n"
fails = 0


def run(*args):
    proc = subprocess.run([sys.executable, str(TOOL), *args], capture_output=True, text=True, encoding="utf-8")
    return proc.returncode, proc.stdout.strip(), proc.stderr.strip()


def check(name, cond, detail):
    global fails
    print(("PASS " if cond else "FAIL ") + name + " | " + detail)
    if not cond:
        fails += 1


# Steam root that lists a second library; the second library has a mismatched BetaKey and UMod.dll missing.
root = HERE / "steam"
lib = HERE / "lib2"
(root / "steamapps").mkdir(parents=True)
(root / "steamapps" / "libraryfolders.vdf").write_text(
    '"libraryfolders"\n{\n\t"0"\n\t{\n\t\t"path"\t\t"%s"\n\t}\n\t"1"\n\t{\n\t\t"path"\t\t"%s"\n\t}\n}\n'
    % (str(root).replace("\\", "\\\\"), str(lib).replace("\\", "\\\\")), encoding="utf-8")
(lib / "steamapps").mkdir(parents=True)
(lib / "steamapps" / "appmanifest_2079120.acf").write_text(
    '"AppState"\n{\n\t"buildid"\t\t"111"\n\t"TargetBuildID"\t\t"222"\n\t"UserConfig"\n\t{\n\t\t"BetaKey"\t\t"beta"\n\t}\n'
    '\t"MountedConfig"\n\t{\n\t\t"BetaKey"\t\t""\n\t}\n}\n', encoding="utf-8")
managed = lib / "steamapps" / "common" / "Warudo" / "Warudo_Data" / "Managed"
managed.mkdir(parents=True)
for name in ("MagicaClothV2.dll", "UMod-Shared.dll", "Warudo.Core.dll"):
    (managed / name).write_bytes(name.encode())

code, out, err = run("--steam-root", str(root), "--json")
rec = json.loads(out)
check("second library + mismatch + missing DLL", code == 2 and rec["buildid"] == "111" and rec["branch"] == "mismatch"
      and rec["sha256"]["UMod.dll"] == "MISSING" and "UMod.dll" in err, "exit=%d branch=%s err=%s" % (code, rec["branch"], err))

code, out, err = run("--steam-root", str(HERE / "nowhere"))
check("no install", code == 2 and out == "" and "appmanifest_2079120.acf" in err and str(HERE) not in err, "exit=%d err=%s" % (code, err))

# Logs: A (10 lines, 2 loads), B = A + 3 lines with 1 load, C = a restarted log of 15 lines with 3 loads.
a = ["line %d\n" % i if i not in (3, 8) else LOAD for i in range(1, 11)]
b = a + ["tail 1\n", LOAD, "tail 3\n"]
c = ["fresh %d\n" % i if i not in (2, 5, 9) else LOAD for i in range(1, 16)]
for name, lines in (("A.log", a), ("B.log", b), ("C.log", c)):
    (HERE / name).write_text("".join(lines), encoding="utf-8")

code, out, _ = run("--steam-root", str(root), "--player-log", str(HERE / "A.log"), "--json")
pa = json.loads(out)["playerLog"]
check("first run counts all", pa["lines"] == 10 and pa["scriptLoads"] == 2 and not pa["restarted"], json.dumps(pa))
token = pa["next"]

code, out, _ = run("--steam-root", str(root), "--player-log", str(HERE / "B.log"), "--since", token, "--json")
pb = json.loads(out)["playerLog"]
check("same session counts only new", pb["scriptLoads"] == 1 and not pb["restarted"], json.dumps(pb))

code, out, _ = run("--steam-root", str(root), "--player-log", str(HERE / "C.log"), "--since", token, "--json")
pc = json.loads(out)["playerLog"]
check("longer restarted log detected", pc["restarted"] and pc["scriptLoads"] == 3, json.dumps(pc))

code, out, _ = run("--steam-root", str(root), "--player-log", str(HERE / "A.log"), "--since", "20:" + token.split(":")[1])
check("shorter log detected", "restarted" in out and "scriptLoads(after 20)=2" in out, out.split("| ")[-1])

code, out, _ = run("--steam-root", str(root), "--player-log", str(HERE / "missing.log"))
check("missing Player.log keeps the record", code == 2 and "Player.log not found" in out and "MagicaClothV2.dll=" in out, out[-60:])

# Broken known_builds.json falls back to unknown.
spec = importlib.util.spec_from_file_location("fp", TOOL)
fp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(fp)
broken = HERE / "broken.json"
broken.write_text("{not json", encoding="utf-8")
fp.KNOWN = broken
check("broken known_builds.json", fp.match_known("1", {}).startswith("unknown"), fp.match_known("1", {}))

shutil.rmtree(HERE, ignore_errors=True)
print("FAILS", fails)
sys.exit(1 if fails else 0)
