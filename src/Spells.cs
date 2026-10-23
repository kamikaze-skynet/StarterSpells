using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Localization.Settings;

namespace StarterSpells;

/// <summary>Spell choices, unlock checks and display names.</summary>
internal static class Spells
{
    /// <summary>The chosen spell for a wizard, or null for the wizard's normal spell.</summary>
    internal static SpellType? ChoiceFor(CharacterId character)
    {
        if (!Plugin.Choices.TryGetValue(character, out var entry)) return null;
        string v = entry.Value;
        if (string.IsNullOrEmpty(v) || v == Plugin.DefaultChoice) return null;
        if (!Enum.TryParse(v, out SpellType spell)) return null;
        if (!Plugin.AllowLockedSpells.Value && IsLocked(spell)) return null;
        return spell;
    }

    internal static bool IsLocked(SpellType spell)
    {
        try
        {
            var config = SpellConfigurationRepository.Get(spell);
            return config != null && config.IsLockedByProgression();
        }
        catch { return false; }
    }

    /// <summary>"Default" followed by every spell the player may pick.</summary>
    internal static List<string> Options()
    {
        var list = new List<string> { Plugin.DefaultChoice };
        foreach (SpellType s in Enum.GetValues(typeof(SpellType)))
            if (Plugin.AllowLockedSpells.Value || !IsLocked(s)) list.Add(s.ToString());
        return list;
    }

    internal static SpellType? NormalSpellFor(CharacterId character)
    {
        try
        {
            var res = CharacterResourceRepository.Get(character);
            var skin = LocalWizard.Player?.GetSelectedSkin() ?? res.DefaultSkin;
            StarterPatches.Bypass = true;
            try { return res.GetInitialSpell(skin); }
            finally { StarterPatches.Bypass = false; }
        }
        catch { return null; }
    }

    private static readonly Dictionary<string, string> nameCache = new();

    internal static string SpellName(SpellType spell) => Localized("SpellName_" + spell, Pretty(spell.ToString()));
    internal static string WizardName(CharacterId c) => Localized("Character_" + c, Pretty(c.ToString()));

    private static string Localized(string key, string fallback)
    {
        if (nameCache.TryGetValue(key, out var cached)) return cached;
        string result = null;
        try
        {
            var db = LocalizationSettings.StringDatabase;
            if (db != null) result = db.GetLocalizedString(key);
        }
        catch { }

        bool ok = !string.IsNullOrWhiteSpace(result) && result != key && !result.Contains("No translation found");
        if (ok) nameCache[key] = result; // only cache real names; retry later if localization wasn't ready
        return ok ? result : fallback;
    }

    /// <summary>"BombLauncher" -> "Bomb Launcher", "Smg" -> "SMG".</summary>
    private static string Pretty(string id)
    {
        if (id == "Smg") return "SMG";
        var sb = new StringBuilder();
        for (int i = 0; i < id.Length; i++)
        {
            if (i > 0 && char.IsUpper(id[i]) && !char.IsUpper(id[i - 1])) sb.Append(' ');
            sb.Append(id[i]);
        }
        return sb.ToString();
    }
}
