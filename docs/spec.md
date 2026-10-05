# Live NHL pool draft board

## Specs

### Capability: kit-import


#### Purpose

Loads the draftable pool — skaters, goalies and NHL teams with their cap hits — from the PoolExpert Draft Kit XLSX before the draft starts.

#### ADDED Requirements

##### Requirement: Import Draft Kit
The system SHALL import a Draft Kit XLSX and replace the draftable pool with its skaters (C, LW, RW, D), goalies (G) and NHL teams.

###### Scenario: Valid kit imported
- **WHEN** the host uploads the 2026-2027 Draft Kit before any pick
- **THEN** the pool holds 918 skaters, 93 goalies and 32 teams, each skater and goalie with first name, last name, NHL team, position and cap hit

###### Scenario: Name markers stripped
- **WHEN** a last name in the kit ends with `°`
- **THEN** the imported name has no `°`

##### Requirement: Missing cap hit counts as zero
The system SHALL import a player with no cap hit as pickable with a cap hit of 0 and mark them as having no cap hit.

###### Scenario: Goalie without cap hit
- **WHEN** the kit row for a goalie has an empty cap hit
- **THEN** the goalie is in the pool with cap hit 0 and a "no cap hit" marker

##### Requirement: Teams have no cap hit
The system SHALL import NHL teams with a cap hit of 0.

###### Scenario: Team cap
- **WHEN** a team is imported
- **THEN** its cap hit is 0

##### Requirement: Reject unreadable kit
The system SHALL reject a file that is not a readable Draft Kit and keep the current pool unchanged, reporting which required column or sheet is missing.

###### Scenario: Wrong file
- **WHEN** the host uploads an XLSX without a `CapH` column
- **THEN** the import fails with a message naming the missing column and the previous pool is kept

##### Requirement: Import only before the draft starts
The system SHALL refuse an import once at least one pick exists.

###### Scenario: Import after first pick
- **WHEN** one pick exists and the host uploads a kit
- **THEN** the import is refused and the pool and picks are unchanged

### Capability: draft-setup


#### Purpose

Lets the host configure a draft before it starts: who drafts, in which order, for how many rounds, and under which cap and roster counts.

#### ADDED Requirements

##### Requirement: Draft settings with defaults
The system SHALL let the host set the number of rounds, the salary cap, the defensemen minimum, the goalie count and the team count, defaulting to 20 rounds, 104 M$, 2, 2 and 2.

###### Scenario: Fresh draft defaults
- **WHEN** a new draft is created
- **THEN** settings are 20 rounds, 104 M$ cap, D minimum 2, goalies 2, teams 2

##### Requirement: Settings are consistent
The system SHALL reject settings where the D minimum plus the goalie count plus the team count exceeds the number of rounds, where any value is negative, or where rounds or cap is zero.

###### Scenario: Too many required picks
- **WHEN** the host sets 5 rounds with D 2, G 2, teams 2
- **THEN** the settings are rejected with a message

##### Requirement: Poolers
The system SHALL let the host add, remove and rename poolers; names MUST be non-empty and unique.

###### Scenario: Duplicate name
- **WHEN** the host adds a pooler named like an existing one
- **THEN** the change is rejected

##### Requirement: Draft order
The system SHALL let the host shuffle the pooler order randomly or reorder poolers manually; the order is round 1's pick order.

###### Scenario: Random draw
- **WHEN** the host shuffles 12 poolers
- **THEN** the board shows the 12 poolers in a new random order

##### Requirement: Setup locked once the draft starts
The system SHALL refuse changes to rounds, cap, counts, pooler list and order once at least one pick exists; renaming a pooler SHALL stay allowed.

###### Scenario: Change rounds mid-draft
- **WHEN** a pick exists and the host changes rounds
- **THEN** the change is refused

###### Scenario: Rename mid-draft
- **WHEN** a pick exists and the host renames a pooler
- **THEN** the name changes and picks are kept

##### Requirement: Reset draft
The system SHALL let the host clear all picks after an explicit confirmation, keeping settings, poolers and the imported pool.

###### Scenario: Reset
- **WHEN** the host confirms reset
- **THEN** every box is empty and setup is unlocked

### Capability: draft-board


#### Purpose

The live board the room watches: every pooler's picks per round, whose turn it is, and each pooler's cap and roster needs, kept safe across restarts.

#### ADDED Requirements

