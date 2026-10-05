# Spec Delta

## Purpose

Finds an unpicked player or team quickly when the host fills a box during the live draft.

## ADDED Requirements

### Requirement: Search unpicked by name
The system SHALL search unpicked players and teams by first or last name, case- and accent-insensitive.

#### Scenario: Accent-insensitive
- **WHEN** the host types "stutzle"
- **THEN** Tim Stützle is listed if unpicked

#### Scenario: Picked player hidden
- **WHEN** McDavid is picked and the host types "mcdavid"
- **THEN** no result is listed

### Requirement: Filter by position
The system SHALL filter results by C, LW, RW, D, G or Team.

#### Scenario: Goalies only
- **WHEN** the host filters on G
- **THEN** only unpicked goalies are listed

### Requirement: Filter by NHL team
The system SHALL filter results by NHL team.

#### Scenario: Oilers only
- **WHEN** the host filters on EDM
- **THEN** only unpicked Oilers players, and the Oilers team if unpicked, are listed

### Requirement: Eligible only
The system SHALL, when "eligible only" is on, hide results the pick rules would refuse for the box being filled.

#### Scenario: Unaffordable hidden
- **WHEN** Sam has 3.0 remaining and "eligible only" is on
- **THEN** players with a cap hit above 3.0 are not listed

### Requirement: Result details
The system SHALL show for each result its name, position, NHL team and cap hit, with the "no cap hit" marker when applicable.

#### Scenario: No cap hit player
- **WHEN** a player without cap hit in the kit is listed
- **THEN** it shows cap 0 with the marker
