# Run our live NHL pool draft from one laptop, rules enforced, offline

## What is happening
Our pool drafts live with friends. Tracking picks, remaining cap and roster
rules by hand is slow and error-prone: someone goes over the cap, misses a
goalie, or picks an already-taken player. Results then get re-typed into
PoolExpert for the season.

## Who it affects, and when
The host (pool admin) running the yearly draft on one laptop, board shown to
~12 poolers in the room. Every draft; next one for season 2026-2027.

## How to see it
Run a draft today: no single board shows, per pooler, picks per round, cap
remaining, average cap left per remaining round, and D/G/team picks still
needed. Nothing stops an illegal pick.

Expected board: poolers as rows (in draft order), rounds as columns. Click a
box → search unpicked players/teams → assign. Per pooler: cap remaining,
cap remaining ÷ rounds remaining, D still needed, G still needed, teams still
needed.

Edge cases that must hold:
- Pooler with 2 rounds left and 0 goalies picks a forward → blocked.
- Pick whose cap hit exceeds cap remaining → blocked.
- 3rd goalie or 3rd team → blocked (exact 2). 3rd D → allowed (minimum 2).
- Player with no CapH in the file → pickable at 0, flagged.
- Wrong pick → click filled box, replace or clear; player back in pool.
- Browser refresh / laptop restart mid-draft → resumes exactly.
- Team already picked by another pooler → not offered.

## Is it isolated
New app, empty repo. Related: `x:\nhl-fantasy-draft` (MAUI draft-prep kit)
already parses the same XLSX (`src\NhlDraftKit.Core\KitReader.cs`,
ClosedXML; team-code mapping in `Teams.cs`). This app is the live draft
board it lacks.

## Constraints
- Web app, works without internet: local C# API + browser on host laptop.
- Also deployed to an Azure Web App (CI/CD on GitHub Actions), host-only login. Local stays the draft-night path and pushes every change to Azure when online; Azure is a read-only mirror.
- C# backend; persists full draft state after every pick.
- React/TypeScript frontend written by the user (learning); only minimal
  stub files with beginner comments, first-reactor style.
- Player source: PoolExpert Draft Kit XLSX, sheets `Joueurs` (skaters,
  C/LW/RW/D), `Gardiens` (G), `Équipes` (32 teams, no cap). Header on row 4,
  merged group row above, last name in unlabeled column after `Nom`, `°`
  markers, `CapH` in M$ (nullable).
- XLSX parsing shared with nhl-fantasy-draft as a library.
- Host-editable at setup: poolers + order (snake, randomly drawn), rounds
  (default 20), cap (default 104 M$), D min / G exact / teams exact
  (default 2/2/2).
- Snake order: current pick highlighted, not locked.
- Search: name (accent-insensitive), filter by position, by NHL team, hide
  picks that break that pooler's rules.
- Import only before first pick.
- PoolExpert has no import/API: provide a per-pooler roster view easy to
  re-type into PoolExpert.

## Not this
- Multi-device / per-pooler phones, live sync.
- Two-way sync or editing on Azure.
- Season tracking, scoring, trades (PoolExpert's job).
- Re-import or merge after the draft starts.
- File export to PoolExpert (no import exists).
- Turn locking.

## Open questions
- Shared lib packaging with nhl-fantasy-draft (project ref, NuGet, submodule)?
  → user / build-specs.
- Order draw done in-app or entered manually? → user.
- Does PoolExpert offer a support-assisted bulk import? → PoolExpert support.

## Questions and answers
1. **Q:** Who uses it live? **A:** One host device; offline = local.
2. **Q:** Pick order? **A:** Snake, order set at start (random draw).
3. **Q:** Team pick? **A:** NHL team, no cap hit, from XLSX `Équipes`.
4. **Q:** Rule breach? **A:** Block.
5. **Q:** Cap avg / overrun? **A:** Remaining ÷ rounds left; over cap blocked.
6. **Q:** No CapH? **A:** Pickable at 0.
7. **Q:** Old app? **A:** Shared library.
8. **Q:** Survive restart? **A:** Yes, API persists all.
9. **Q:** Corrections? **A:** Replace/clear any box.
10. **Q:** Out-of-turn picks? **A:** Allowed; current highlighted.
11. **Q:** Settings? **A:** Poolers/order, rounds, cap, D/G/T counts.
12. **Q:** >2 G or teams? **A:** No, exact 2; D minimum.
13. **Q:** Team shared? **A:** No, one owner.
14. **Q:** Re-import? **A:** Before draft only.
15. **Q:** PoolExpert? **A:** No import exists; entry-friendly roster view.
