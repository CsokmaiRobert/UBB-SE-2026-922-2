# Games / Listings Feature

## Overview
This feature handles the page where users manage their board games.
In the WinUI desktop app this is the "My Listings" page.
In the MVC web app, it should be the "My Games" page.

## Requirements
- **Access Control**:
  - Unauthenticated users: Redirect to Login.
  - Normal users: See and manage only their own games.
  - Admins: See all games and manage any game.
- **Features**:
  - View games (List/Details).
  - Create new games (with image upload).
  - Edit games (toggle Active/Inactive, update details, image upload).
  - Delete games (check for active rentals/conflicts).
- **Visuals**:
  - Inactive games should be visually muted (e.g., lower opacity, grayscale).

## Architecture
- **MVC Controller** (`GamesController`)
- **Proxy Service** (`IGameProxyService` / `GameProxyService`)
- **API Controller** (`GamesController` in API)
- **API Service** (`IGameService` / `GameService`)
- **Repository** (`GameRepository`)
- **Database**
