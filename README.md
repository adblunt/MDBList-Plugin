# MDBList Ratings Provider (Emby)

Adds Rotten Tomatoes critic and audience ratings to Emby using MDBList.

## What it does

- Adds Rotten Tomatoes ratings to your Emby library metadata.
- Works with both movies and TV series.
- Updates:
  - `CriticRating` with the Tomatometer / critics score
  - `CommunityRating` with the audience score
- Runs during normal metadata refresh, so there is no separate sync task to manage.

## What you need

- An MDBList API key
- An Emby Server installation that supports this plugin

## Installation

1. Stop Emby Server.
2. Copy `MdbListRatingsProvider.dll` to your Emby `plugins` folder.
3. Start Emby Server.
4. Open the `MDBList Ratings Provider` plugin settings in Emby.
5. Enter your MDBList API key and save.
6. Refresh metadata for your library or individual items.

## Configuration

This plugin has one setting:

- `ApiKey`: your MDBList API key

## Build from source

If you want to build the plugin yourself:

```powershell
dotnet restore
dotnet build -c Release
```

Build output:

- `bin\Release\net8.0\MdbListRatingsProvider.dll`

## For developers

- Target framework: `net8.0`
- Emby package: `mediabrowser.server.core` `4.9.1.80`
- Config UI: `Configuration/config.html`
