# nhl-draft-app

Live board for our yearly NHL pool draft. One host laptop runs it, the room watches it, and it blocks illegal picks. It works fully offline. An Azure copy mirrors the draft online.

## What it does

- **Import** the PoolExpert Draft Kit XLSX: skaters, goalies, NHL teams and cap hits.
- **Setup:** poolers (max 12 by default), snake order (random draw or manual), rounds (20), cap (104 M$), D minimum (2), goalies (2), teams (2). The pooler list and order lock after the first pick, but renaming stays allowed. Settings can change mid-draft when the existing picks still fit (#3). **New draft** keeps the settings and poolers in a fresh file.
- **Board:** poolers × rounds. Click a box, search an unpicked player or team, and pick. The current snake pick is highlighted, and any box can be filled, replaced or cleared.
- **Rules enforced:** cap not exceeded, one owner per player or team, exactly 2 goalies and 2 teams, and the D / G / team picks still needed must stay reachable.
- **Per pooler:** cap remaining, average cap per remaining player pick (team picks excluded), and the D, goalies and teams still needed.
- **Roster view** for re-typing into PoolExpert, which has no import.
- **Saved after every change**, so the draft survives a refresh or a restart.

Full specs: [docs/intent.md](docs/intent.md) (the problem), [docs/spec.md](docs/spec.md) (requirements and exit conditions), [docs/plan.md](docs/plan.md) (delivery chunks = GitHub issues).

### Status

| Chunk | Issue | State |
|---|---|---|
| API scaffold, draft state, persistence, setup | [#1](https://github.com/obrousseau1/nhl-draft-app/issues/1) | in review, CI green |
| Web scaffold, offline hosting | [#2](https://github.com/obrousseau1/nhl-draft-app/issues/2) | planned |
| Pick rules, snake order, totals | [#3](https://github.com/obrousseau1/nhl-draft-app/issues/3) | planned |
| Draft Kit import | [#4](https://github.com/obrousseau1/nhl-draft-app/issues/4) | planned |
| Search and roster view | [#5](https://github.com/obrousseau1/nhl-draft-app/issues/5) | planned |
| Azure deploy | [#6](https://github.com/obrousseau1/nhl-draft-app/issues/6) | planned |
| Push local draft to Azure mirror | [#7](https://github.com/obrousseau1/nhl-draft-app/issues/7) | planned |

## Tech choices

| Area | Choice | Why |
|---|---|---|
| API | .NET 10 ASP.NET Core controllers (`api/NhlDraftApp.Api`), one per resource | All draft rules live in tested C#; controller actions only map HTTP to the domain. |
| Storage | One JSON file per draft (newest = current), written to a temp file and then swapped in | It's one ~100 KB document, so no database is needed. A crash never leaves a half-written file, and a new draft never overwrites the previous one. |
| Frontend | React + TypeScript + Vite (`web/`, planned) | Written by hand as a learning project. |
| XLSX parsing | `NhlDraftKit.Core` from [nhl-fantasy-draft](https://github.com/obrousseau1/nhl-fantasy-draft) via relative project reference (planned, #4) | Already reads this exact kit. |
| Tests | xUnit, FluentAssertions, NSubstitute | Test names follow `Method_When_Condition_Should_Expectation`. |
| CI/CD | GitHub Actions; Azure App Service (planned) | Azure access limited to the host by Easy Auth. |
| Online copy | Local app pushes snapshots; Azure is a read-only mirror (planned, #7) | Drafting never depends on the connection. |

## Security (local)

The API listens on `localhost` only, so other devices can't reach it. Writes coming from another website are refused (403, `Origin` check), and requests for a host other than `localhost`/`127.0.0.1` are refused (400, `AllowedHosts`). Errors are ProblemDetails (`application/problem+json`, message in `detail`). Login only exists on Azure (#6).

## Run

Requires the .NET 10 SDK.

```
dotnet run --project api/NhlDraftApp.Api        # http://localhost:5190
dotnet build NhlDraftApp.slnx
dotnet test NhlDraftApp.slnx
```

Try the endpoints with [NhlDraftApp.Api.http](api/NhlDraftApp.Api/NhlDraftApp.Api.http) (VS Code REST Client, Rider or Visual Studio).

**Draft data:** one file per draft, `draft-<yyyyMMdd-HHmmss-fff>.json`, in `%LOCALAPPDATA%NhlDraftApp` by default. Override the folder with the `Draft:DataFolder` setting, e.g. `Draft__DataFolder=C:	emp
hl`. The newest file opens at startup. `POST /api/draft/new` starts a new draft that keeps the settings and poolers, and the previous file is kept. An unreadable file is renamed `.corrupt-<timestamp>`.

### Coming with later chunks

- **#2:** `cd web`, `pnpm install` and `pnpm dev` for the frontend in development. On draft night, `dotnet run` alone serves the built app, offline.
- **#4:** clone `nhl-fantasy-draft` next to this repo (`X:\nhl-draft-app` and `X:\nhl-fantasy-draft`), or the build fails.

## CI

[.github/workflows/ci.yml](.github/workflows/ci.yml) builds and tests on **every push to any branch**, `main` included. A PR isn't needed to get a run: push the branch and check the Actions tab, or the PR's checks once one is open.

## Conventions

- **Branches:** `NDA-<issue>-<short-name>`, e.g. `NDA-1-API-scaffolding`.
- **Commits and PRs:** start the title with `NDA-<issue>`, which auto-links to that issue (repo autolink). Add `Closes #<issue>` so the merge closes it.
- **Build must be clean:** warnings are errors (`Directory.Build.props`), code style is checked at build time, and StyleCop rules are on.
- **Code style** ([.editorconfig](.editorconfig)):
  - `System` usings first, then the rest alphabetically.
  - Public members before private.
  - `var` everywhere (`var x = new Foo()`).
  - No braces on one-line bodies.
  - No `_ =` discards.
- **Tests:** xUnit, FluentAssertions and NSubstitute, named `Method_When_Condition_Should_Expectation`. Repeated values go in private constants.
