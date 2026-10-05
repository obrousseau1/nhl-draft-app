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

### Capability: deployment

#### Purpose

Builds and tests every change on GitHub, and publishes the app to an Azure Web App reachable only by the host, alongside the local offline run.

#### ADDED Requirements

##### Requirement: Continuous integration
The system SHALL build and test the API and lint, typecheck and test the web app on every push and pull request to `main`.

###### Scenario: Pull request checked
- **WHEN** a pull request targets `main`
- **THEN** a GitHub Actions run builds and tests both, and a failure marks the pull request red

##### Requirement: Continuous deployment to Azure
The system SHALL deploy to the Azure Web App on every push to `main` whose build and tests pass.

###### Scenario: Merge deploys
- **WHEN** a commit lands on `main` and CI passes
- **THEN** the Azure Web App serves that commit's build

###### Scenario: Red build not deployed
- **WHEN** CI fails on `main`
- **THEN** nothing is deployed

##### Requirement: Azure access limited to the host
The Azure-hosted app SHALL require a login for every page and API call and admit only the host's account.

###### Scenario: Anonymous visitor
- **WHEN** someone opens the Azure URL without logging in
- **THEN** they are sent to the login page and see no draft data

###### Scenario: Other account
- **WHEN** a logged-in account other than the host's opens the Azure URL
- **THEN** access is denied

##### Requirement: Azure draft persists
The Azure-hosted draft SHALL survive an app restart and a redeploy.

###### Scenario: Redeploy mid-season
- **WHEN** picks exist on Azure and a new commit is deployed
- **THEN** the board shows the same picks

##### Requirement: Local run never needs Azure
The local run SHALL never need the Azure app; the Azure draft only changes through pushes from the local app (see draft-sync).

###### Scenario: Internet down on draft night
- **WHEN** the host runs the app locally with no internet
- **THEN** the draft works fully, regardless of the Azure app

### Capability: draft-sync

#### Purpose

Keeps the Azure app as a read-only mirror of the local draft: the local app pushes every change when online, and drafting never depends on the connection.

#### ADDED Requirements

##### Requirement: Automatic push when online
The local app SHALL push the full draft to the Azure app within 30 seconds of any change while a connection is available and sync is configured.

###### Scenario: Pick pushed
- **WHEN** the host makes a pick while online
- **THEN** the Azure board shows that pick within 30 seconds

##### Requirement: Offline changes pushed later
The local app SHALL keep working with no connection, mark the draft as pending, and push the latest draft once the connection returns.

###### Scenario: Back online
- **WHEN** 12 picks are made offline and the connection returns
- **THEN** the Azure board shows all 12 picks without any host action

##### Requirement: Older snapshot refused
The Azure app SHALL refuse a pushed draft whose revision is not newer than the one it holds.

###### Scenario: Stale laptop copy
- **WHEN** Azure holds revision 40 and a push carries revision 35
- **THEN** the push is refused and Azure keeps revision 40

##### Requirement: Azure is a read-only mirror
The Azure app SHALL refuse every draft change except a pushed snapshot, and show when it was last updated.

###### Scenario: Edit on Azure
- **WHEN** the host tries to make a pick on the Azure app
- **THEN** the change is refused with a message that Azure is a read-only mirror

##### Requirement: Sync status visible locally
The local app SHALL report the sync state as synced, pending, or failing with its reason, including when the host must log in.

###### Scenario: Login needed
- **WHEN** the cached login has expired
- **THEN** the status says login is needed and shows the device-login code, and picks keep working

##### Requirement: Host logs in once
The local app SHALL authenticate to Azure as the host with a device-code login and reuse the cached login across restarts until it expires.

###### Scenario: Restart keeps login
- **WHEN** the host logged in yesterday and restarts the local app
- **THEN** pushes resume without a new login

##### Requirement: Sync is optional
The local app SHALL behave exactly as without sync when no Azure address is configured.

###### Scenario: No Azure configured
- **WHEN** no Azure address is set
- **THEN** no push is attempted and the status reports sync off

## Brittle and to watch