##### Requirement: Board grid
The system SHALL show one row per pooler in draft order and one column per round; each box shows the picked player's name, position, NHL team and cap hit, or the picked team's name, or is empty.

###### Scenario: Filled box
- **WHEN** Alex picked Connor McDavid in round 1
- **THEN** Alex's round 1 box shows "Connor McDavid", C, EDM, 12.5

##### Requirement: Snake order current pick
The system SHALL highlight the current pick: the first empty box in snake order, where odd rounds follow the draft order and even rounds reverse it.

###### Scenario: Round 2 reverses
- **WHEN** poolers A, B, C have all picked in round 1 and nobody in round 2
- **THEN** C's round 2 box is highlighted

###### Scenario: Skipped box stays current
- **WHEN** B's round 1 box is empty and C's round 1 box is filled
- **THEN** B's round 1 box is highlighted

##### Requirement: Any empty box can be filled
The system SHALL let the host pick into any empty box, not only the highlighted one.

###### Scenario: Out of turn
- **WHEN** the current pick is A round 3 and the host picks for D round 3
- **THEN** the pick is saved and A round 3 stays highlighted

##### Requirement: Replace or clear a pick
The system SHALL let the host replace or clear a filled box; a cleared or replaced player returns to the pool.

###### Scenario: Clear
- **WHEN** the host clears Alex's round 1 box holding McDavid
- **THEN** the box is empty and McDavid is searchable again

##### Requirement: Pooler totals
The system SHALL show per pooler: cap remaining (cap minus picked cap hits), average cap per remaining player pick (cap remaining ÷ (empty boxes − teams still needed), since teams cost no cap; blank when that is 0), and defensemen, goalies and teams still needed.

###### Scenario: Totals after two picks
- **WHEN** cap is 104, 20 rounds, and Alex picked McDavid (12.5, C) and a D at 3.5
- **THEN** Alex shows cap remaining 88.0, average 5.5 (88 ÷ 16: 18 empty boxes minus 2 team picks), D needed 1, G needed 2, teams needed 2

##### Requirement: Draft survives restarts
The system SHALL save settings, poolers, order, pool and picks after every change and restore them when the app or the browser restarts.

###### Scenario: Laptop restart
- **WHEN** 37 picks are made and the app is stopped and started again
- **THEN** the board shows the same 37 picks, totals and current pick

##### Requirement: Works offline
The system SHALL run entirely on the host machine without internet access.

###### Scenario: No network
- **WHEN** the laptop has no network connection
- **THEN** import, setup, picking and the board all work

### Capability: pick-rules


#### Purpose

Decides whether a player or team can go into a given pooler's box, so illegal picks are blocked before they reach the board.

#### ADDED Requirements

##### Requirement: One owner per player and team
The system SHALL refuse a player or team already picked in another box.

###### Scenario: Taken player
- **WHEN** McDavid is in Alex's box and the host picks McDavid for Sam
- **THEN** the pick is refused with a message naming the owner

##### Requirement: Cap not exceeded
The system SHALL refuse a pick whose cap hit is greater than the pooler's cap remaining, counting a replaced box's player as removed.

###### Scenario: Over cap
- **WHEN** Sam has 3.0 remaining and the host picks a 4.0 player
- **THEN** the pick is refused

###### Scenario: Replacement frees cap
- **WHEN** Sam has 1.0 remaining and replaces a 5.0 player with a 5.5 player
- **THEN** the pick is accepted

##### Requirement: Goalie and team counts are exact maxima
The system SHALL refuse a goalie or team pick that would exceed the goalie or team count.

###### Scenario: Third goalie
- **WHEN** Sam has 2 goalies and the count is 2
- **THEN** a third goalie is refused

##### Requirement: Defensemen beyond minimum allowed
The system SHALL accept defensemen beyond the D minimum, subject to the other rules.

###### Scenario: Third defenseman
- **WHEN** Sam has 2 D and 10 empty boxes
- **THEN** a third D is accepted

##### Requirement: Required picks stay reachable
The system SHALL refuse a pick that leaves fewer empty boxes than the pooler's defensemen, goalies and teams still needed.

###### Scenario: Forward with goalies missing
- **WHEN** Sam has 2 empty boxes, needs 2 goalies, and the host picks a forward
- **THEN** the pick is refused with a message saying goalies are still needed

##### Requirement: Refusal reason
The system SHALL state the rule that refused a pick.

###### Scenario: Reason shown
- **WHEN** a pick is refused
- **THEN** the host sees which rule blocked it

