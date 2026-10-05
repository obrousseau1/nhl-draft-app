# Spec Delta

## Purpose

Lets the host configure a draft before it starts: who drafts, in which order, for how many rounds, and under which cap and roster counts.

## ADDED Requirements

### Requirement: Draft settings with defaults
The system SHALL let the host set the number of rounds, the salary cap, the defensemen minimum, the goalie count and the team count, defaulting to 20 rounds, 104 M$, 2, 2 and 2.

#### Scenario: Fresh draft defaults
- **WHEN** a new draft is created
- **THEN** settings are 20 rounds, 104 M$ cap, D minimum 2, goalies 2, teams 2

### Requirement: Settings are consistent
The system SHALL reject settings where the D minimum plus the goalie count plus the team count exceeds the number of rounds, where any value is negative, or where rounds or cap is zero.

#### Scenario: Too many required picks
- **WHEN** the host sets 5 rounds with D 2, G 2, teams 2
- **THEN** the settings are rejected with a message

### Requirement: Poolers
The system SHALL let the host add, remove and rename poolers; names MUST be non-empty and unique.

#### Scenario: Duplicate name
- **WHEN** the host adds a pooler named like an existing one
- **THEN** the change is rejected

### Requirement: Draft order
The system SHALL let the host shuffle the pooler order randomly or reorder poolers manually; the order is round 1's pick order.

#### Scenario: Random draw
- **WHEN** the host shuffles 12 poolers
- **THEN** the board shows the 12 poolers in a new random order

### Requirement: Setup locked once the draft starts
The system SHALL refuse changes to rounds, cap, counts, pooler list and order once at least one pick exists; renaming a pooler SHALL stay allowed.

#### Scenario: Change rounds mid-draft
- **WHEN** a pick exists and the host changes rounds
- **THEN** the change is refused

#### Scenario: Rename mid-draft
- **WHEN** a pick exists and the host renames a pooler
- **THEN** the name changes and picks are kept

### Requirement: Reset draft
The system SHALL let the host clear all picks after an explicit confirmation, keeping settings, poolers and the imported pool.

#### Scenario: Reset
- **WHEN** the host confirms reset
- **THEN** every box is empty and setup is unlocked
