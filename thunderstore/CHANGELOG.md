# Changelog

## 1.0.1
- Fix: the picker showed its arrows but no wizard or spell. The selected wizard is now remembered from the game's own selection calls, with fallbacks, and the log explains if it can't be found.
- The picker shows "Pick a wizard first" instead of blank text.
- Moved the picker to the bottom center by default, so it no longer covers the top menu bar. The setting is now `[Picker] ScreenPosition`.

## 1.0.0
- First release: pick any of the 25 spells as each wizard's starting spell, using an in-menu picker (click or F6/F7) or the config file.
