# Design

## Context

Empty repo. Motivation and scope: see `proposal.md` and `docs/intent.md`. Requirements: see `specs/`.

- `X:\nhl-fantasy-draft\src\NhlDraftKit.Core` (net10.0, ClosedXML 0.105.1) already reads this exact Draft Kit: `KitReader.Read(path) → Kit { Season, Players, Teams }`, `Player { First, Last, Team, Position, CapHit?, Group }`, `TeamRow { Abbrev, Name }`, `Teams.FromKit/FromName` map kit codes to NHL codes. `KitFormatException` names missing headers.
- `X:\first-reactor` is the layout and toolchain model: `api/` minimal API + xUnit tests, `web/` Vite + React 19 + TS 6 + oxlint + vitest, `.slnx`, `/api` proxied by Vite in dev.
- The user writes the React code themselves (learning). Claude ships toolchain config that works, and source files holding beginner comments only.
- Toolchain on host: .NET SDK 10.0.401, Node 26, pnpm 12.

## Goals / Non-Goals

**Goals:**
- All draft logic (rules, snake, totals, search) in C#, unit-tested, so the frontend only displays and calls.
- One command in "draft night" mode: API serves the built frontend, no internet, no second terminal.
- A documented HTTP contract (`.http` file) the user codes the frontend against.

