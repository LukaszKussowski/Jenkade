#!/usr/bin/env python3
"""Fails when Polish text appears outside the files allowed to hold it.

Polish belongs only in Polish resources (*.pl.resx) and in GLOSSARY.md's _Polish UI_ entries;
code, docs and English resources are English. Detects Polish by its diacritics, so it is a
heuristic: Polish written without them passes.
"""
import re
import subprocess
import sys

ALLOWED = re.compile(r"(^|/)(GLOSSARY\.md|[^/]+\.pl\.resx)$")
# Polish diacritics, lower and upper case, escaped so this file passes its own check.
POLISH = re.compile("[\u0105\u0107\u0119\u0142\u0144\u00f3\u015b\u017a\u017c\u0104\u0106\u0118\u0141\u0143\u00d3\u015a\u0179\u017b]")

files = subprocess.run(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard"], capture_output=True, check=True, text=True).stdout.split("\0")
found = []
for path in filter(None, files):
    if ALLOWED.search(path):
        continue
    try:
        with open(path, encoding="utf-8") as f:
            lines = f.read().splitlines()
    except (UnicodeDecodeError, FileNotFoundError, IsADirectoryError):
        continue
    found += [f"{path}:{n}: {line.strip()}" for n, line in enumerate(lines, 1) if POLISH.search(line)]

if found:
    print("Polish text outside *.pl.resx and GLOSSARY.md (see CODING_STANDARDS.md):")
    print("\n".join(found))
    sys.exit(1)
