using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace StarterSpells;

/// <summary>
/// Small on-screen picker shown in the main menu (hidden in runs):
///   STARTING SPELL - Moon Mage
///   [<]   Laser   [>]
///   F6 / F7 to change - F8 to hide
/// Built from the game's own UI components (IMGUI buttons are stripped from this game).
/// </summary>
public class PickerUI : MonoBehaviour
{
    public PickerUI(IntPtr ptr) : base(ptr) { }

    private GameObject canvasGo, panel;
    private TextMeshProUGUI title, value, hint;
    private float nextScan;
    private bool inMenu, hidden;
    private CharacterId shownCharacter = (CharacterId)(-1);
    private string shownChoice;
    private static Action prevAction, nextAction; // kept alive for IL2CPP
    private static UnityAction prevUnity, nextUnity;

    private static PickerUI instance;

    private float nextErrorLog;

    private void Update()
    {
        instance = this;
        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 0.5f;
            Step("menu check", () =>
            {
                bool menu = UObject.FindObjectOfType<MainMenuManager>() != null;
                bool run = UObject.FindObjectOfType<RunManager>() != null;
                inMenu = menu && !run;
            });
        }

        bool show = inMenu && Plugin.ShowPicker.Value && !hidden;
        if (show && panel == null) Step("build", Build);
        if (canvasGo != null && canvasGo.activeSelf != show) Step("show/hide", () => canvasGo.SetActive(show));

