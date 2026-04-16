# Changelog

## [1.9.0] - 2026-04-15

### Fixed

- Fixed an issue where selecting JToken as the return format would throw an exception if the server returned a non-JSON response; the response body is now returned as a raw string instead, preserving the status code for flow control.

## [1.8.0] - 2026-03-07

### Fixed

- Improve the handling of disposable objects to avoid problems with timeouts related to HttpClient.

### Added

- Added an option "CacheHttpClient" to disable caching httpClient. (there were cached by default)

## [1.7.0] - 2026-03-03

### Added

- Added CertificateStoreLocation option to allow selection between CurrentUser and LocalMachine certificate stores when
  using certificate authentication.

## [1.6.0] - 2026-01-27

### Fixed

- GET requests ignore message body content

## [1.5.0] - 2025-10-03

### Changed

- Changed default return format from String to JToken

## [1.4.0] - 2025-03-25

### Changed

- Update packages:
  Newtonsoft.Json 12.0.1 -> 13.0.3
  System.DirectoryServices 7.0.0 -> 9.0.3
  System.Runtime.Caching 7.0.0 -> 9.0.3
  coverlet.collector 3.1.0 -> 6.0.4
  Microsoft.NET.Test.Sdk 16.7.0 -> 17.13.0
  MSTest.TestAdapter 2.1.2 -> 3.8.3
  MSTest.TestFramework 2.1.2 -> 3.8.3
  nunit 3.12.0 -> 4.3.2
  NUnit3TestAdapter 3.17.0 -> 5.0.0
  RichardSzalay.MockHttp 6.0.0 -> 7.0.0
  xunit.extensibility.core 2.4.2 -> 2.9.3

## [1.3.0] - 2024-12-30

### Changed

- Descriptions of ClientCertificate suboptions includes clearer information about usage in terms of different types of
  ClientCertificate.

## [1.2.0] - 2024-08-19

### Changed

- Removed handling where only PATCH, PUT, POST and DELETE requests were allowed to have the Content-Type header and
  content, due to HttpClient failing if e.g., a GET request had content. HttpClient has since been updated to tolerate
  such requests.

## [1.1.2] - 2024-01-16

### Fixed

- Fixed misleading documentation.

## [1.1.1] - 2023-06-09

### Fixed

- Fixed issue with terminating the Task by adding cancellationToken to the method ReadAsStringAsync when JToken as
  ReturnFormat.

## [1.1.0] - 2023-05-08

### Changed

- [Breaking] Changed ResultMethod to ReturnFormat which describes the parameter better.
- Fixed documentation link in the main method.

## [1.0.0] - 2023-01-24

### Added

- Initial implementation of Frends.HTTP.Request.
