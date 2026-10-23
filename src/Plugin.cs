using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace StarterSpells;

/// <summary>
/// Pick any spell as your wizard's starting spell, including spells no wizard starts with.
///
/// The game equips a starting spell only on the owning player's own machine (GameplayPlayerIdentity, IsOwner),
/// then syncs it to everyone through the networked spell list. So this mod only changes your own wizard,
/// works when other players don't have it, and needs no network messages. Compatible with MoreWizards.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BasePlugin
{
    public const string Guid = "girthy_mods.StarterSpells";
    public const string Name = "StarterSpells";
    public const string Version = "1.0.1";

    internal static new ManualLogSource Log;

    internal const string DefaultChoice = "Default";

    internal static readonly Dictionary<CharacterId, ConfigEntry<string>> Choices = new();
    internal static ConfigEntry<bool> AllowLockedSpells;
    internal static ConfigEntry<bool> ShowPicker;
    internal static ConfigEntry<string> PrevKey;
    internal static ConfigEntry<string> NextKey;
    internal static ConfigEntry<string> ToggleKey;
    internal static ConfigEntry<string> PickerPosition;

    public override void Load()
    {
        Log = base.Log;

        var options = new List<string> { DefaultChoice };
        foreach (SpellType s in Enum.GetValues(typeof(SpellType))) options.Add(s.ToString());
        var list = new AcceptableValueList<string>(options.ToArray());

        foreach (CharacterId c in Enum.GetValues(typeof(CharacterId)))
        {
            if (c == CharacterId.TestCharacter) continue;
            Choices[c] = Config.Bind("Starting Spells", c.ToString(), DefaultChoice,
                new ConfigDescription($"Starting spell for {c}. Default = the wizard's normal spell.", list));
        }

        AllowLockedSpells = Config.Bind("General", "AllowLockedSpells", false,
            "Allow starting with spells you haven't unlocked yet. Off = only spells you've unlocked.");
        ShowPicker = Config.Bind("Picker", "ShowPicker", true,
            "Show the starting-spell picker in the main menu.");
        PickerPosition = Config.Bind("Picker", "ScreenPosition", "BottomCenter",
            new ConfigDescription("Where the picker sits on screen.",
                new AcceptableValueList<string>("TopCenter", "TopLeft", "TopRight", "BottomLeft", "BottomRight", "BottomCenter")));
        PrevKey = Config.Bind("Picker", "PreviousKey", "F6", "Key for the previous spell (Unity Input System key name).");
        NextKey = Config.Bind("Picker", "NextKey", "F7", "Key for the next spell.");
        ToggleKey = Config.Bind("Picker", "ToggleKey", "F8", "Key to show or hide the picker.");

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(StarterPatches));
        harmony.PatchAll(typeof(LocalWizard));
        AddComponent<PickerUI>();

        Log.LogInfo($"{Name} {Version} ready.");
    }
}
