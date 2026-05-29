using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GoodSamaritanNpc;

public sealed class GoodSamaritanModMenuPage
{
    public string Id { get; }
    public string Title { get; }
    internal Action DrawAction { get; }

    public GoodSamaritanModMenuPage(string id, string title, Action drawAction)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Title = string.IsNullOrWhiteSpace(title) ? Id : title;
        DrawAction = drawAction ?? (() => { });
    }

    public void Draw()
    {
        DrawAction();
    }
}

public static class GoodSamaritanModMenuApi
{
    private static readonly List<GoodSamaritanModMenuPage> RegisteredPages = new();

    public static IReadOnlyList<GoodSamaritanModMenuPage> Pages => RegisteredPages;

    public static void RegisterPage(string id, string title, Action drawAction)
    {
        if (drawAction == null)
        {
            return;
        }

        for (int i = 0; i < RegisteredPages.Count; i++)
        {
            if (string.Equals(RegisteredPages[i].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                RegisteredPages[i] = new GoodSamaritanModMenuPage(id, title, drawAction);
                return;
            }
        }

        RegisteredPages.Add(new GoodSamaritanModMenuPage(id, title, drawAction));
    }
}

public sealed class GoodSamaritanMenuController : MonoBehaviour
{
    private const int WindowId = 739102;

    private GameObject configButtonObject;
    private Rect windowRect = new(90f, 70f, 620f, 680f);
    private Vector2 scroll;
    private float nextButtonSearchTime;
    private bool isOpen;
    private bool registeredBuiltinPage;
    private int selectedPage;

    public GoodSamaritanMenuController(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        RegisterBuiltinPage();
    }

    public void Update()
    {
        RegisterBuiltinPage();

        if (Input.GetKeyDown(KeyCode.F8))
        {
            isOpen = !isOpen;
        }

        if (configButtonObject != null)
        {
            return;
        }

        if (Time.unscaledTime < nextButtonSearchTime)
        {
            return;
        }

        nextButtonSearchTime = Time.unscaledTime + 1f;
        TryInstallMainMenuButton();
    }

    public void OnGUI()
    {
        if (!isOpen)
        {
            return;
        }

        windowRect = GUI.Window(WindowId, windowRect, (GUI.WindowFunction)DrawWindow, "Mod Config");
    }

    private void RegisterBuiltinPage()
    {
        if (registeredBuiltinPage)
        {
            return;
        }

        GoodSamaritanModMenuApi.RegisterPage("good_samaritan_npc", "Good Samaritan NPC", GoodSamaritanConfigGui.Draw);
        registeredBuiltinPage = true;
    }

    private void ToggleWindow()
    {
        isOpen = !isOpen;
    }

    private void TryInstallMainMenuButton()
    {
        var playButton = FindPlayButton();
        if (playButton == null)
        {
            return;
        }

        var parent = playButton.transform.parent;
        if (parent == null)
        {
            return;
        }

        var clone = Object.Instantiate(((Component)playButton).gameObject, parent);
        clone.name = "GoodSamaritanModConfigButton";
        configButtonObject = clone;

        int sibling = playButton.transform.GetSiblingIndex();
        clone.transform.SetSiblingIndex(Mathf.Min(parent.childCount - 1, sibling + 1));

        var srcRect = ((Component)playButton).GetComponent<RectTransform>();
        var cloneRect = clone.GetComponent<RectTransform>();
        if (srcRect != null && cloneRect != null)
        {
            cloneRect.anchorMin = srcRect.anchorMin;
            cloneRect.anchorMax = srcRect.anchorMax;
            cloneRect.pivot = srcRect.pivot;
            cloneRect.sizeDelta = srcRect.sizeDelta;
            cloneRect.anchoredPosition = srcRect.anchoredPosition + new Vector2(0f, -Mathf.Max(42f, srcRect.rect.height + 8f));
        }

        SetButtonLabel(clone, "Mods");

        var button = clone.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((UnityAction)ToggleWindow);
        }
    }

