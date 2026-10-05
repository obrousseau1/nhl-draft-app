# Spec Delta

## Purpose

Lets the host configure a draft before it starts: who drafts, in which order, for how many rounds, and under which cap and roster counts.

## ADDED Requirements

### Requirement: Draft settings with defaults
The system SHALL let the host set the number of rounds, the salary cap, the defensemen minimum, the goalie count, the team count and the maximum number of poolers, defaulting to 20 rounds, 104 M$, 2, 2, 2 and 12.

#### Scenario: Fresh draft defaults
- **WHEN** a new draft is created
- **THEN** settings are 20 rounds, 104 M$ cap, D minimum 2, goalies 2, teams 2, max poolers 12

### Requirement: Settings are consistent
The system SHALL reject settings where rounds are outside 1–50, the cap is outside (0, 1000] M$, the D minimum, goalie or team count is outside 0–rounds, their sum exceeds the rounds, or max poolers is outside 2–30 or below the poolers already added.

#### Scenario: Too many required picks
- **WHEN** the host sets 5 rounds with D 2, G 2, teams 2
- **THEN** the settings are rejected with a message

#### Scenario: Overflowing count
- **WHEN** the host sets D minimum to the largest integer and goalies to 1
- **THEN** the settings are rejected

#### Scenario: Boundaries accepted
- **WHEN** the host sets 50 rounds, a 1000 M$ cap, or D + goalies + teams equal to the rounds
- **THEN** the settings are applied

### Requirement: Poolers
The system SHALL let the host add, remove and rename poolers; names MUST be non-empty, at most 40 characters and unique ignoring case, and adding SHALL be refused once the maximum number of poolers is reached.

#### Scenario: Duplicate name
- **WHEN** Alex exists and the host adds "alex"
- **THEN** the change is rejected with "Alex is already in the pooler list."

#### Scenario: Pooler limit reached
- **WHEN** max poolers is 12, 12 poolers exist and the host adds Max
- **THEN** the change is rejected with "Cannot add Max: the pooler limit of 12 is reached."

### Requirement: Draft order
The system SHALL let the host shuffle the pooler order randomly or reorder poolers manually; the order is round 1's pick order.

#### Scenario: Random draw
- **WHEN** the host shuffles 12 poolers
- **THEN** the board shows the 12 poolers in a new random order

### Requirement: Setup locked once the draft starts
The system SHALL refuse changes to the pooler list and order once at least one pick exists; renaming a pooler SHALL stay allowed. Settings changes after the first pick follow "Settings change mid-draft".

#### Scenario: Add pooler mid-draft
- **WHEN** a pick exists and the host adds a pooler
- **THEN** the change is refused, saying the setup is locked, renaming is allowed and reset clears all picks

#### Scenario: Rename mid-draft
- **WHEN** a pick exists and the host renames a pooler
- **THEN** the name changes and picks are kept

### Requirement: Settings change mid-draft
The system SHALL accept a settings change after the first pick only when every existing pick still fits: rounds at or above the last filled round, each pooler's cap used within the new cap, goalies and teams picked within the new counts, and the picks still needed reachable; otherwise it SHALL refuse with the reason.

#### Scenario: Cap fixed after 10 rounds
- **WHEN** 10 rounds are drafted under a 100 M$ cap, every pooler used at most 60 M$, and the host sets the cap to 104 M$
- **THEN** the cap changes and all picks are kept

#### Scenario: Change that breaks a pick
- **WHEN** a pooler already has 2 goalies and the host sets the goalie count to 1
- **THEN** the change is refused with the reason

### Requirement: Reset draft
The system SHALL let the host clear all picks after an explicit confirmation, keeping settings, poolers and the imported pool.

#### Scenario: Reset
- **WHEN** the host confirms reset
- **THEN** every box is empty and setup is unlocked

### Requirement: Start a new draft
The system SHALL let the host start a new draft that keeps the current settings and poolers with no picks, saved separately so the previous draft stays untouched; the newest draft opens at startup.

#### Scenario: Next season
- **WHEN** last season's draft is complete and the host starts a new draft
- **THEN** the board shows the same poolers and settings with empty boxes, and last season's draft is still saved
