using System;
using HarmonyLib;
using Unity.Netcode;

namespace StarterSpells;

/// <summary>
/// CharacterResource.GetInitialSpell is called from a few places. We only change its answer while:
///  - the local player's own wizard is being set up (GameplayPlayerIdentity on the owner's machine),
///  - the main menu's wizard info panel is showing a wizard (so the panel previews your pick),
///  - one of your own spells checks "is this my starting spell" (e.g. the vampire's life steal follows your pick).
/// Other players' wizards are never touched.
/// </summary>
[HarmonyPatch]
internal static class StarterPatches
{
    [ThreadStatic] private static int context;
    [ThreadStatic] internal static bool Bypass;

    private static bool IsLocalOwner(NetworkBehaviour nb)
    {
        try
        {
            if (nb.IsOwner) return true;
            var nm = NetworkManager.Singleton;
            return nm == null || !nm.IsListening; // offline: the only wizard is ours
        }
        catch { return false; }
    }

    // ---------- the swap ----------

    [HarmonyPatch(typeof(CharacterResource), nameof(CharacterResource.GetInitialSpell))]
    [HarmonyPostfix]
    private static void GetInitialSpell_Postfix(CharacterResource __instance, ref SpellType __result)
    {
        if (context <= 0 || Bypass) return;
        try
        {
            var choice = Spells.ChoiceFor(__instance.Id);
            if (choice.HasValue) __result = choice.Value;
        }
        catch (Exception ex) { Plugin.Log.LogError($"GetInitialSpell: {ex}"); }
    }

    // ---------- contexts: your own wizard being set up ----------

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.InitializeOnClientsRpc))]
    [HarmonyPrefix]
    private static void InitRpc_Prefix(GameplayPlayerIdentity __instance, out bool __state)
    {
        __state = IsLocalOwner(__instance);
        if (__state) context++;
    }

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.InitializeOnClientsRpc))]
    [HarmonyPostfix]
    private static void InitRpc_Postfix(GameplayPlayerIdentity __instance, bool __state)
    {
        if (!__state) return;
        context = Math.Max(0, context - 1);
        LogEquip(__instance);
    }

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.Initialize))]
    [HarmonyPrefix]
    private static void Init_Prefix(GameplayPlayerIdentity __instance, out bool __state)
    {
        __state = IsLocalOwner(__instance);
        if (__state) context++;
    }

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.Initialize))]
    [HarmonyPostfix]
    private static void Init_Postfix(GameplayPlayerIdentity __instance, bool __state)
    {
        if (!__state) return;
        context = Math.Max(0, context - 1);
        LogEquip(__instance);
    }

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.EquipInitialSpell))]
    [HarmonyPrefix]
    private static void Equip_Prefix(GameplayPlayerIdentity __instance, out bool __state)
    {
        __state = IsLocalOwner(__instance);
        if (__state) context++;
    }

    [HarmonyPatch(typeof(GameplayPlayerIdentity), nameof(GameplayPlayerIdentity.EquipInitialSpell))]
    [HarmonyPostfix]
    private static void Equip_Postfix(bool __state)
    {
        if (__state) context = Math.Max(0, context - 1);
    }

    // ---------- context: your own spells asking "am I the starting spell?" ----------

    [HarmonyPatch(typeof(PlayerSpell), nameof(PlayerSpell.HasLifeStealOnSpell))]
    [HarmonyPrefix]
    private static void LifeSteal_Prefix(PlayerSpell __instance, out bool __state)
    {
        __state = IsLocalOwner(__instance);
        if (__state) context++;
    }

    [HarmonyPatch(typeof(PlayerSpell), nameof(PlayerSpell.HasLifeStealOnSpell))]
    [HarmonyPostfix]
    private static void LifeSteal_Postfix(bool __state)
    {
        if (__state) context = Math.Max(0, context - 1);
    }

    // ---------- context: main menu wizard info panel (preview) ----------

    [HarmonyPatch(typeof(StatButtonsPanel), nameof(StatButtonsPanel.OnWizardButtonClicked))]
    [HarmonyPrefix]
    private static void WizardPanel_Prefix() => context++;

    [HarmonyPatch(typeof(StatButtonsPanel), nameof(StatButtonsPanel.OnWizardButtonClicked))]
    [HarmonyPostfix]
    private static void WizardPanel_Postfix() => context = Math.Max(0, context - 1);

    private static void LogEquip(GameplayPlayerIdentity identity)
    {
        try
        {
            var res = identity.Character;
            if (res == null) return;
            var choice = Spells.ChoiceFor(res.Id);
            if (choice.HasValue) Plugin.Log.LogInfo($"Starting spell for {res.Id}: {choice.Value}");
        }
        catch { }
    }
}
