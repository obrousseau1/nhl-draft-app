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
