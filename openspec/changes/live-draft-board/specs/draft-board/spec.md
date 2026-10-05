# Spec Delta

## Purpose

The live board the room watches: every pooler's picks per round, whose turn it is, and each pooler's cap and roster needs, kept safe across restarts.

## ADDED Requirements

### Requirement: Board grid
The system SHALL show one row per pooler in draft order and one column per round; each box shows the picked player's name, position, NHL team and cap hit, or the picked team's name, or is empty.

#### Scenario: Filled box
- **WHEN** Alex picked Connor McDavid in round 1
- **THEN** Alex's round 1 box shows "Connor McDavid", C, EDM, 12.5

### Requirement: Snake order current pick
The system SHALL highlight the current pick: the first empty box in snake order, where odd rounds follow the draft order and even rounds reverse it.

#### Scenario: Round 2 reverses
- **WHEN** poolers A, B, C have all picked in round 1 and nobody in round 2
- **THEN** C's round 2 box is highlighted

#### Scenario: Skipped box stays current
- **WHEN** B's round 1 box is empty and C's round 1 box is filled
- **THEN** B's round 1 box is highlighted

### Requirement: Any empty box can be filled
The system SHALL let the host pick into any empty box, not only the highlighted one.

#### Scenario: Out of turn
- **WHEN** the current pick is A round 3 and the host picks for D round 3
- **THEN** the pick is saved and A round 3 stays highlighted

### Requirement: Replace or clear a pick
The system SHALL let the host replace or clear a filled box; a cleared or replaced player returns to the pool.

#### Scenario: Clear
- **WHEN** the host clears Alex's round 1 box holding McDavid
- **THEN** the box is empty and McDavid is searchable again

### Requirement: Pooler totals
The system SHALL show per pooler: cap remaining (cap minus picked cap hits), average cap per remaining player pick (cap remaining ÷ (empty boxes − teams still needed), since teams cost no cap; blank when that is 0), and defensemen, goalies and teams still needed.

#### Scenario: Totals after two picks
- **WHEN** cap is 104, 20 rounds, and Alex picked McDavid (12.5, C) and a D at 3.5
- **THEN** Alex shows cap remaining 88.0, average 5.5 (88 ÷ 16: 18 empty boxes minus 2 team picks), D needed 1, G needed 2, teams needed 2

### Requirement: Draft survives restarts
The system SHALL save settings, poolers, order, pool and picks after every change and restore them when the app or the browser restarts.

#### Scenario: Laptop restart
- **WHEN** 37 picks are made and the app is stopped and started again
- **THEN** the board shows the same 37 picks, totals and current pick

### Requirement: Works offline
The system SHALL run entirely on the host machine without internet access.

#### Scenario: No network
- **WHEN** the laptop has no network connection
- **THEN** import, setup, picking and the board all work
