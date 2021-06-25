# Release notes

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