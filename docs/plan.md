# Plan: Live NHL pool draft board

## Approach

The host needs one offline board that tracks every pooler's picks, cap and roster needs and blocks illegal picks (see `intent.md`). The C# API is built in additive slices: state and setup first, then the web scaffold so the user can start React early against a live API, then the riskiest logic (pick rules), then the brittle Core dependency in its own PR, then read-only search and roster, then Azure deployment, then the one-way sync that keeps Azure a mirror of the local draft. CI grows with the code: chunk 1 adds the .NET job, chunk 2 the web job and publish bundle, chunk 4 the private Core checkout, chunk 6 the deploy job.

## Chunks

### 1. Scaffold the API with draft state, persistence and setup ([#1](https://github.com/obrousseau1/nhl-draft-app/issues/1))

- **Scope** — `NhlDraftApp.slnx`, `api/NhlDraftApp.Api` (Draft state, JSON store with atomic write, setup mutations, setup endpoints, `GET /api/draft`), `api/NhlDraftApp.Api.Tests`, `.http` entries for these endpoints, data path from config `Draft:DataPath`, `.github/workflows/ci.yml` with the `dotnet` job (build + test)
- **Ships alone because** — runnable API with setup only; nothing depends on it yet
- **Coverage** — xUnit tests for each draft-setup scenario, save → load round-trip, endpoint tests (WebApplicationFactory), data-path override test, green Actions run
- **Specs** — draft-setup: Draft settings with defaults, Settings are consistent, Poolers, Draft order, Setup locked once the draft starts, Reset draft; deployment: Continuous integration (.NET part)
- **Risk / brittle** — Risk 2 (state lost mid-draft): atomic temp-file replace, configurable data path; visible in the round-trip test
- **Exit conditions** — EC-3, EC-4, EC-21, EC-24

### 2. Add the web scaffold, offline hosting and README ([#2](https://github.com/obrousseau1/nhl-draft-app/issues/2))

- **Scope** — `web/` (`package.json`, `index.html`, `vite.config.ts`, `tsconfig*.json`, `.oxlintrc.json` real; `src/main.tsx`, `src/App.tsx`, `src/test-setup.ts` comments only), static hosting of `wwwroot` with SPA fallback in the API, CI `web` job + publish step copying `web/dist` into the publish `wwwroot`, `README.md`
- **Ships alone because** — user's React starting point against chunk 1's live API; API still runs without `dist`
- **Coverage** — `pnpm run lint`, `pnpm run typeCheck`, `pnpm run test` in `web`; endpoint test that `/` serves index from a temp dist; manual run with network off; green Actions run, publish artifact holds `wwwroot/index.html`
- **Specs** — draft-board: Works offline; deployment: Continuous integration (web part)
- **Risk / brittle** — comment-only stubs (`--passWithNoTests`), port 5190 in two places, Risk 4 (draft night needs internet or two terminals)
- **Exit conditions** — EC-12, EC-15, EC-22, EC-23

### 3. Add pick rules, snake order, totals and picking ([#3](https://github.com/obrousseau1/nhl-draft-app/issues/3))