- **Relative ProjectReference to `X:\nhl-fantasy-draft\src\NhlDraftKit.Core`** — any rename, move or signature change in the old repo (`KitReader.Read`, `Player`, `TeamRow`, `Teams.FromKit`) breaks this build. A fresh clone of this repo alone does not build.
- **`KitReader` is owned by the other app** — it may evolve for season-prep needs (e.g. new required headers) and start rejecting kits this app could read.
- **Pool snapshot (design D3)** — picks reference `PoolEntry.Id`; if Id generation changes between versions, an existing `draft.json` loses its picks' links.
- **Comment-only `.tsx` stubs** — lint/typecheck/test must pass on files with no code; `vitest --passWithNoTests` hides a real "no tests" state once the user starts writing code.
- **Dev port 5190 hard-coded in two places** (`launchSettings.json`, `vite.config.ts`).
- **CI checks out the private `nhl-fantasy-draft` side by side** with a token secret; an expired token or a renamed repo turns every run red at checkout.
- **JSON file + in-process lock assume one Azure instance** — scale-out would corrupt or split the draft.
- **Every push to `main` deploys and restarts the Azure app** — safe only because restart restores state.
- **Revision counter in `draft.json`** — Azure trusts it to order snapshots; a reset or a restored old file must never lower it, or every push is refused.
- **Easy Auth now validates bearer tokens** — allowed audience and the Entra public-client flag are hand-set; a change in the portal silently stops sync (status turns failing).

## Biggest risks

1. **Reachability rule wrong in a corner case** (replacement of a G/Team/D box, D beyond minimum, last round). Likely enough — it is the only non-trivial arithmetic. Visible only mid-draft when a legal pick is refused or an illegal one passes. → EC-7, EC-8.
2. **Draft state lost mid-draft** (crash during write, wrong path, StrictMode double request). Unlikely with atomic replace, catastrophic if it happens. → EC-11.
3. **Kit next season or this kit's edge rows fail import** (goalie without CapH, team mapping for `Uta`, `Vgk`). Visible at import, before the draft — the cheap time. → EC-1, EC-17.
4. **Draft night needs internet or two terminals** (dev mode left as only run path, CDN asset). Visible only on the night. → EC-12.
5. **Azure draft lost on redeploy** (data file written outside `/home`). Likely if the path setting is missed; visible only after a deploy. → EC-24, EC-26.
6. **Azure app open to anyone** (Easy Auth misconfigured, assignment not required). Public repo makes the URL guessable. → EC-25.
7. **Sync silently stops** (token expired, audience wrong, stale revision) and Azure shows an old board. Likely at least once; visible only through the sync status. → EC-28, EC-30.

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
- **Manual deploy trigger** — user chose deploy on every push to `main`.
- **Easy Auth "login only to edit" / shared access code** — needs app code; one host device, so login always, host only.
- **Publish profile for deploy** — basic auth is off by default on new App Services; OIDC instead, no long-lived secret.
- **Manual export/import sync** — user wants automatic push.
- **Two-way sync with an Azure database (Cosmos DB / Azure SQL)** — conflict handling and change tracking would outweigh the app; one-way push, no database.
- **Editable Azure, last push wins** — silently loses Azure-side edits; Azure is a read-only mirror.

## Exit conditions

