# Changelog

## [1.0.1.12] - 2026-04-09

### Fixed
- Added support for the `popcorn` rating source from MDBList to ensure Rotten Tomatoes audience scores are correctly mapped to Emby `CommunityRating`.
- Corrected parsing logic to prefer the newer `ratings` array while maintaining fallback to root properties.

### Added
- Added custom plugin thumbnail (`thumb.png`) to improve visibility in the Emby dashboard.

## [1.0.1.9] - 2026-04-08

### Added
- Robust diagnostic logging for MDBList API requests and JSON responses.
- Masked API key logging to protect user credentials while debugging.

### Fixed
- Graceful handling of HTTP 429 (Rate Limit) errors; provider now fail-silently with a warning instead of throwing an exception.
- Advanced JSON parsing for the MDBList `ratings` array format.
- Normalized Rotten Tomatoes Audience scores (scaled 0-100 down to 0-10) for correct Emby Community Rating display.
- Migrated to modern RESTful API routes (`/imdb/movie/{id}` and `/imdb/show/{id}`).
- Fixed configuration save button by binding to the modern Emby `BaseView` lifecycle.
- Resolved "underlay" rendering issue in Emby UI by modernizing the HTML/JS configuration structure.

### Changed
- Bumped version to `1.0.1.9` for final deployment.