### Capability: player-search


#### Purpose

Finds an unpicked player or team quickly when the host fills a box during the live draft.

#### ADDED Requirements

##### Requirement: Search unpicked by name
The system SHALL search unpicked players and teams by first or last name, case- and accent-insensitive.

###### Scenario: Accent-insensitive
- **WHEN** the host types "stutzle"
- **THEN** Tim Stützle is listed if unpicked

###### Scenario: Picked player hidden
- **WHEN** McDavid is picked and the host types "mcdavid"
- **THEN** no result is listed

##### Requirement: Filter by position
The system SHALL filter results by C, LW, RW, D, G or Team.

###### Scenario: Goalies only
- **WHEN** the host filters on G
- **THEN** only unpicked goalies are listed

##### Requirement: Filter by NHL team
The system SHALL filter results by NHL team.

###### Scenario: Oilers only
- **WHEN** the host filters on EDM
- **THEN** only unpicked Oilers players, and the Oilers team if unpicked, are listed

##### Requirement: Eligible only
The system SHALL, when "eligible only" is on, hide results the pick rules would refuse for the box being filled.

###### Scenario: Unaffordable hidden
- **WHEN** Sam has 3.0 remaining and "eligible only" is on
- **THEN** players with a cap hit above 3.0 are not listed

##### Requirement: Result details
The system SHALL show for each result its name, position, NHL team and cap hit, with the "no cap hit" marker when applicable.

###### Scenario: No cap hit player
- **WHEN** a player without cap hit in the kit is listed
- **THEN** it shows cap 0 with the marker

### Capability: roster-view


#### Purpose

Shows each pooler's roster in a form that is quick to re-type into PoolExpert, which has no import.

#### ADDED Requirements

##### Requirement: Per-pooler roster
The system SHALL list, for a chosen pooler, every pick grouped as forwards, defensemen, goalies and teams, each line showing full name, NHL team and cap hit, plus the total cap used.

###### Scenario: Roster of Alex
- **WHEN** the host opens Alex's roster
- **THEN** picks are grouped Forwards, Defensemen, Goalies, Teams, and the total cap used is shown

##### Requirement: Available during and after the draft
The system SHALL show the roster view at any time, including partial rosters.

###### Scenario: Partial roster
- **WHEN** Alex has 5 picks
- **THEN** the roster lists those 5

## Brittle and to watch

- **Relative ProjectReference to `X:\nhl-fantasy-draft\src\NhlDraftKit.Core`** — any rename, move or signature change in the old repo (`KitReader.Read`, `Player`, `TeamRow`, `Teams.FromKit`) breaks this build. A fresh clone of this repo alone does not build.
- **`KitReader` is owned by the other app** — it may evolve for season-prep needs (e.g. new required headers) and start rejecting kits this app could read.
- **Pool snapshot (design D3)** — picks reference `PoolEntry.Id`; if Id generation changes between versions, an existing `draft.json` loses its picks' links.
- **Comment-only `.tsx` stubs** — lint/typecheck/test must pass on files with no code; `vitest --passWithNoTests` hides a real "no tests" state once the user starts writing code.
- **Dev port 5190 hard-coded in two places** (`launchSettings.json`, `vite.config.ts`).

## Biggest risks

1. **Reachability rule wrong in a corner case** (replacement of a G/Team/D box, D beyond minimum, last round). Likely enough — it is the only non-trivial arithmetic. Visible only mid-draft when a legal pick is refused or an illegal one passes. → EC-7, EC-8.
2. **Draft state lost mid-draft** (crash during write, wrong path, StrictMode double request). Unlikely with atomic replace, catastrophic if it happens. → EC-11.
3. **Kit next season or this kit's edge rows fail import** (goalie without CapH, team mapping for `Uta`, `Vgk`). Visible at import, before the draft — the cheap time. → EC-1, EC-17.
4. **Draft night needs internet or two terminals** (dev mode left as only run path, CDN asset). Visible only on the night. → EC-12.

## Discarded options and why

- **Git submodule / local NuGet / extract Core repo** — more friction than value for a one-laptop app; user chose relative ProjectReference after pros/cons.
- **SQLite / EF Core persistence** — a ~100 KB single document with no queries; JSON file with atomic replace is enough.
- **Rules in the frontend** — the user is learning React; keeping logic in tested C# keeps the frontend to display and calls.
- **Multi-device / per-pooler phones** — ruled out in intent; one host device.
- **Turn locking** — user wants highlight only.
- **Commented-code React stubs** — user writes the code; only comments.
- **Reusing Core's `DraftSession`** — it is season-prep (ranking, stats sync), not a live draft.
- **Export file for PoolExpert** — PoolExpert has no documented import; roster view instead.
- **Re-import / merge mid-draft** — ruled out in intent.

