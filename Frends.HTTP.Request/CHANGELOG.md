# Changelog

## [1.2.0] - 2024-08-19
### Changed
- Removed handling where only PATCH, PUT, POST and DELETE requests were allowed to have the Content-Type header and content, due to HttpClient failing if e.g., a GET request had content. HttpClient has since been updated to tolerate such requests.

## [1.1.2] - 2024-01-16
### Fixed
- Fixed misleading documentation. 

## [1.1.1] - 2023-06-09
### Fixed
- Fixed issue with terminating the Task by adding cancellationToken to the method ReadAsStringAsync when JToken as ReturnFormat. 

## [1.1.0] - 2023-05-08
### Changed
- [Breaking] Changed ResultMethod to ReturnFormat which describes the parameter better. 
- Fixed documentation link in the main method.

## [1.0.0] - 2023-01-24
### Added
- Initial implementation of Frends.HTTP.Request.
