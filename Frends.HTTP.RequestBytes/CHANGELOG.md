# Changelog

## [1.0.2] - 2024-08-19
### Changed
- Removed handling where only PATCH, PUT, POST and DELETE requests were allowed to have the Content-Type header and content, due to HttpClient failing if e.g., a GET request had content. HttpClient has since been updated to tolerate such requests.

## [1.0.1] - 2024-01-17
### Fixed
- Fixed issues which CodeQL found in the codebase.
 Fixed Code Coverage

## [1.0.0] - 2023-01-27
### Added
- Initial implementation of Frends.HTTP.RequestBytes.
