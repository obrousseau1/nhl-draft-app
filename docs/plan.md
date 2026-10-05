# Plan: Live NHL pool draft board

## Approach

The host needs one offline board that tracks every pooler's picks, cap and roster needs and blocks illegal picks (see `intent.md`). The C# API is built in additive slices: state and setup first, then the web scaffold so the user can start React early against a live API, then the riskiest logic (pick rules), then the brittle Core dependency in its own PR, then read-only search and roster.

## Chunks

### 1. Scaffold the API with draft state, persistence and setup ([#1](https://github.com/obrousseau1/nhl-draft-app/issues/1))

- **Scope** — `NhlDraftApp.slnx`, `api/NhlDraftApp.Api` (Draft state, JSON store with atomic write, setup mutations, setup endpoints, `GET /api/draft`), `api/NhlDraftApp.Api.Tests`, `.http` entries for these endpoints
- **Ships alone because** — runnable API with setup only; nothing depends on it yet
- **Coverage** — xUnit tests for each draft-setup scenario, save → load round-trip, endpoint tests (WebApplicationFactory)
- **Specs** — draft-setup: Draft settings with defaults, Settings are consistent, Poolers, Draft order, Setup locked once the draft starts, Reset draft
- **Risk / brittle** — Risk 2 (state lost mid-draft): atomic temp-file replace, configurable data path; visible in the round-trip test
- **Exit conditions** — EC-3, EC-4

### 2. Add the web scaffold, offline hosting and README ([#2](https://github.com/obrousseau1/nhl-draft-app/issues/2))

- **Scope** — `web/` (`package.json`, `index.html`, `vite.config.ts`, `tsconfig*.json`, `.oxlintrc.json` real; `src/main.tsx`, `src/App.tsx`, `src/test-setup.ts` comments only), static hosting of `web/dist` with SPA fallback in the API, `README.md`
- **Ships alone because** — user's React starting point against chunk 1's live API; API still runs without `dist`
- **Coverage** — `pnpm run lint`, `pnpm run typeCheck`, `pnpm run test` in `web`; endpoint test that `/` serves index from a temp dist; manual run with network off
- **Specs** — draft-board: Works offline
- **Risk / brittle** — comment-only stubs (`--passWithNoTests`), port 5190 in two places, Risk 4 (draft night needs internet or two terminals)
- **Exit conditions** — EC-12, EC-15

### 3. Add pick rules, snake order, totals and picking ([#3](https://github.com/obrousseau1/nhl-draft-app/issues/3))

- **Scope** — `PickRules`, `Snake`, `PoolerTotals`, pick/replace/clear mutations and endpoints, `.http` entries; tests seed an in-memory pool (no XLSX)
- **Ships alone because** — additive on chunk 1; riskiest logic lands early
- **Coverage** — unit test per pick-rules and draft-board scenario, endpoint tests (422 `{ reason }`), restart test on the same data file
- **Specs** — pick-rules: all 6 requirements; draft-board: Board grid, Snake order current pick, Any empty box can be filled, Replace or clear a pick, Pooler totals, Draft survives restarts
- **Risk / brittle** — Risk 1 (reachability corner cases: replacing a G/Team/D box, last round); visible as a refused legal pick or accepted illegal one in tests
- **Exit conditions** — EC-5, EC-6, EC-7, EC-8, EC-9, EC-10, EC-11

### 4. Import the Draft Kit through NhlDraftKit.Core ([#4](https://github.com/obrousseau1/nhl-draft-app/issues/4))

- **Scope** — relative ProjectReference to `X:\nhl-fantasy-draft\src\NhlDraftKit.Core`, Kit → pool entry mapping, `POST /api/kit`, `.http` entry, real-kit check
- **Ships alone because** — one additive endpoint; isolates the brittle cross-repo seam in its own PR
- **Coverage** — tests with an XLSX built in-test via ClosedXML; manual import of the real 2026-2027 kit (918 / 93 / 32)
- **Specs** — kit-import: Import Draft Kit, Missing cap hit counts as zero, Teams have no cap hit, Reject unreadable kit, Import only before the draft starts
- **Risk / brittle** — relative ProjectReference, `KitReader` owned by the other app, pool entry Id stability in `draft.json`, Risk 3 (kit edge rows)
- **Exit conditions** — EC-1, EC-2, EC-17

### 5. Add player search and the roster view ([#5](https://github.com/obrousseau1/nhl-draft-app/issues/5))

- **Scope** — search (accent-insensitive name, position, NHL team, eligible-only via `PickRules`), roster grouping, both endpoints, `.http` entries
- **Ships alone because** — read-only endpoints on top of chunks 1 and 3
- **Coverage** — unit and endpoint tests for each player-search and roster-view scenario
- **Specs** — player-search: all 5 requirements; roster-view: Per-pooler roster, Available during and after the draft
- **Risk / brittle** — none
- **Exit conditions** — EC-13, EC-14

## Task-wide exit conditions

EC-16 (build + tests), EC-18 (complexity), EC-19 (lean + clean-code audit), EC-20 (xUnit / FluentAssertions / NSubstitute, `Method_When_Condition_Should_Expectation`).

## Deliberately not chunks

*(none)* — openspec task 6.3 (`.http` file) is spread across chunks 1, 3, 4, 5; task 8.2 (real kit) rides in chunk 4.

## Changelog

- **2026-10-05 — Creation** — written by /plan-and-shred from intent.md and spec.md.
