# Spec Delta

## Purpose

Keeps the Azure app as a read-only mirror of the local draft: the local app pushes every change when online, and drafting never depends on the connection.

## ADDED Requirements

### Requirement: Automatic push when online
The local app SHALL push the full draft to the Azure app within 30 seconds of any change while a connection is available and sync is configured.

#### Scenario: Pick pushed
- **WHEN** the host makes a pick while online
- **THEN** the Azure board shows that pick within 30 seconds

### Requirement: Offline changes pushed later
The local app SHALL keep working with no connection, mark the draft as pending, and push the latest draft once the connection returns.

#### Scenario: Back online
- **WHEN** 12 picks are made offline and the connection returns
- **THEN** the Azure board shows all 12 picks without any host action

### Requirement: Older snapshot refused
The Azure app SHALL refuse a pushed draft whose revision is not newer than the one it holds.

#### Scenario: Stale laptop copy
- **WHEN** Azure holds revision 40 and a push carries revision 35
- **THEN** the push is refused and Azure keeps revision 40

### Requirement: Azure is a read-only mirror
The Azure app SHALL refuse every draft change except a pushed snapshot, and show when it was last updated.

#### Scenario: Edit on Azure
- **WHEN** the host tries to make a pick on the Azure app
- **THEN** the change is refused with a message that Azure is a read-only mirror

### Requirement: Sync status visible locally
The local app SHALL report the sync state as synced, pending, or failing with its reason, including when the host must log in.

#### Scenario: Login needed
- **WHEN** the cached login has expired
- **THEN** the status says login is needed and shows the device-login code, and picks keep working

### Requirement: Host logs in once
The local app SHALL authenticate to Azure as the host with a device-code login and reuse the cached login across restarts until it expires.

#### Scenario: Restart keeps login
- **WHEN** the host logged in yesterday and restarts the local app
- **THEN** pushes resume without a new login

### Requirement: Sync is optional
The local app SHALL behave exactly as without sync when no Azure address is configured.

#### Scenario: No Azure configured
- **WHEN** no Azure address is set
- **THEN** no push is attempted and the status reports sync off
