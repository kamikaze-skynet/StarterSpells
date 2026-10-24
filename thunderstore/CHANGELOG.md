## 1.1.1
- **Host's rules for everyone:** `AllowLockedSpells` now comes from the host while you're in their game. Your own your own starting spells and the picker stay yours. Your config file isn't changed, and your setting is back when you leave. If the host doesn't have the mod, locked spells stay locked.

# Changelog

## 1.1.0
- The picker shows each spell's icon next to its name, using the game's own icons. Default shows your wizard's normal spell icon, and locked spells show dimmed.

## 1.0.2
- Fix: the picker's text (wizard, spell, hint) was invisible. The text boxes were shorter than the game font's line height, and TextMeshPro's ellipsis mode then draws nothing. Text now always draws, with roomier boxes, and long spell names shrink to fit between the arrows.
- The log now shows what the picker displays (`Picker: ...`).

## 1.0.1
- Fix: the picker showed its arrows but no wizard or spell. The selected wizard is now remembered from the game's own selection calls, with fallbacks, and the log explains if it can't be found.
- The picker shows "Pick a wizard first" instead of blank text.
- Moved the picker to the bottom center by default, so it no longer covers the top menu bar. The setting is now `[Picker] ScreenPosition`.

## 1.0.0
- First release: pick any of the 25 spells as each wizard's starting spell, using an in-menu picker (click or F6/F7) or the config file.
