# Proposal

## Why

Our yearly live pool draft is tracked by hand: nobody sees each pooler's remaining cap or missing goalies/teams at a glance, and illegal picks (over cap, taken player, 3rd goalie) slip through. See `docs/intent.md`. Next draft is for season 2026-2027.

## What Changes

- New local web app run by the host on one laptop, no internet needed: C# API + React/TypeScript UI.
- Import the PoolExpert Draft Kit XLSX (skaters, goalies, NHL teams, cap hits) before the draft.
- Draft setup: poolers, snake order (random draw or manual), rounds, cap, D minimum, G and team exact counts.
- Draft board: poolers × rounds grid, current pick highlighted, click a box to pick, replace or clear.
- Pick rules enforced: cap, unique owner, G/team exact, D minimum reachable.
- Player search with name, position, NHL team filters and "eligible only".
- Per-pooler roster view for re-typing into PoolExpert.
- Draft state saved after every change; survives refresh and restart.
- XLSX parsing reused from `X:\nhl-fantasy-draft` (`NhlDraftKit.Core`) via relative project reference.
- Frontend: only toolchain config and comment-only source stubs; the user writes the React code.

## Capabilities

### New Capabilities
- `kit-import`: load players, goalies and NHL teams with cap hits from the Draft Kit XLSX.
- `draft-setup`: poolers, draft order, rounds, cap and roster-count settings.
- `draft-board`: the pick grid, snake turn, making/replacing/clearing picks, per-pooler totals, persistence.
- `pick-rules`: which picks are legal for a pooler in a round.
- `player-search`: finding an unpicked player or team for a box.
- `roster-view`: per-pooler roster listing for PoolExpert entry.

### Modified Capabilities
_(none — new project)_

## Impact

- New repo layout: `api/` (C# API + tests), `web/` (Vite React TS), `NhlDraftApp.slnx`.
- Dependency on `X:\nhl-fantasy-draft\src\NhlDraftKit.Core` (net10.0, ClosedXML) — both repos must sit side by side.
- Local data file under the user's app-data folder.
- No external services, no network calls at runtime.