**Non-Goals:**
- Writing any React component, hook or style.
- App-level auth (Azure login is platform config; the only app code is the sync client's token), multi-device editing, two-way sync, real-time push to browsers.
- Changing `NhlDraftKit.Core` (consumed as is).

## Decisions

**D1 — Layout mirrors first-reactor.** `NhlDraftApp.slnx`, `api/NhlDraftApp.Api`, `api/NhlDraftApp.Api.Tests`, `web/`. *Alt:* single project — rejected; user already knows the first-reactor shape.

**D2 — Reuse Core by relative ProjectReference** (`..\..\..\nhl-fantasy-draft\src\NhlDraftKit.Core\NhlDraftKit.Core.csproj`). User decision. *Alt:* submodule, local NuGet, extract repo — rejected for friction on a one-laptop app. Only `KitReader`, `Kit`, `Player`, `TeamRow` are used; Core's `DraftSession` is season-prep logic and is not reused.

**D3 — Pool snapshot in draft state.** On import the API maps `Kit` into its own `PoolEntry { Id, First, Last, NhlTeam, Position (C/LW/RW/D/G/Team), CapHit, NoCapHit }` list and stores it. Picks reference `PoolEntry.Id`. Teams become entries with `Position = Team`, `CapHit = 0`. Isolates the app from Core model changes after import and keeps restore independent of the XLSX file. Upload is multipart → temp file → `KitReader.Read` → delete temp (KitReader takes a path).

**D4 — Domain in plain classes, controllers thin.** `Draft` (state + mutations, returns a result with refusal reason), `PickRules.Check(draft, pooler, round, entry)`, `Snake.Current(draft)`, `PoolerTotals.Of(draft, pooler)`, `Search.Find(...)`. `[ApiController]` classes (one per resource: `DraftController`, `PoolersController`, later `PicksController`, `KitController`, `PoolController`, `SyncController`) map HTTP ↔ domain and return `IResult`; `ChangeResponses` maps a `Change` to its status code. User decision: controllers over minimal-API `Map*` endpoints. Rules are one ordered list of checks, first failure wins → one reason string (spec *Refusal reason*).

**D5 — Rules arithmetic.** For a pick of entry *e* into box (p, r), with the box's current pick removed first:
- owner: *e* not in any other box.
- cap: `capUsed + e.CapHit ≤ cap` (decimal, rounded to 3 places to absorb kit noise like 12.604).
- G/Team: `count(G)+1 ≤ goalies` / `count(Team)+1 ≤ teams`.
- reachable: `max(0, dMin − D) + (goalies − G) + (teams − T) ≤ emptyBoxes` after the pick.
"Eligible only" search runs `PickRules.Check` per candidate (≈1 050 entries, trivial).

**D6 — Persistence: one JSON file, atomic write.** `%LOCALAPPDATA%\NhlDraftApp\draft.json` (path read from config `Draft:DataPath`; tests and Azure override it — Azure uses `/home/data/draft.json`, the only persisted folder). Write to `draft.json.tmp` then `File.Move(overwrite: true)` after every mutation; load at startup. One draft at a time. *Alt:* SQLite/EF — rejected, a 100 KB document with no queries. A single in-process lock serialises mutations (one host, but React StrictMode double calls exist).

**D7 — Offline "draft night" mode.** `pnpm build` outputs to `web/dist`; the API serves it via `UseStaticFiles` + `MapFallbackToFile("index.html")` from a configured path. Dev keeps Vite on its port with `/api` proxy to `http://localhost:5190`. No CDN fonts or scripts in `index.html`.

**D8 — HTTP contract** (all JSON; refusals = 422 `{ reason }`; locked setup/import = 409 `{ reason }`):
- `GET /api/draft` → settings, poolers (order), picks, per-pooler totals, current pick, pool size, `started`.
- `PUT /api/draft/settings`, `POST /api/poolers`, `PUT /api/poolers/{id}` (rename), `DELETE /api/poolers/{id}`, `PUT /api/poolers/order`, `POST /api/poolers/shuffle`, `POST /api/draft/reset`.
- `POST /api/kit` (multipart file).
- `GET /api/pool?q=&position=&team=&pooler=&round=&eligibleOnly=`.
- `PUT /api/picks/{poolerId}/{round}` `{ entryId }`, `DELETE /api/picks/{poolerId}/{round}`.
- `GET /api/poolers/{id}/roster`.
Documented with examples in `api/NhlDraftApp.Api/NhlDraftApp.Api.http`.

**D9 — Frontend stubs: config real, source comments only.** Real: `package.json` (first-reactor deps, `test: vitest run --passWithNoTests`), `index.html`, `vite.config.ts`, `tsconfig*.json`, `.oxlintrc.json`. Comment-only: `src/main.tsx`, `src/App.tsx`, `src/test-setup.ts`. `App.tsx` comments list the suggested components (SetupPage, ImportKit, DraftBoard, PickDialog, PoolerTotals, RosterView), the API calls each needs, and the React concept each one practises, in build order. JSON files cannot hold comments, so they carry working content. *Alt:* commented code — rejected, user wants to write it.

**D10 — Test stack.** xUnit + FluentAssertions + NSubstitute (user standard); test names `Method_When_Condition_Should_Expectation`. NSubstitute only where a seam exists (e.g. the random source for shuffle); no interface added just to mock. FluentAssertions 8+ is free for non-commercial use only — fine for a personal app.

**D11 — Accent-insensitive search** via `string.Normalize(FormD)` stripping `NonSpacingMark`, lower-invariant, on first, last and "first last".

**D12 — CI: GitHub Actions, two repos side by side.** `.github/workflows/ci.yml` on push and PR to `main`: checkout this repo into `nhl-draft-app/` and `obrousseau1/nhl-fantasy-draft` (private) into `nhl-fantasy-draft/` with a read-only fine-grained token secret `CORE_REPO_TOKEN`, so D2's relative path resolves unchanged. Jobs: `dotnet` (build + test slnx), `web` (pnpm install, lint, typeCheck, test). *Alt:* submodule/NuGet — already discarded (D2).

**D13 — Publish bundles the web app.** CI builds `web/dist` and copies it into the API publish output's `wwwroot`; D7's static hosting serves `wwwroot` when present. One artifact, same for local "draft night" and Azure.

**D14 — CD to Azure App Service on push to `main`.** Deploy job `needs` both CI jobs, runs only on push to `main`, logs in with OIDC (`azure/login`, federated credential; no publish profile — basic auth is off by default on new apps), deploys with `azure/webapps-deploy`. Target: Linux App Service, .NET 10, F1 (free; cold start acceptable), single instance. App setting `Draft__DataPath=/home/data/draft.json`. User decision: every push to `main`.

**D15 — Azure access via Easy Auth, login always, host only.** App Service Authentication, Microsoft provider, "require authentication" on all requests, Entra enterprise app with "assignment required" and only the host assigned. No app code. Configured once by hand, documented in README. *Alt:* login only to edit (identity check in API) and shared access code — rejected; one host device.

**D16 — One-way sync: local pushes snapshots, Azure mirrors.** User decision (auto-push, read-only mirror). Each saved change bumps `Revision` in `draft.json`. Local: a `BackgroundService` wakes on save, and `PUT /api/sync/snapshot` `{ revision, draft }` to `Sync:AzureUrl` with a bearer token; on failure it retries with backoff (5 s → 5 min) and status = pending/failing. Azure (`Sync:Mode=Mirror`): accepts a snapshot only if `revision` > held revision (else 409), refuses every other write with 403 `{ reason }`. Status (`off | synced | pending | failing`, reason, device code, last pushed at) is part of `GET /api/draft`. No database: Azure keeps the same JSON file under `/home`. *Alt:* manual export/import (user preferred automatic), two-way merge + Cosmos/SQL (conflict handling dwarfs the app), editable Azure with last push wins (silent loss).

**D17 — Sync auth: MSAL device code through Easy Auth.** Local app is an MSAL public client (`Microsoft.Identity.Client` + `.Extensions.Msal` for a DPAPI-protected token cache) requesting `api://<clientId>/access_as_user`. Entra app: expose that scope, allow public client flows; Easy Auth "allowed token audiences" includes `api://<clientId>`. Assignment-required still restricts to the host. Device code shown via sync status. `ITokenSource` and the snapshot `HttpClient` are seams NSubstitute / a fake handler substitute in tests (allowed by EC-19).

## Risks / Trade-offs

- [Sync auth misconfigured (audience, public client flag)] → status "failing: 401"; drafting unaffected; README lists the Entra settings.
- [Laptop restored from an old `draft.json`] → its revisions are lower; Azure refuses (409) and status shows the reason; forcing a push is out of scope.

- [Private Core repo token expires] → CI red on checkout; README notes renewal.
- [Scale-out to a second Azure instance] → JSON file + in-process lock assume one instance; README states single instance.
- [Push to `main` deploys mid-draft and restarts the app] → restart restores state (D6); don't merge on draft night.
- [Azure lags the local draft while offline] → by design; status shows pending until the push lands.

- [Relative ProjectReference breaks if folders move or Core changes signature] → build fails loudly; pin by noting the Core commit used in README.
- [Kit layout changes next season] → `KitFormatException` surfaces missing headers; import refused, old pool kept.
- [Comment-only `.tsx` files render a blank page] → expected; README says so. Lint/typecheck still pass on comment-only modules (`moduleDetection: force`).
- [JSON file corruption on power loss] → atomic temp-file replace; worst case last change lost.
- [Cap float drift] → decimal + 3-place rounding.
- [Team picks have no kit cap/position] → mapped explicitly in D3; covered by tests.

## Migration Plan

New app; nothing to migrate. Rollback = delete `draft.json`.

## Open Questions

_(none — order draw resolved as in-app shuffle plus manual reorder)_