    private static Button FindPlayButton()
    {
        var buttons = Object.FindObjectsOfType<Button>();
        if (buttons == null)
        {
            return null;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null || ((Component)button).gameObject.name == "GoodSamaritanModConfigButton")
            {
                continue;
            }

            string label = GetButtonLabel(((Component)button).gameObject);
            if (string.Equals(label, "Play", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(label, "开始", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(label, "开始游戏", StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return null;
    }

    private static string GetButtonLabel(GameObject go)
    {
        var tmp = go.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            return tmp.text?.Trim() ?? string.Empty;
        }

        var text = go.GetComponentInChildren<Text>(true);
        return text == null ? string.Empty : text.text.Trim();
    }

    private static void SetButtonLabel(GameObject go, string label)
    {
        var tmp = go.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
        }

        var text = go.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private void DrawWindow(int id)
    {
        var pages = GoodSamaritanModMenuApi.Pages;
        if (pages.Count == 0)
        {
            GUILayout.Label("No mod config pages registered.");
            GUI.DragWindow();
            return;
        }

        selectedPage = Mathf.Clamp(selectedPage, 0, pages.Count - 1);

        GUILayout.BeginHorizontal();
        for (int i = 0; i < pages.Count; i++)
        {
            if (GUILayout.Toggle(selectedPage == i, pages[i].Title, GUI.skin.button, GUILayout.Height(30f)))
            {
                selectedPage = i;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        scroll = GUILayout.BeginScrollView(scroll);
        pages[selectedPage].Draw();
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", GUILayout.Width(110f), GUILayout.Height(30f)))
        {
            isOpen = false;
        }
        GUILayout.EndHorizontal();

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 28f));
    }
}

internal static class GoodSamaritanConfigGui
{
    internal static void Draw()
    {
        var s = GoodSamaritanPlugin.Settings;
        bool changed = false;

        GUILayout.Label("NPC witness behavior");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Easy", GUILayout.Height(28f)))
        {
            s.ApplyPreset("Easy");
            changed = true;
        }
        if (GUILayout.Button("Normal", GUILayout.Height(28f)))
        {
            s.ApplyPreset("Normal");
            changed = true;
        }
        if (GUILayout.Button("Hard", GUILayout.Height(28f)))
        {
            s.ApplyPreset("Hard");
            changed = true;
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"Current preset: {s.NpcSuspicionPreset.Value}");

        changed |= Toggle(s.Enabled, "Enabled");
        changed |= Toggle(s.ConvertExistingNpcs, "Convert existing NPCs");
        changed |= Slider(s.ExistingNpcChance, "Existing NPC chance", 0f, 1f);
        changed |= IntSlider(s.ExtraSpawnCount, "Extra spawn count", 0, 20);
        changed |= Slider(s.ScanIntervalSeconds, "Scan interval", 0.1f, 5f);
        changed |= Slider(s.WitnessRadius, "Witness radius", 3f, 40f);
        changed |= Slider(s.WitnessFovDegrees, "Witness FOV", 20f, 180f);
        changed |= Slider(s.ReportCooldownSeconds, "Report cooldown", 1f, 45f);
        changed |= Slider(s.TargetCooldownSeconds, "Target cooldown", 1f, 60f);
        changed |= Slider(s.HighlightSeconds, "Highlight seconds", 0.5f, 12f);

        GUILayout.Space(8f);
        GUILayout.Label("Detection rules");
        changed |= ToggleCustom(s.EnableDirectTargetReports, "Direct target reports");
        changed |= ToggleCustom(s.DetectRevealingActions, "Reveal actions");
        changed |= ToggleCustom(s.DetectCarriedContraband, "Carried contraband");
        changed |= ToggleCustom(s.DetectHiddenContraband, "Hidden contraband");
        changed |= ToggleCustom(s.DetectContrabandPickup, "Contraband pickup");
        changed |= ToggleCustom(s.DetectCivilianAttacks, "Civilian attacks");
        changed |= ToggleCustom(s.DetectJumping, "Jumping");
        changed |= ToggleCustom(s.DetectLineCutting, "Likely line cutting");

        GUILayout.Space(8f);
        GUILayout.Label("Playable witnesses");
        changed |= Toggle(s.EnablePlayableWitnessPlayers, "Enable playable witness players");
        changed |= IntSlider(s.MaxPlayableWitnessPlayers, "Max playable witnesses", 0, 8);
        changed |= Slider(s.PlayableWitnessChance, "Playable witness chance", 0f, 1f);

        GUILayout.Space(8f);
        GUILayout.Label("Client feedback");
        changed |= Toggle(s.EnableCustomClientMarker, "Custom marker");
        changed |= Toggle(s.EnableVoiceLine, "Voice line");
        changed |= TextField(s.Language, "Language");

        if (changed)
        {
            s.Save();
        }
    }

    private static bool Toggle(ConfigEntry<bool> entry, string label)
    {
        bool next = GUILayout.Toggle(entry.Value, $"{label}: {(entry.Value ? "On" : "Off")}");
        if (next == entry.Value)
        {
            return false;
        }

        entry.Value = next;
        return true;
    }

    private static bool ToggleCustom(ConfigEntry<bool> entry, string label)
    {
        bool changed = Toggle(entry, label);
        if (changed)
        {
            GoodSamaritanPlugin.Settings.NpcSuspicionPreset.Value = "Custom";
        }

        return changed;
    }

    private static bool Slider(ConfigEntry<float> entry, string label, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {entry.Value:0.##}", GUILayout.Width(230f));
        float next = GUILayout.HorizontalSlider(entry.Value, min, max);
        GUILayout.EndHorizontal();

        next = Mathf.Round(next * 100f) / 100f;
        if (Mathf.Approximately(next, entry.Value))
        {
            return false;
        }

        entry.Value = next;
        return true;
    }

    private static bool IntSlider(ConfigEntry<int> entry, string label, int min, int max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {entry.Value}", GUILayout.Width(230f));
        int next = Mathf.RoundToInt(GUILayout.HorizontalSlider(entry.Value, min, max));
        GUILayout.EndHorizontal();

        if (next == entry.Value)
        {
            return false;
        }

        entry.Value = next;
        return true;
    }

    private static bool TextField(ConfigEntry<string> entry, string label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(230f));
        string next = GUILayout.TextField(entry.Value ?? string.Empty);
        GUILayout.EndHorizontal();

        if (string.Equals(next, entry.Value, StringComparison.Ordinal))
        {
            return false;
        }

        entry.Value = next;
        return true;
    }
}
