# Spec Delta

## Purpose

Decides whether a player or team can go into a given pooler's box, so illegal picks are blocked before they reach the board.

## ADDED Requirements

### Requirement: One owner per player and team
The system SHALL refuse a player or team already picked in another box.

#### Scenario: Taken player
- **WHEN** McDavid is in Alex's box and the host picks McDavid for Sam
- **THEN** the pick is refused with a message naming the owner

### Requirement: Cap not exceeded
The system SHALL refuse a pick whose cap hit is greater than the pooler's cap remaining, counting a replaced box's player as removed.

#### Scenario: Over cap
- **WHEN** Sam has 3.0 remaining and the host picks a 4.0 player
- **THEN** the pick is refused

#### Scenario: Replacement frees cap
- **WHEN** Sam has 1.0 remaining and replaces a 5.0 player with a 5.5 player
- **THEN** the pick is accepted

### Requirement: Goalie and team counts are exact maxima
The system SHALL refuse a goalie or team pick that would exceed the goalie or team count.

#### Scenario: Third goalie
- **WHEN** Sam has 2 goalies and the count is 2
- **THEN** a third goalie is refused

### Requirement: Defensemen beyond minimum allowed
The system SHALL accept defensemen beyond the D minimum, subject to the other rules.

#### Scenario: Third defenseman
- **WHEN** Sam has 2 D and 10 empty boxes
- **THEN** a third D is accepted

### Requirement: Required picks stay reachable
The system SHALL refuse a pick that leaves fewer empty boxes than the pooler's defensemen, goalies and teams still needed.

#### Scenario: Forward with goalies missing
- **WHEN** Sam has 2 empty boxes, needs 2 goalies, and the host picks a forward
- **THEN** the pick is refused with a message saying goalies are still needed

### Requirement: Refusal reason
The system SHALL state the rule that refused a pick.

#### Scenario: Reason shown
- **WHEN** a pick is refused
- **THEN** the host sees which rule blocked it
