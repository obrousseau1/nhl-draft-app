# Spec Delta

## Purpose

Builds and tests every change on GitHub, and publishes the app to an Azure Web App reachable only by the host, alongside the local offline run.

## ADDED Requirements

### Requirement: Continuous integration
The system SHALL build and test the API and lint, typecheck and test the web app on every push and pull request to `main`.

#### Scenario: Pull request checked
- **WHEN** a pull request targets `main`
- **THEN** a GitHub Actions run builds and tests both, and a failure marks the pull request red

### Requirement: Continuous deployment to Azure
The system SHALL deploy to the Azure Web App on every push to `main` whose build and tests pass.

#### Scenario: Merge deploys
- **WHEN** a commit lands on `main` and CI passes
- **THEN** the Azure Web App serves that commit's build

#### Scenario: Red build not deployed
- **WHEN** CI fails on `main`
- **THEN** nothing is deployed

### Requirement: Azure access limited to the host
The Azure-hosted app SHALL require a login for every page and API call and admit only the host's account.

#### Scenario: Anonymous visitor
- **WHEN** someone opens the Azure URL without logging in
- **THEN** they are sent to the login page and see no draft data

#### Scenario: Other account
- **WHEN** a logged-in account other than the host's opens the Azure URL
- **THEN** access is denied

### Requirement: Azure draft persists
The Azure-hosted draft SHALL survive an app restart and a redeploy.

#### Scenario: Redeploy mid-season
- **WHEN** picks exist on Azure and a new commit is deployed
- **THEN** the board shows the same picks

### Requirement: Local run never needs Azure
The local run SHALL never need the Azure app; the Azure draft only changes through pushes from the local app (see draft-sync).

#### Scenario: Internet down on draft night
- **WHEN** the host runs the app locally with no internet
- **THEN** the draft works fully, regardless of the Azure app
