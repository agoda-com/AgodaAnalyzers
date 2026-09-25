#!/usr/bin/env python3
"""Count distinct diagnostic sites per ID in a `dotnet build` log.

MSBuild prints each diagnostic once per project and target framework, and again in
the summary, so raw grep counts overstate. A site here is (file, line, column, ID).

usage: count_diagnostics.py BUILD_LOG ID [ID ...] [--sites]
  --sites  print one "file(line,col): ID message" line per distinct site instead of counts
"""
import re
import sys
from collections import Counter

LINE = re.compile(
    r"^(?P<file>.+?)\((?P<line>\d+),(?P<col>\d+)(?:,\d+,\d+)?\): "
    r"(?i:warning|error|info) (?P<id>[A-Za-z]+\d+): (?P<msg>.*?)(?: \[[^\]]+\])?$"
)


def main(argv):
    args = [a for a in argv if not a.startswith("--")]
    if len(args) < 2:
        sys.exit(__doc__)
    log, ids = args[0], set(args[1:])
    sites = {}
    with open(log, encoding="utf-8", errors="replace") as f:
        for raw in f:
            m = LINE.match(raw.strip())
            if m and m["id"] in ids:
                key = (m["file"], int(m["line"]), int(m["col"]), m["id"])
                sites.setdefault(key, m["msg"])

    if "--sites" in argv:
        for (file, line, col, diag_id), msg in sorted(sites.items()):
            print(f"{file}({line},{col}): {diag_id} {msg}")
        return

    per_id = Counter(k[3] for k in sites)
    per_file = Counter((k[3], k[0]) for k in sites)
    for diag_id in sorted(ids):
        print(f"{diag_id}: {per_id[diag_id]} distinct sites")
        for (i, file), n in per_file.most_common():
            if i == diag_id:
                print(f"  {n:5d}  {file}")


if __name__ == "__main__":
    main(sys.argv[1:])
