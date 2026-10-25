using System;
using HarmonyLib;
using UObject = UnityEngine.Object;

namespace StarterSpells;

/// <summary>
/// Which wizard the local player has selected.
/// Three sources, most reliable first:
///  1. remembered from the game's own LocalPlayer.SetSelectedCharacter / LoadSelectedCharacter calls,
///  2. SingletonPersistent&lt;LocalPlayer&gt;.Instance,
///  3. FindObjectOfType&lt;LocalPlayer&gt;() (cached; at most once a second).
/// Logs once if none work, so the log says why the picker can't show a wizard.
/// </summary>
[HarmonyPatch]
internal static class LocalWizard
{
    private static LocalPlayer player;
    private static CharacterId? lastSelected;
    private static float nextFind;
    private static bool loggedFailure;

    internal static LocalPlayer Player
    {
        get
        {
            if (player != null && !player.WasCollected) return player;
            player = null;
            try { player = SingletonPersistent<LocalPlayer>.Instance; } catch { }
            if (player == null && UnityEngine.Time.unscaledTime >= nextFind)
            {
                nextFind = UnityEngine.Time.unscaledTime + 1f;
                try { player = UObject.FindObjectOfType<LocalPlayer>(); } catch { }
            }
            return player;
        }
    }

    internal static CharacterId? Current
    {
        get
        {
            var p = Player;
            bool ready = false;
            try { ready = p != null && p.IsInitialized; } catch { }
            if (ready) // asking before the profile is loaded makes the game log an error
            {
                try { return lastSelected = p.GetSelectedCharacter(); }
                catch (Exception ex) { LogOnce($"reading the selected wizard failed: {ex.Message}"); }
            }
            if (lastSelected.HasValue) return lastSelected;
            if (p == null) LogOnce("the game's LocalPlayer object wasn't found yet");
            return null;
        }
    }

    private static void LogOnce(string why)
    {
        if (loggedFailure) return;
        loggedFailure = true;
        Plugin.Log.LogWarning($"Picker can't tell which wizard is selected: {why}. It will keep trying.");
    }

    // Remember the selection whenever the game sets or loads it.
    [HarmonyPatch(typeof(LocalPlayer), nameof(LocalPlayer.SetSelectedCharacter))]
    [HarmonyPostfix]
    private static void SetSelected(LocalPlayer __instance, CharacterId character)
    {
        player = __instance;
        lastSelected = character;
    }

    [HarmonyPatch(typeof(LocalPlayer), nameof(LocalPlayer.LoadSelectedCharacter))]
    [HarmonyPostfix]
    private static void LoadSelected(LocalPlayer __instance)
    {
        player = __instance;
        try { if (__instance.IsInitialized) lastSelected = __instance.GetSelectedCharacter(); } catch { }
    }
}
