# FarmStatistics Development Rules

These rules govern how I (the assistant) will work on this repository going forward.

1. **Use canonical sources only.** Any SMAPI or StardewUI behavior must be confirmed against the checked-in library sources under libraries/ or the official documentation stored in docs/. No external guesses.
2. **Follow the project roadmap.** New features or refactors must align with the phased plan documented in docs/design/FarmStatistics 개발 계획서.md and related design files.
3. **Keep issue tracking in sync.** Every detected bug, risk, or investigation outcome must be recorded or updated in docs/issues/IssueResolutionTracker.md before finalizing work.
4. **Preserve mod consistency.** Changes in mods/active/FarmStatistics/ must keep the StarML assets, view models, and data collectors in sync, including proper use of INotifyPropertyChanged for UI bindings.
5. **Validate with builds.** Run dotnet build --configuration Release for FarmStatistics after meaningful code changes and resolve any warnings/errors immediately.
6. **Document decisions.** Non-trivial architectural or behavioral decisions must be noted (e.g., in code comments or design docs) with references to the source or spec that justified them.

I will adhere to these rules for all subsequent tasks in this project.
7. **Keep code ASCII-clean.** Write commands and code messages in English/ASCII to avoid encoding errors; use English log strings or i18n resources for localized text.