## Exit conditions

```json
{
  "version": 1,
  "exitConditions": [
    { "id": "EC-1", "description": "Kit import tests pass: skaters, goalies and teams loaded; `°` stripped; missing CapH → 0 with NoCapHit; teams cap 0; bad file refused naming the missing column with pool unchanged.", "verified": false },
    { "id": "EC-2", "description": "Import after a pick is refused (409) and pool and picks are unchanged, shown by an endpoint test.", "verified": false },
    { "id": "EC-3", "description": "Setup tests pass: defaults 20 / 104 / 2 / 2 / 2; inconsistent settings rejected; duplicate or empty pooler name rejected; shuffle and manual reorder change round-1 order.", "verified": false },
    { "id": "EC-4", "description": "Setup lock tests pass: rounds, cap, counts, poolers and order refused after a pick (409); rename still allowed; reset clears picks only and unlocks setup.", "verified": false },
    { "id": "EC-5", "description": "Board state test: GET /api/draft returns one row per pooler in order, one box per round, filled boxes carrying name, position, NHL team and cap hit.", "verified": false },
    { "id": "EC-6", "description": "Snake tests pass: round 2 reverses order, a skipped earlier box stays current, an out-of-turn pick is saved without moving the current pick.", "verified": false },
    { "id": "EC-7", "description": "Pick rule tests pass: taken player refused naming owner; over cap refused; replacement frees cap; 3rd G / 3rd team refused; 3rd D accepted.", "verified": false },
    { "id": "EC-8", "description": "Reachability tests pass: forward refused when empty boxes < D+G+T still needed, including when replacing a G, Team or D box; each refusal returns its rule's reason (422).", "verified": false },
    { "id": "EC-9", "description": "Replace and clear tests pass: cleared or replaced entry is searchable again; a refused pick leaves state unchanged.", "verified": false },
    { "id": "EC-10", "description": "Totals test passes: cap 104, 20 rounds, picks 12.5 C + 3.5 D → remaining 88.0, average 5.5 (88 ÷ (18 empty − 2 teams needed)), D 1, G 2, teams 2; average blank when only team picks remain.", "verified": false },
    { "id": "EC-11", "description": "Restart test passes: picks made, host restarted on the same data file, GET /api/draft returns identical picks, totals and current pick; writes go through a temp file then replace.", "verified": false },
    { "id": "EC-12", "description": "Offline run: with `web/dist` built, `dotnet run --project api/NhlDraftApp.Api` alone serves the app at `/` with network disabled, and index.html references no external URL.", "verified": false },
    { "id": "EC-13", "description": "Search tests pass: 'stutzle' finds Stützle; picked entries hidden; position (incl. Team) and NHL team filters; eligible-only hides entries PickRules refuses; NoCapHit marker present.", "verified": false },
    { "id": "EC-14", "description": "Roster tests pass: picks grouped Forwards/Defensemen/Goalies/Teams with name, NHL team, cap hit and total cap; partial roster lists only existing picks.", "verified": false },
    { "id": "EC-15", "description": "Frontend stubs: `src/*.ts(x)` contain comments only; in `web`, `pnpm run lint`, `pnpm run typeCheck` and `pnpm run test` exit 0.", "verified": false },
    { "id": "EC-16", "description": "`dotnet build NhlDraftApp.slnx` exits 0 with no warnings and `dotnet test NhlDraftApp.slnx` passes (build implied).", "verified": false },
    { "id": "EC-17", "description": "Real kit `draftkit  Joueurs gardiens équipes - maj 2026-09-30.xlsx` imported through the running API yields 918 skaters, 93 goalies, 32 teams.", "verified": false },
    { "id": "EC-18", "description": "Complexity is low: no method over ~30 lines, nesting ≤ 3, endpoints only map HTTP ↔ domain (no rule logic in endpoint lambdas), main pick path readable from the endpoint into PickRules without other hops.", "verified": false },
    { "id": "EC-19", "description": "As lean as possible: no interface unless a test substitutes it with NSubstitute, no repository/mediator layer, no package beyond Core's ClosedXML and the test stack, no setting the specs do not name; clean-code skill audit on the new C# reports no Major finding.", "verified": false },
    { "id": "EC-20", "description": "Unit tests use xUnit, FluentAssertions and NSubstitute, and every test method is named Method_When_Condition_Should_Expectation (checked by grep over `api/NhlDraftApp.Api.Tests`).", "verified": false }
  ]
}
```

## Proposed task list

# Tasks

## 1. Backend scaffold

- [ ] 1.1 Create `NhlDraftApp.slnx`, `api/NhlDraftApp.Api` (net10.0 minimal API, port 5190) and `api/NhlDraftApp.Api.Tests` (xUnit, FluentAssertions, NSubstitute); verify `dotnet build NhlDraftApp.slnx` exits 0
- [ ] 1.2 Add relative ProjectReference to `X:\nhl-fantasy-draft\src\NhlDraftKit.Core`; verify build exits 0 and a test can call `KitReader`

## 2. Draft state and persistence

- [ ] 2.1 Add `Draft` state (settings with defaults, poolers, order, pool entries, picks) and JSON store with atomic write to a configurable path; verify tests for defaults and save → load round-trip
- [ ] 2.2 Add setup mutations: settings validation, add/rename/remove pooler, reorder, shuffle, reset, lock once a pick exists (rename allowed); verify tests cover each `draft-setup` scenario

## 3. Kit import

- [ ] 3.1 Map `Kit` to pool entries (skaters, goalies, teams as `Team` with cap 0, missing CapH → 0 + NoCapHit, `°` stripped) and refuse import after first pick; verify tests using an XLSX built in-test with ClosedXML cover every `kit-import` scenario
- [ ] 3.2 Add `POST /api/kit` (multipart → temp file → read → delete); verify an endpoint test returns 409 after a pick and 400 with the missing column name on a bad file

## 4. Pick rules, snake, totals

- [ ] 4.1 Implement `PickRules.Check` (owner, cap with replacement, G/Team maxima, D beyond minimum, reachability, reason text); verify tests cover every `pick-rules` scenario
- [ ] 4.2 Implement `Snake.Current` and `PoolerTotals.Of`; verify tests cover round-2 reversal, skipped box, out-of-turn pick and the totals scenario (88.0 / 5.5 / 1 / 2 / 2)
- [ ] 4.3 Add pick, replace, clear mutations using `PickRules`; verify tests for clear returning the player to the pool and refused picks leaving state unchanged

## 5. Search and roster

- [ ] 5.1 Implement search (accent-insensitive name, position, NHL team, eligible-only via `PickRules`); verify tests cover every `player-search` scenario including "stutzle"
- [ ] 5.2 Implement per-pooler roster grouped Forwards/Defensemen/Goalies/Teams with total cap; verify tests cover full and partial roster

## 6. HTTP endpoints and offline hosting

- [ ] 6.1 Map all endpoints of design D8 with 422/409 `{ reason }` responses; verify endpoint tests (WebApplicationFactory) for one success and one refusal per route group
- [ ] 6.2 Serve `web/dist` with SPA fallback when present; verify an endpoint test that `/` returns the index file from a temp dist folder
- [ ] 6.3 Write `NhlDraftApp.Api.http` with one example per endpoint; verify each request runs against a started API
- [ ] 6.4 Verify restart scenario: make picks, restart the host in a test, `GET /api/draft` returns the same picks and current pick

## 7. Frontend scaffold (config real, source comments only)

- [ ] 7.1 Create `web/package.json`, `index.html`, `vite.config.ts` (proxy `/api` → 5190, vitest jsdom), `tsconfig*.json`, `.oxlintrc.json` from first-reactor; verify `pnpm install` succeeds
- [ ] 7.2 Create comment-only `src/main.tsx`, `src/App.tsx` (component list, API calls, concepts, build order), `src/test-setup.ts`; verify `pnpm run lint`, `pnpm run typeCheck`, `pnpm run test` exit 0

## 8. Docs and integration

- [ ] 8.1 Write `README.md` (setup, dev two-terminal run, draft-night single run, Core sibling-folder requirement and commit, frontend is yours); verify commands run as written
- [ ] 8.2 Import the real 2026-2027 kit through the running API; verify `GET /api/draft` reports 918 skaters, 93 goalies, 32 teams

## Changelog

- **2026-10-05 — Creation** — written by /build-specs from intent.md.