- **Scope** — `PickRules`, `Snake`, `PoolerTotals`, pick/replace/clear mutations and endpoints, `.http` entries; tests seed an in-memory pool (no XLSX)
- **Ships alone because** — additive on chunk 1; riskiest logic lands early
- **Coverage** — unit test per pick-rules and draft-board scenario, endpoint tests (422 ProblemDetails), restart test on the same data file
- **Specs** — pick-rules: all 6 requirements; draft-setup: Settings change mid-draft (from PR #8 review); draft-board: Board grid, Snake order current pick, Any empty box can be filled, Replace or clear a pick, Pooler totals, Draft survives restarts
- **Risk / brittle** — Risk 1 (reachability corner cases: replacing a G/Team/D box, last round); visible as a refused legal pick or accepted illegal one in tests
- **Exit conditions** — EC-5, EC-6, EC-7, EC-8, EC-9, EC-10, EC-11

### 4. Import the Draft Kit through NhlDraftKit.Core ([#4](https://github.com/obrousseau1/nhl-draft-app/issues/4))

- **Scope** — relative ProjectReference to `X:\nhl-fantasy-draft\src\NhlDraftKit.Core`, Kit → pool entry mapping, `POST /api/kit`, `.http` entry, real-kit check, CI checkout of private `nhl-fantasy-draft` side by side (`CORE_REPO_TOKEN` secret*)
- **Ships alone because** — one additive endpoint; isolates the brittle cross-repo seam in its own PR
- **Coverage** — tests with an XLSX built in-test via ClosedXML; manual import of the real 2026-2027 kit (918 / 93 / 32); green Actions run with Core checked out
- **Specs** — kit-import: Import Draft Kit, Missing cap hit counts as zero, Teams have no cap hit, Reject unreadable kit, Import only before the draft starts
- **Risk / brittle** — relative ProjectReference, `KitReader` owned by the other app, pool entry Id stability in saved draft files, Risk 3 (kit edge rows), CI token for the private Core repo
- **Exit conditions** — EC-1, EC-2, EC-17 (EC-21 extended: Core checked out)

### 5. Add player search and the roster view ([#5](https://github.com/obrousseau1/nhl-draft-app/issues/5))

- **Scope** — search (accent-insensitive name, position, NHL team, eligible-only via `PickRules`), roster grouping, both endpoints, `.http` entries
- **Ships alone because** — read-only endpoints on top of chunks 1 and 3
- **Coverage** — unit and endpoint tests for each player-search and roster-view scenario
- **Specs** — player-search: all 5 requirements; roster-view: Per-pooler roster, Available during and after the draft
- **Risk / brittle** — none
- **Exit conditions** — EC-13, EC-14

### 6. Deploy to Azure App Service ([#6](https://github.com/obrousseau1/nhl-draft-app/issues/6))

- **Scope** — CI `deploy` job (needs CI jobs, push to `main` only, OIDC `azure/login`, `azure/webapps-deploy`), README "Deploy to Azure" section; deploy job guarded `if: github.ref == 'refs/heads/main'` (CI runs on every branch); Azure resources created by hand* (Linux App Service F1 .NET 10, `Draft__DataFolder=/home/data`, `AllowedHosts=<app>.azurewebsites.net`, Entra app + federated credential, Easy Auth login always, assignment required, host only)
- **Ships alone because** — adds a job; local run and CI unaffected
- **Coverage** — push to `main` deploys; manual checks: anonymous visit → login, other account denied, picks survive restart and redeploy, red CI deploys nothing
- **Specs** — deployment: Continuous deployment to Azure, Azure access limited to the host, Azure draft persists, Local run never needs Azure
- **Risk / brittle** — Risk 5 (draft lost on redeploy), Risk 6 (app open to anyone), single instance, deploy restarts the app
- **Exit conditions** — EC-25, EC-26

### 7. Push the local draft to the Azure mirror ([#7](https://github.com/obrousseau1/nhl-draft-app/issues/7))

- **Scope** — draft `Revision`, Azure mirror mode (`Sync:Mode=Mirror`, snapshot endpoint, writes refused), local push `BackgroundService` with backoff, MSAL device-code token source with persistent cache, sync status in `GET /api/draft`, README sync section; Entra scope + public client flows + Easy Auth allowed audience by hand*
- **Ships alone because** — sync is off unless `Sync:AzureUrl` is set; local draft and deploy unaffected
- **Coverage** — tests with a fake HTTP handler and substituted token source; manual offline → reconnect run against Azure
- **Specs** — draft-sync: all 7 requirements
- **Risk / brittle** — Risk 7 (sync silently stops), revision counter must never decrease, hand-set Easy Auth audience / public client flag
- **Exit conditions** — EC-27, EC-28, EC-29, EC-30

## Task-wide exit conditions

EC-16 (build + tests), EC-18 (complexity), EC-19 (lean + clean-code audit), EC-20 (xUnit / FluentAssertions / NSubstitute, `Method_When_Condition_Should_Expectation`).

## Deliberately not chunks

*(none)* — openspec task 6.3 (`.http` file) is spread across chunks 1, 3, 4, 5; task 8.2 (real kit) rides in chunk 4; tasks 9.1-9.2 ride in chunks 1, 2 and 4; 9.3-9.5 in chunk 6; 10.x in chunk 7.

## Changelog

- **2026-10-05 — Creation** — written by /plan-and-shred from intent.md and spec.md.
- **2026-10-05 — CI/CD and Azure** — CI added to chunks 1, 2, 4; new chunk 6 (Azure deploy); EC-21 to EC-26 assigned.
- **2026-10-05 — Sync to Azure** — new chunk 7 (one-way push, read-only mirror); EC-27 to EC-30 assigned.
- **2026-10-05 — Build, shred 1** — built as planned; `Pick` record exists already (needed for the lock), pool entries left to chunks 3 and 4.
- **2026-10-05 — PR #8 review** — chunk 1 grew: same-origin write guard, AllowedHosts, IDraftFile driver, one file per draft + `POST /api/draft/new`, MaxPoolers, ProblemDetails errors, CI on every branch push. Chunk 3 gains "Settings change mid-draft"; chunk 6 gains the `main` deploy guard and `AllowedHosts`/`Draft__DataFolder` app settings.
