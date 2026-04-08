# MDBList Ratings Provider (Emby)

`MDBList Ratings Provider` is an Emby metadata plugin that fetches Rotten Tomatoes ratings from MDBList during normal library metadata refresh.

## What it does

- Implements standard remote metadata providers for:
  - `Movie`
  - `Series`
- Maps MDBList ratings to Emby fields:
  - `rt_score` (Critic) -> `CriticRating`
  - `tomatoes_audience` (Audience) -> `CommunityRating` (Scaled 0-10)
- Uses modern RESTful API routes (`/imdb/movie/` and `/imdb/show/`).
- Handles `HTTP 429` rate limiting gracefully by returning an empty result to allow other providers to proceed.
- Provides detailed diagnostic logging in `embyserver.txt` for troubleshooting lookup failures.

## Requirements

- .NET SDK `8.0+`
- Emby Server compatible with `mediabrowser.server.core` `4.9.1.80`
- MDBList API key

## Build

```powershell
dotnet restore
dotnet build -c Release
```

Output:

- `bin\Release\net8.0\MdbListRatingsProvider.dll`

## Install in Emby

1. Stop Emby Server.
2. Copy `MdbListRatingsProvider.dll` to your Emby `plugins` folder.
3. Start Emby Server.
4. Go to plugin settings for `MDBList Ratings Provider`.
5. Enter your MDBList API key and save.
6. In library metadata settings, enable/prioritize this provider as desired.

## Configuration

The plugin exposes one setting:

- `ApiKey`: MDBList API key used for rating lookups.

## Project structure

- `Plugin.cs`: plugin entry point + web config page registration
- `Configuration.cs`: plugin configuration model
- `MdbListProvider.cs`: movie/series metadata provider implementation
- `Configuration/config.html`: plugin config UI
- `MdbListRatingsProvider.csproj`: project definition

## Notes

- This plugin is designed as a metadata provider, not a scheduled task or sync process.
- Networking uses Emby's `IHttpClient` abstraction for runtime compatibility.