```json
{
  "version": 1,
  "exitConditions": [
    { "id": "EC-1", "description": "Kit import tests pass: skaters, goalies and teams loaded; `°` stripped; missing CapH → 0 with NoCapHit; teams cap 0; bad file refused naming the missing column with pool unchanged.", "verified": false },
    { "id": "EC-2", "description": "Import after a pick is refused (409) and pool and picks are unchanged, shown by an endpoint test.", "verified": false },
    { "id": "EC-3", "description": "Setup tests pass: defaults 20 / 104 / 2 / 2 / 2; inconsistent settings rejected; duplicate or empty pooler name rejected; shuffle and manual reorder change round-1 order.", "verified": true },
    { "id": "EC-4", "description": "Setup lock tests pass: rounds, cap, counts, poolers and order refused after a pick (409); rename still allowed; reset clears picks only and unlocks setup.", "verified": true },
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
    { "id": "EC-18", "description": "Complexity is low: no method over ~30 lines, nesting ≤ 3, controller actions only map HTTP ↔ domain (no rule logic in controllers), main pick path readable from the controller action into PickRules without other hops.", "verified": false },
    { "id": "EC-19", "description": "As lean as possible: no interface unless a test substitutes it with NSubstitute, no repository/mediator layer, no package beyond Core's ClosedXML, MSAL (+ its cache extension) and the test stack, no setting the specs do not name; clean-code skill audit on the new C# reports no Major finding.", "verified": false },
    { "id": "EC-20", "description": "Unit tests use xUnit, FluentAssertions and NSubstitute, and every test method is named Method_When_Condition_Should_Expectation with an underscore between each of the 5 sections, and repeated test values are private constants or fields (checked by grep `_When_[A-Za-z]+_Should_` over `api/NhlDraftApp.Api.Tests`).", "verified": false },
    { "id": "EC-21", "description": "GitHub Actions `dotnet` job builds and tests `NhlDraftApp.slnx` on every push and PR to main, with nhl-fantasy-draft checked out side by side; a green run is visible on GitHub.", "verified": false },
    { "id": "EC-22", "description": "GitHub Actions `web` job runs pnpm lint, typeCheck and test on every push and PR to main; a failing step fails the run.", "verified": false },
    { "id": "EC-23", "description": "The CI publish artifact contains the API and `wwwroot/index.html` from the built web app.", "verified": false },
    { "id": "EC-24", "description": "The data file path comes from config `Draft:DataPath` (default under %LOCALAPPDATA%); a test proves the override is used.", "verified": true },
    { "id": "EC-25", "description": "On Azure, an anonymous request is redirected to login and a non-host account is denied; checked by hand in a private browser window.", "verified": false },
    { "id": "EC-26", "description": "A push to main with green CI deploys to Azure; picks made on Azure survive an app restart and the next deploy; a red CI run deploys nothing.", "verified": false },
    { "id": "EC-27", "description": "Push tests pass (fake HTTP handler, substituted token source): a change is pushed; offline changes mark pending and the latest draft is pushed when the connection returns; no Azure address → status off and no push.", "verified": false },
    { "id": "EC-28", "description": "Mirror tests pass: Azure in mirror mode refuses a snapshot whose revision is not newer (409) and refuses every other write (403 with reason); revision never decreases, including after reset.", "verified": false },
    { "id": "EC-29", "description": "GET /api/draft reports sync status off, synced, pending or failing with reason and last pushed time; an expired login reports 'login needed' with the device code while picks still succeed.", "verified": false },
    { "id": "EC-30", "description": "End to end by hand: picks made with the network off appear on the Azure board within a minute of reconnecting, and a restart of the local app keeps the login.", "verified": false }
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

## 9. CI/CD and Azure

- [ ] 9.1 Add `.github/workflows/ci.yml` `dotnet` job (checkout this repo + private Core repo side by side via `CORE_REPO_TOKEN`, build + test slnx) on push and PR to `main`; verify a green run on GitHub Actions
- [ ] 9.2 Add `web` job (pnpm install, lint, typeCheck, test) and a publish step that copies `web/dist` into the API publish `wwwroot`; verify a green run and that the publish artifact contains `wwwroot/index.html`
- [ ] 9.3 * Create Azure resources by hand (Linux App Service F1 .NET 10, app setting `Draft__DataPath=/home/data/draft.json`, Entra app with federated credential for this repo's `main`, Easy Auth require login + assignment required, host only) following README steps; verify the README steps are complete
- [ ] 9.4 Add deploy job (`needs` CI jobs, push to `main` only, OIDC `azure/login`, `azure/webapps-deploy`); verify a push to `main` deploys and the Azure URL serves the app after login
- [ ] 9.5 Verify Azure access and persistence: anonymous visit redirected to login, other account denied, picks survive restart and redeploy

## 10. Sync local to Azure mirror

- [ ] 10.1 Add `Revision` to the draft (bumped on every save) and Azure mirror mode (`Sync:Mode=Mirror`: snapshot accepted only if newer, else 409; all other writes 403); verify tests cover the stale-snapshot and edit-on-Azure scenarios
- [ ] 10.2 Add the local push service (wake on save, bearer token, backoff retry) and sync status in `GET /api/draft`; verify tests with a fake handler and substituted token source cover push, offline-then-back, failing reason and sync off
- [ ] 10.3 Add MSAL device-code token source with persistent cache; verify a test that an expired login sets status "login needed" with the code while picks still succeed
- [ ] 10.4 * Configure Entra (expose `access_as_user`, allow public client flows) and Easy Auth allowed audience, documented in README; verify README steps are complete
- [ ] 10.5 Verify end to end: picks made offline appear on the Azure board within a minute of reconnecting; a restart keeps the login

## Changelog

- **2026-10-05 — Creation** — written by /build-specs from intent.md.
- **2026-10-05 — CI/CD and Azure** — added `deployment` capability, EC-21 to EC-26, task group 9 (user request).
- **2026-10-05 — Sync to Azure** — added `draft-sync` capability (auto-push, read-only mirror), EC-27 to EC-30, task group 10; deployment "independent drafts" requirement reworded (user request).
- **2026-10-05 — Build, shred 1** — EC-3, EC-4, EC-24 verified (36 tests green, 0 warnings). EC-21 written (`.github/workflows/ci.yml`) but unverified until the branch is pushed and a run goes green. Setup refusals return 400 `{ reason }` (invalid), 409 (locked), 404 (unknown pooler); 422 stays reserved for pick refusals. NSubstitute is used to substitute `Random` for the shuffle.
- **2026-10-05 — Build, shred 1** — endpoints moved from minimal API to `[ApiController]` controllers (user decision); routes, status codes and bodies unchanged, EC-18 reworded. Malformed JSON bodies now get ProblemDetails 400 from `[ApiController]`; domain refusals keep `{ reason }`.