        if (inMenu) Step("keys", HandleKeys);
        if (show && panel != null) Step("refresh", () => Refresh(false));
    }

    private void Step(string what, Action action)
    {
        try { action(); }
        catch (Exception ex)
        {
            if (Time.unscaledTime < nextErrorLog) return; // don't flood the log every frame
            nextErrorLog = Time.unscaledTime + 10f;
            Plugin.Log.LogError($"Picker ({what}): {ex}");
        }
    }

    // ---------- input ----------

    private static bool Pressed(string keyName)
    {
        var kb = Keyboard.current;
        if (kb == null || string.IsNullOrEmpty(keyName)) return false;
        if (!Enum.TryParse(keyName, true, out Key key) || key == Key.None) return false;
        var control = kb[key];
        return control != null && control.wasPressedThisFrame;
    }

    private void HandleKeys()
    {
        if (Pressed(Plugin.ToggleKey.Value)) hidden = !hidden;
        if (hidden || !Plugin.ShowPicker.Value) return;
        if (Pressed(Plugin.PrevKey.Value)) Cycle(-1);
        if (Pressed(Plugin.NextKey.Value)) Cycle(+1);
    }

    private static void Cycle(int dir)
    {
        var character = CurrentCharacter();
        if (!character.HasValue || !Plugin.Choices.TryGetValue(character.Value, out var entry)) return;

        List<string> options = Spells.Options();
        int i = options.IndexOf(entry.Value);
        if (i < 0) i = 0;
        i = (i + dir + options.Count) % options.Count;
        entry.Value = options[i]; // saved to the config file automatically
        instance?.Refresh(true);
    }

    private static CharacterId? CurrentCharacter() => LocalWizard.Current;

    // ---------- display ----------

    private void Refresh(bool force)
    {
        if (panel == null) return;

        var character = CurrentCharacter();
        if (!character.HasValue || !Plugin.Choices.TryGetValue(character.Value, out var entry))
        {
            if (shownChoice == "<none>" && !force) return;
            shownChoice = "<none>";
            shownCharacter = (CharacterId)(-1);
            title.text = "STARTING SPELL";
            value.text = "Pick a wizard first";
            value.color = new Color(1f, 1f, 1f, 0.7f);
            Plugin.Log.LogInfo("Picker: no wizard selected yet.");
            return;
        }

        string choice = entry.Value;
        if (!force && character.Value == shownCharacter && choice == shownChoice) return;
        shownCharacter = character.Value;
        shownChoice = choice;

        title.text = $"STARTING SPELL  -  {Spells.WizardName(character.Value)}";

        var picked = Spells.ChoiceFor(character.Value);
        if (picked.HasValue)
        {
            value.text = Spells.SpellName(picked.Value);
            value.color = new Color(1f, 0.85f, 0.35f);
        }
        else
        {
            var normal = Spells.NormalSpellFor(character.Value);
            value.text = normal.HasValue ? $"Default ({Spells.SpellName(normal.Value)})" : "Default";
            value.color = Color.white;
            if (choice != Plugin.DefaultChoice && Enum.TryParse(choice, out SpellType locked))
                value.text = $"{Spells.SpellName(locked)} (locked)";
        }
        Plugin.Log.LogInfo($"Picker: {title.text} | {value.text}");
    }

    // ---------- building the UI ----------

    private void Build()
    {
        TMP_FontAsset font = null;
        var anyText = UObject.FindObjectOfType<TextMeshProUGUI>();
        if (anyText != null) font = anyText.font;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        if (font == null) return; // try again next frame once the menu's text has loaded

        canvasGo = new GameObject("StarterSpells_Canvas");
        UObject.DontDestroyOnLoad(canvasGo);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        panel = NewRect("Panel", canvasGo.transform);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.05f, 0.14f, 0.85f);
        var pr = panel.GetComponent<RectTransform>();
        pr.sizeDelta = new Vector2(600f, 150f);
        Anchor(pr, Plugin.PickerPosition.Value);

        title = NewText("Title", panel.transform, font, 20f, new Vector2(0f, 46f), new Vector2(580f, 40f));
        title.color = new Color(0.75f, 0.7f, 0.95f);
        value = NewText("Value", panel.transform, font, 30f, new Vector2(0f, 2f), new Vector2(400f, 56f));
        value.enableAutoSizing = true; // long names shrink instead of running under the arrows
        value.fontSizeMin = 18f;
        value.fontSizeMax = 30f;
        hint = NewText("Hint", panel.transform, font, 15f, new Vector2(0f, -48f), new Vector2(580f, 32f));
        hint.color = new Color(1f, 1f, 1f, 0.5f);
        hint.text = $"{Plugin.PrevKey.Value} / {Plugin.NextKey.Value} to change  -  {Plugin.ToggleKey.Value} to hide";

        prevAction ??= () => Cycle(-1);
        nextAction ??= () => Cycle(+1);
        prevUnity ??= DelegateSupport.ConvertDelegate<UnityAction>(prevAction);
        nextUnity ??= DelegateSupport.ConvertDelegate<UnityAction>(nextAction);
        NewButton("Prev", panel.transform, font, "<", new Vector2(-235f, 2f), prevUnity);
        NewButton("Next", panel.transform, font, ">", new Vector2(235f, 2f), nextUnity);

        Refresh(true);
        Plugin.Log.LogInfo("Starting-spell picker ready.");
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font, float size, Vector2 pos, Vector2 box)
    {
        var go = NewRect(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center; // centred both ways
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow; // Ellipsis draws nothing if the line is taller than the box
        t.raycastTarget = false;
        t.color = Color.white;
        return t;
    }

    private static void NewButton(string name, Transform parent, TMP_FontAsset font, string label, Vector2 pos, UnityAction onClick)
    {
        var go = NewRect(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(56f, 48f);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.35f, 0.22f, 0.6f, 0.95f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var t = NewText("Label", go.transform, font, 30f, Vector2.zero, rt.sizeDelta);
        t.text = label;
    }

    private static void Anchor(RectTransform rt, string where)
    {
        Vector2 a = where switch
        {
            "TopLeft" => new Vector2(0f, 1f),
            "TopRight" => new Vector2(1f, 1f),
            "BottomLeft" => new Vector2(0f, 0f),
            "BottomRight" => new Vector2(1f, 0f),
            "BottomCenter" => new Vector2(0.5f, 0f),
            _ => new Vector2(0.5f, 1f),
        };
        rt.anchorMin = a;
        rt.anchorMax = a;
        rt.pivot = a;
        rt.anchoredPosition = new Vector2(a.x == 0f ? 24f : a.x == 1f ? -24f : 0f, a.y == 1f ? -24f : 24f);
    }
}
