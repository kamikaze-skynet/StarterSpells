# StarterSpells

Choose your wizard's **starting spell** in The Spell Brigade, including the spells **no wizard starts with**.

## How to use

In the main menu, a small panel at the bottom of the screen shows your current wizard and their starting spell:

```
STARTING SPELL  -  Moon Mage
   [<]  [icon]  Bomb Launcher  [>]
F6 / F7 to change  -  F8 to hide
```

- Click the arrows, or press **F6 / F7**, to cycle through the spells.
- Each wizard remembers their own pick. Choose **Default** for the wizard's normal spell.
- The wizard info panel in the menu previews the spell you picked.
- The panel only appears in the main menu, never during a run.

You can also set picks in your mod manager's config editor (or `BepInEx\config\girthy_mods.StarterSpells.cfg`): there's one dropdown per wizard under **Starting Spells**.

## Multiplayer

- **Only your own wizard changes.** The game equips each player's starting spell on that player's own machine, then syncs it to everyone, so other players don't need this mod.
- **Works with [MoreWizards](https://thunderstore.io/c/the-spell-brigade/p/girthy_mods/MoreWizards/)** and with vanilla lobbies.

## Config

| Section | Setting | Default | What it does |
|---|---|---|---|
| Starting Spells | *(one per wizard)* | Default | Starting spell for that wizard |
| General | AllowLockedSpells | false | Allow spells you haven't unlocked yet |
| Picker | ShowPicker | true | Show the picker in the main menu |
| Picker | ScreenPosition | BottomCenter | BottomCenter, BottomLeft, BottomRight, TopCenter, TopLeft or TopRight |
| Picker | PreviousKey / NextKey | F6 / F7 | Keys to cycle spells (Unity Input System key names) |
| Picker | ToggleKey | F8 | Show or hide the picker |

## Notes

- Spell names use the game's own translations, so they appear in your language.
- A wizard's passive that refers to "your starting spell" (like the Vampire Mage's life steal) follows your pick.
- Locked spells are skipped unless `AllowLockedSpells` is on.
