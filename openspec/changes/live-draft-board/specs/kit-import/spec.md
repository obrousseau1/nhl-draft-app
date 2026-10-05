# Spec Delta

## Purpose

Loads the draftable pool — skaters, goalies and NHL teams with their cap hits — from the PoolExpert Draft Kit XLSX before the draft starts.

## ADDED Requirements

### Requirement: Import Draft Kit
The system SHALL import a Draft Kit XLSX and replace the draftable pool with its skaters (C, LW, RW, D), goalies (G) and NHL teams.

#### Scenario: Valid kit imported
- **WHEN** the host uploads the 2026-2027 Draft Kit before any pick
- **THEN** the pool holds 918 skaters, 93 goalies and 32 teams, each skater and goalie with first name, last name, NHL team, position and cap hit

#### Scenario: Name markers stripped
- **WHEN** a last name in the kit ends with `°`
- **THEN** the imported name has no `°`

### Requirement: Missing cap hit counts as zero
The system SHALL import a player with no cap hit as pickable with a cap hit of 0 and mark them as having no cap hit.

#### Scenario: Goalie without cap hit
- **WHEN** the kit row for a goalie has an empty cap hit
- **THEN** the goalie is in the pool with cap hit 0 and a "no cap hit" marker

### Requirement: Teams have no cap hit
The system SHALL import NHL teams with a cap hit of 0.

#### Scenario: Team cap
- **WHEN** a team is imported
- **THEN** its cap hit is 0

### Requirement: Reject unreadable kit
The system SHALL reject a file that is not a readable Draft Kit and keep the current pool unchanged, reporting which required column or sheet is missing.

#### Scenario: Wrong file
- **WHEN** the host uploads an XLSX without a `CapH` column
- **THEN** the import fails with a message naming the missing column and the previous pool is kept

### Requirement: Import only before the draft starts
The system SHALL refuse an import once at least one pick exists.

#### Scenario: Import after first pick
- **WHEN** one pick exists and the host uploads a kit
- **THEN** the import is refused and the pool and picks are unchanged
