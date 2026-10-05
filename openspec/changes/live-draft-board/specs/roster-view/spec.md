# Spec Delta

## Purpose

Shows each pooler's roster in a form that is quick to re-type into PoolExpert, which has no import.

## ADDED Requirements

### Requirement: Per-pooler roster
The system SHALL list, for a chosen pooler, every pick grouped as forwards, defensemen, goalies and teams, each line showing full name, NHL team and cap hit, plus the total cap used.

#### Scenario: Roster of Alex
- **WHEN** the host opens Alex's roster
- **THEN** picks are grouped Forwards, Defensemen, Goalies, Teams, and the total cap used is shown

### Requirement: Available during and after the draft
The system SHALL show the roster view at any time, including partial rosters.

#### Scenario: Partial roster
- **WHEN** Alex has 5 picks
- **THEN** the roster lists those 5
