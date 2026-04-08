# AGENTS.md

Guidance for coding agents and contributors working in this repository.

## Scope

- Keep this project as a dedicated Emby metadata provider plugin.
- Do not add scheduled-task, sync-daemon, or standalone CLI behavior unless explicitly requested.

## Technical constraints

- Target framework: `net8.0`
- Primary package: `mediabrowser.server.core` (currently `4.9.1.80`)
- Keep `Configuration/config.html` embedded in the project file.

## Versioning behavior

- Emby can cache plugin assemblies aggressively.
- When plugin code or embedded assets change, bump plugin version metadata in `MdbListRatingsProvider.csproj`:
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`

## Metadata mapping contract

- MDBList `ratings.rt_score` -> Emby `CriticRating`
- MDBList `ratings.rt_audience_score` -> Emby `CommunityRating`

Do not change this mapping without explicit approval.

## Resilience expectations

- If MDBList returns `HTTP 429`, log a warning and return an empty metadata result.
- Avoid throwing for recoverable API errors; let other Emby providers continue.

## Coding style

- Use clear, minimal implementations.
- Keep dependencies small and aligned with Emby APIs.
- Preserve plugin identity values unless intentionally versioning/forking:
  - Plugin Name: `MDBList Ratings Provider`
  - Plugin Id: `2f0cbf4d-249d-4f22-b451-2f2e1766fb7e`

## Validation checklist

Before finishing changes, run:

```powershell
dotnet build -c Release
```

Confirm:

- Build succeeds with zero errors.
- `config.html` is still embedded as a manifest resource.
