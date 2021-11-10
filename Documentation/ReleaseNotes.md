# Release notes

## 2.2.0 (@ time for .Net 6)

Package stays in .Net Standard 2.0, so no big difference, just upgraded Json package reference.

**Fixes**

* Homepage config values allows to filter (whitelist) on key/subkey values. Created bunch of missing unit-tests to safeguard this functionality.

**Overall**

* Updated dependency packages (MS Abstractions - 3.1.21, System.Text.Json - 6.0.0)

## 2.1.1
**Fixes**

* Configuration validation middleware page - Dependecy injection moved to `invoke` method (instead of contructor) as it appears correct way of getting them properly resolved.

## 2.1.0
**Improved**

* Index page - there can be more link buttons added and more generalized.
* `IndexPage.SetHealthPageUrl` and `IndexPage.SetSwaggerUrl` are marked as obsolete. Now `IndexPage.AddLinkButton()` should be used instead.


## 2.0.1
**Improved**

* Index page - when used external content file with links, they are not styled as buttons.

## 2.0.0
*Breaking changes only for Index page functionality.*

**New**

* Added functionality to show actual configuration values (filtered) on index page and their source.
  * \+ Configuration value obfuscation helpers.
* Added functionality to include custom content (file) into index page.

**Improved**

* Setting up Index page in fluent manner. Now only dealing with `IndexPage` object itself.

## 1.1.0

**New**

* Added options setting for ErrorHandler. This allows additionally to filter out elements in stacktrace frames.

**Fixes**

* Made StackTrace retrieval for ErrorHandler more stable.
* Typo fixes in documentation (Intellisense).
## 1.0.0
* Initial release.