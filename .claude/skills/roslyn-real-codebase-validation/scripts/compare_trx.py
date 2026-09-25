#!/usr/bin/env python3
"""Compare test outcomes between two `dotnet test --logger trx` runs.

Each argument is a .trx file or a folder searched recursively for .trx files.
A test whose outcome changed after a code fix is the main signal that the fix
changed what the test checks.

usage: compare_trx.py BEFORE AFTER
exit code: 0 if no outcome changed, 1 otherwise
"""
import sys
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def outcomes(path):
    p = Path(path)
    files = [p] if p.is_file() else sorted(p.rglob("*.trx"))
    if not files:
        sys.exit(f"no .trx files under {path}")
    result = {}
    for trx in files:
        root = ET.parse(trx).getroot()
        classes = {
            d.get("id"): d.find("t:TestMethod", NS).get("className")
            for d in root.iterfind(".//t:TestDefinitions/t:UnitTest", NS)
        }
        for r in root.iterfind(".//t:Results/t:UnitTestResult", NS):
            name = f"{classes.get(r.get('testId'), '?')}.{r.get('testName')}"
            result[name] = r.get("outcome")
    return result


def main(argv):
    if len(argv) != 2:
        sys.exit(__doc__)
    before, after = outcomes(argv[0]), outcomes(argv[1])

    print("before:", dict(Counter(before.values())))
    print("after: ", dict(Counter(after.values())))

    changed = sorted(n for n in before.keys() & after.keys() if before[n] != after[n])
    missing = sorted(before.keys() - after.keys())
    added = sorted(after.keys() - before.keys())

    for label, names in (("OUTCOME CHANGED", changed), ("only before (didn't run after)", missing), ("only after", added)):
        if names:
            print(f"\n{label}: {len(names)}")
            for n in names:
                print(f"  {before.get(n, '-'):>8} -> {after.get(n, '-'):<8} {n}")

    sys.exit(1 if changed or missing else 0)


if __name__ == "__main__":
    main(sys.argv[1:])
