# StarterSpells

A [BepInEx](https://github.com/BepInEx/BepInEx) IL2CPP mod for **The Spell Brigade** that lets you pick **any spell as your wizard's starting spell**, including the spells no wizard starts with.

**Download:** [Thunderstore — girthy_mods/StarterSpells](https://thunderstore.io/c/the-spell-brigade/p/girthy_mods/StarterSpells/) (install with r2modman or Gale).

- An in-menu picker: click ◀ ▶ or press F6/F7, and F8 hides it. Each wizard remembers its own pick.
- One dropdown per wizard in your mod manager's config editor.
- The game's wizard info panel previews your pick, and spell names use the game's own translations.
- **Only your wizard changes, so other players don't need the mod.** It works with [MoreWizards](https://github.com/kamikaze-skynet/MoreWizards) and in vanilla lobbies.

See [`thunderstore/README.md`](thunderstore/README.md) for the player-facing description and config options.

## Building

Requirements: [.NET SDK 8](https://dotnet.microsoft.com/download), plus the game with **BepInExPack IL2CPP 6.0.788** installed and launched once.

```powershell
dotnet build src\StarterSpells.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Spell Brigade"
# r2modman/Gale profile:  -p:BepInExDir="$env:APPDATA\r2modmanPlus-local\TheSpellBrigade\profiles\Default\BepInEx"
# copy into BepInEx\plugins: -p:CopyToPlugins=true

.\package.ps1 -GameDir "D:\SteamLibrary\steamapps\common\The Spell Brigade"   # -> dist\girthy_mods-StarterSpells-<version>.zip
```

## How it works

`CharacterResource.GetInitialSpell(skin)` returns a wizard's starting spell: its `InitialSpell`, or `InitialPrestigeSpell` for prestige skins.

The game equips the starting spell in `GameplayPlayerIdentity.InitializeOnClientsRpc` / `Initialize`. The equip only runs where `IsOwner` is true, meaning on that player's own machine, and `SpellEquipper.EquipSpell` adds the spell to an owner-written `NetworkList` that syncs to everyone. So changing your own starting spell needs no networking, and other players don't need the mod.

The mod postfixes `GetInitialSpell`, but only changes the answer inside these contexts:
- **Your own wizard being set up:** `InitializeOnClientsRpc`, `Initialize` and `EquipInitialSpell` on the owner.
- **The main-menu wizard panel:** `StatButtonsPanel.OnWizardButtonClicked`, for the preview.
- **Your own spells asking "am I the starting spell?":** `PlayerSpell.HasLifeStealOnSpell`, so the Vampire Mage's life steal follows your pick.

Other players' wizards are never touched.

`PickerUI` is a `MonoBehaviour` injected with `ClassInjector` (`BasePlugin.AddComponent`). It builds its panel from TextMeshPro text and `UnityEngine.UI.Button`, because IMGUI's `GUI.Button` is stripped from this game. It reads keys through the Unity Input System. Names come from the `SpellName_<SpellType>` and `Character_<CharacterId>` localization keys, falling back to cleaned-up enum names.

## Layout

```
src/Plugin.cs          config (one entry per wizard) and startup
src/StarterPatches.cs  the GetInitialSpell override and its contexts
src/Spells.cs          choices, unlock checks, localized names
src/PickerUI.cs        in-menu picker
thunderstore/          manifest, icon, README, CHANGELOG for the Thunderstore zip
```

## License

[MIT](LICENSE). The Spell Brigade is © Bolt Blaster Games. This is an unofficial fan mod.
