# Working on Atlas TaskFlow

Atlas TaskFlow is Koda Simmons's course project: an ASP.NET Core 8 MVC task manager for small business teams.
Start with [README.md](README.md) for the app and [roadmap/ROADMAP.md](roadmap/ROADMAP.md) for what is planned and
where it stands.

## Roadmap rules

The roadmap follows the Yardgoat Caboose convention (https://yardgoat.ai/caboose.md, CC BY 4.0).

1. **Authority.** `roadmap/ROADMAP.md` owns plan and status only. Put details in plan files, decision records or docs,
   and link them. Don't copy them into the roadmap.
2. **Birth.** A new plan, phase or wave goes into the §4 register in the same commit that creates its file.
3. **Maintenance.** When a plan changes state, update its plan file and the §4 register in the same commit.
4. Use only the nine status words in ROADMAP.md §0.
5. Record a significant technical choice as a new `DR-nnnn` in `roadmap/decisions/` and list it in its INDEX.
6. Record Koda's instructions word for word as `OV-nnn` in `roadmap/rulings/`. Never edit a ruling; add a new one.
7. Add notable changes to `CHANGELOG.md`.
8. Don't start work on a plan until Koda says to.

## Before every commit

```bash
dotnet test                              # all tests pass
python3 scripts/roadmap-lint.py          # roadmap rules hold
python3 scripts/roadmap-lint.py --self-test   # after changing the lint script
```

CI runs the same checks on every push and pull request.

## Course rules

- Keep scope to the current sprint's assignment. Don't add features just to make the app look bigger.
- Reflections and other graded writing are Koda's own words. Help with material, not the writing.
- Commit messages: a short summary line in the imperative, then why the change was made.
