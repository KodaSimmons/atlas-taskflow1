#!/usr/bin/env python3
"""Checks that roadmap/ follows its own rules (see roadmap/ROADMAP.md §0).

Usage:
  python3 scripts/roadmap-lint.py              check the repo's roadmap
  python3 scripts/roadmap-lint.py --self-test  prove every check can fail
"""
import re
import shutil
import sys
import tempfile
from pathlib import Path

STATUS_WORDS = {"HORIZON", "PLANNED", "ACTIVE", "BLOCKED", "REVIEW", "DONE", "DEFERRED", "DROPPED", "SUPERSEDED"}
REGISTER_ROW = re.compile(r"^\| \[(PLAN-\d{3})\]\((plans/[^)]+)\) \|.*\| `([A-Z]+)` \|$")
PLAN_STATUS = re.compile(r"^Status: `([A-Z]+)`", re.M)
LINK = re.compile(r"\]\(([^)#]+)(?:#[^)]*)?\)")


def check(root: Path) -> list[str]:
    """Returns one message per problem; an empty list means the roadmap is valid."""
    errors = []
    roadmap_file = root / "ROADMAP.md"
    if not roadmap_file.exists():
        return ["missing: ROADMAP.md"]
    roadmap = roadmap_file.read_text()

    # Register rows: each links an existing plan file, uses an allowed word and matches the plan's own status.
    registered = {}
    for line in roadmap.splitlines():
        m = REGISTER_ROW.match(line)
        if not m:
            continue
        plan_id, rel, status = m.groups()
        if plan_id in registered:
            errors.append(f"register: {plan_id} is listed twice")
        registered[plan_id] = rel
        if status not in STATUS_WORDS:
            errors.append(f"status: {plan_id} uses '{status}' in the register, which is not a status word")
        plan_file = root / rel
        if not plan_file.exists():
            errors.append(f"register: {plan_id} links {rel}, which does not exist")
            continue
        text = plan_file.read_text()
        if not text.startswith(f"# {plan_id} "):
            errors.append(f"plan: {rel} should start with '# {plan_id} '")
        sm = PLAN_STATUS.search(text)
        if not sm:
            errors.append(f"plan: {rel} has no 'Status: `WORD`' line")
        elif sm.group(1) not in STATUS_WORDS:
            errors.append(f"status: {rel} uses '{sm.group(1)}', which is not a status word")
        elif sm.group(1) != status:
            errors.append(f"maintenance: {plan_id} is `{status}` in the register but `{sm.group(1)}` in its plan file")

    # Birth rule: every plan file is registered.
    for plan_file in sorted((root / "plans").glob("PLAN-*.md")):
        plan_id = plan_file.name[:8]
        if plan_id not in registered:
            errors.append(f"birth: {plan_file.name} exists but is not in the §4 register")

    # Decision and ruling indexes list every record and nothing else.
    for folder, prefix in (("decisions", "DR-"), ("rulings", "OV-")):
        index = root / folder / "INDEX.md"
        if not index.exists():
            errors.append(f"missing: {folder}/INDEX.md")
            continue
        listed = set(re.findall(rf"\]\(({prefix}[^)]+\.md)\)", index.read_text()))
        present = {p.name for p in (root / folder).glob(f"{prefix}*.md")}
        for name in sorted(present - listed):
            errors.append(f"index: {folder}/{name} is not listed in {folder}/INDEX.md")
        for name in sorted(listed - present):
            errors.append(f"index: {folder}/INDEX.md lists {name}, which does not exist")

    # Every relative link in the roadmap resolves.
    for target in LINK.findall(roadmap):
        if "://" not in target and not (root / target).exists():
            errors.append(f"link: ROADMAP.md links {target}, which does not exist")

    return errors


def self_test(root: Path) -> int:
    """Breaks a copy of the roadmap one rule at a time and confirms the matching check fails."""
    def first_plan(r):
        return sorted((r / "plans").glob("PLAN-*.md"))[0]

    def set_plan_status(r, word):
        p = first_plan(r)
        p.write_text(PLAN_STATUS.sub(f"Status: `{word}`", p.read_text(), count=1))

    cases = {
        "birth": lambda r: (r / "plans" / "PLAN-999-unregistered.md").write_text("# PLAN-999 x\n\nStatus: `HORIZON`\n"),
        "maintenance": lambda r: set_plan_status(r, "DONE" if "`DONE`" not in first_plan(r).read_text() else "ACTIVE"),
        "status": lambda r: set_plan_status(r, "FINISHED"),
        "register": lambda r: first_plan(r).unlink(),
        "index": lambda r: (r / "decisions" / "DR-9999-unlisted.md").write_text("# DR-9999\n"),
        "link": lambda r: (r / "ROADMAP.md").write_text((r / "ROADMAP.md").read_text() + "\n[gone](nowhere.md)\n"),
        "missing": lambda r: (r / "rulings" / "INDEX.md").unlink(),
    }

    baseline = check(root)
    if baseline:
        print("self-test needs a valid roadmap to start from:", *baseline, sep="\n  ")
        return 1

    failed = 0
    for name, breaker in cases.items():
        with tempfile.TemporaryDirectory() as tmp:
            copy = Path(tmp) / "roadmap"
            shutil.copytree(root, copy)
            # The roadmap links the repo-level changelog, so bring it along.
            if (root.parent / "CHANGELOG.md").exists():
                shutil.copy(root.parent / "CHANGELOG.md", Path(tmp) / "CHANGELOG.md")
            breaker(copy)
            caught = [e for e in check(copy) if e.startswith(name + ":")]
            print(f"  {'ok  ' if caught else 'FAIL'} {name} check {'fails as expected' if caught else 'did not fire'}")
            failed += not caught
    print("self-test passed" if not failed else f"self-test: {failed} check(s) never fail")
    return 1 if failed else 0


def main() -> int:
    root = Path(__file__).resolve().parent.parent / "roadmap"
    if "--self-test" in sys.argv:
        return self_test(root)
    errors = check(root)
    for e in errors:
        print(e)
    print("roadmap ok" if not errors else f"{len(errors)} roadmap problem(s)")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
