using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
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
    internal Action<GoodSamaritanModMenuBuilder> BuildAction { get; }

    public GoodSamaritanModMenuPage(string id, string title, Action<GoodSamaritanModMenuBuilder> buildAction)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Title = string.IsNullOrWhiteSpace(title) ? Id : title;
        BuildAction = buildAction ?? (_ => { });
    }

    internal void Build(GoodSamaritanModMenuBuilder builder)
    {
        BuildAction(builder);
    }
}

public static class GoodSamaritanModMenuApi
{
    private static readonly List<GoodSamaritanModMenuPage> RegisteredPages = new();

    public static IReadOnlyList<GoodSamaritanModMenuPage> Pages => RegisteredPages;
    internal static int Revision { get; private set; }

    public static void RegisterPage(string id, string title, Action<GoodSamaritanModMenuBuilder> buildAction)
    {
        if (buildAction == null)
        {
            return;
        }

        for (int i = 0; i < RegisteredPages.Count; i++)
        {
            if (string.Equals(RegisteredPages[i].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                RegisteredPages[i] = new GoodSamaritanModMenuPage(id, title, buildAction);
                Revision++;
                return;
            }
        }

        RegisteredPages.Add(new GoodSamaritanModMenuPage(id, title, buildAction));
        Revision++;
    }

    public static void UnregisterPage(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        for (int i = RegisteredPages.Count - 1; i >= 0; i--)
        {
            if (!string.Equals(RegisteredPages[i].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            RegisteredPages.RemoveAt(i);
            Revision++;
            return;
        }
    }
}

public sealed class GoodSamaritanModMenuBuilder
{
    private readonly GoodSamaritanModMenuStyle style;
    private readonly Action requestRebuild;

    public RectTransform Root { get; }

    internal GoodSamaritanModMenuBuilder(RectTransform root, GoodSamaritanModMenuStyle style, Action requestRebuild)
    {
        Root = root;
        this.style = style;
        this.requestRebuild = requestRebuild;
    }

    public TMP_Text AddSection(string title)
    {
        var text = CreateText("Section", Root, title, 24, style.TextColor, FontStyles.Bold);
        AddLayout(text.gameObject, -1f, 34f, 0f, 0f);
        return text;
    }

    public TMP_Text AddText(string textValue, int fontSize = 18)
    {
        var text = CreateText("Text", Root, textValue, fontSize, style.MutedTextColor, FontStyles.Normal);
        text.enableWordWrapping = true;
        AddLayout(text.gameObject, -1f, -1f, 1f, 0f);
        return text;
    }

    public Button AddButton(string label, Action onClick)
    {
        var buttonGo = CreateUiObject("Button", Root);
        var image = buttonGo.AddComponent<Image>();
        image.color = style.ButtonColor;
        image.sprite = style.ButtonSprite;
        image.type = style.ButtonSprite == null ? Image.Type.Simple : Image.Type.Sliced;

        var button = buttonGo.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        button.colors = colors;
        if (onClick != null)
        {
            button.onClick.AddListener((UnityAction)(() => onClick()));
        }

        var labelText = CreateText("Label", buttonGo.transform, label, 18, style.TextColor, FontStyles.Bold);
        Stretch(labelText.rectTransform);
        labelText.alignment = TextAlignmentOptions.Center;

        AddLayout(buttonGo, -1f, 42f, 1f, 0f);
        return button;
    }

    public void AddButtonRow(params GoodSamaritanModMenuButton[] buttons)
    {
        var row = CreateRow("ButtonRow", 44f);
        for (int i = 0; i < buttons.Length; i++)
        {
            var buttonInfo = buttons[i];
            var button = CreateButton(row.transform, buttonInfo.Label, () => buttonInfo.OnClick?.Invoke());
            AddLayout(((Component)button).gameObject, 130f, 38f, 1f, 0f);
        }
    }

    public Toggle AddToggle(string label, bool value, Action<bool> onChanged)
    {
        var row = CreateRow("ToggleRow", 38f);
        var labelText = CreateText("Label", row.transform, label, 17, style.TextColor, FontStyles.Normal);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        AddLayout(labelText.gameObject, -1f, -1f, 1f, 0f);

        var toggleGo = CreateUiObject("Toggle", row.transform);
        AddLayout(toggleGo, 86f, 30f, 0f, 0f);

        var toggle = toggleGo.AddComponent<Toggle>();
        var background = CreateUiObject("Background", toggleGo.transform);
        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = style.ControlColor;
        Stretch(background.GetComponent<RectTransform>());

        var check = CreateUiObject("Checkmark", background.transform);
        var checkImage = check.AddComponent<Image>();
        checkImage.color = style.AccentColor;
        var checkRect = check.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.08f, 0.18f);
        checkRect.anchorMax = new Vector2(0.92f, 0.82f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        var stateText = CreateText("State", toggleGo.transform, value ? "ON" : "OFF", 16, style.TextColor, FontStyles.Bold);
        Stretch(stateText.rectTransform);
        stateText.alignment = TextAlignmentOptions.Center;

        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkImage;
        toggle.isOn = value;
        toggle.onValueChanged.AddListener((UnityAction<bool>)(next =>
        {
            stateText.text = next ? "ON" : "OFF";
            onChanged?.Invoke(next);
        }));

        return toggle;
    }

    public Slider AddFloatSlider(string label, float value, float min, float max, Action<float> onChanged)
    {
        var row = CreateRow("FloatSliderRow", 42f);
        var labelText = CreateText("Label", row.transform, label, 17, style.TextColor, FontStyles.Normal);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        AddLayout(labelText.gameObject, 250f, -1f, 0f, 0f);

        var valueText = CreateText("Value", row.transform, value.ToString("0.##"), 16, style.MutedTextColor, FontStyles.Normal);
        valueText.alignment = TextAlignmentOptions.MidlineRight;
        AddLayout(valueText.gameObject, 72f, -1f, 0f, 0f);

        var slider = CreateSlider(row.transform, min, max, value);
        slider.onValueChanged.AddListener((UnityAction<float>)(next =>
        {
            next = Mathf.Round(next * 100f) / 100f;
            valueText.text = next.ToString("0.##");
            onChanged?.Invoke(next);
        }));
        return slider;
    }

    public Slider AddIntSlider(string label, int value, int min, int max, Action<int> onChanged)
    {
        var row = CreateRow("IntSliderRow", 42f);
        var labelText = CreateText("Label", row.transform, label, 17, style.TextColor, FontStyles.Normal);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        AddLayout(labelText.gameObject, 250f, -1f, 0f, 0f);

        var valueText = CreateText("Value", row.transform, value.ToString(), 16, style.MutedTextColor, FontStyles.Normal);
        valueText.alignment = TextAlignmentOptions.MidlineRight;
        AddLayout(valueText.gameObject, 72f, -1f, 0f, 0f);

        var slider = CreateSlider(row.transform, min, max, value);
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener((UnityAction<float>)(next =>
        {
            int rounded = Mathf.RoundToInt(next);
            valueText.text = rounded.ToString();
            onChanged?.Invoke(rounded);
        }));
        return slider;
    }

    public void RequestRebuild()
    {
        requestRebuild?.Invoke();
    }

    internal GameObject CreateRow(string name, float height)
    {
        var row = CreateUiObject(name, Root);
        var image = row.AddComponent<Image>();
        image.color = style.RowColor;

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 4, 4);

        AddLayout(row, -1f, height, 1f, 0f);
        return row;
    }

    internal Button CreateButton(Transform parent, string label, Action onClick)
    {
        var go = CreateUiObject("Button", parent);
        var image = go.AddComponent<Image>();
        image.color = style.ButtonColor;
        image.sprite = style.ButtonSprite;
        image.type = style.ButtonSprite == null ? Image.Type.Simple : Image.Type.Sliced;

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        if (onClick != null)
        {
            button.onClick.AddListener((UnityAction)(() => onClick()));
        }

        var labelText = CreateText("Label", go.transform, label, 17, style.TextColor, FontStyles.Bold);
        Stretch(labelText.rectTransform);
        labelText.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private Slider CreateSlider(Transform parent, float min, float max, float value)
    {
        var sliderGo = CreateUiObject("Slider", parent);
        AddLayout(sliderGo, 260f, 30f, 1f, 0f);

        var slider = sliderGo.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = Mathf.Clamp(value, min, max);

        var background = CreateUiObject("Background", sliderGo.transform);
        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = style.ControlColor;
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.36f);
        backgroundRect.anchorMax = new Vector2(1f, 0.64f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        var fillArea = CreateUiObject("Fill Area", sliderGo.transform);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.36f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.64f);
        fillAreaRect.offsetMin = new Vector2(8f, 0f);
        fillAreaRect.offsetMax = new Vector2(-8f, 0f);

        var fill = CreateUiObject("Fill", fillArea.transform);
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = style.AccentColor;
        Stretch(fill.GetComponent<RectTransform>());

        var handleArea = CreateUiObject("Handle Slide Area", sliderGo.transform);
        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        Stretch(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(8f, 0f);
        handleAreaRect.offsetMax = new Vector2(-8f, 0f);

        var handle = CreateUiObject("Handle", handleArea.transform);
        var handleImage = handle.AddComponent<Image>();
        handleImage.color = style.TextColor;
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18f, 26f);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private TMP_Text CreateText(string name, Transform parent, string text, int size, Color color, FontStyles styleFlags)
    {
        var go = CreateUiObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = styleFlags;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        if (style.Font != null)
        {
            tmp.font = style.Font;
        }

        return tmp;
    }

    internal static GameObject CreateUiObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return go;
    }

    internal static void AddLayout(GameObject go, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight)
    {
        var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (preferredWidth >= 0f)
        {
            layout.preferredWidth = preferredWidth;
        }

        if (preferredHeight >= 0f)
        {
            layout.preferredHeight = preferredHeight;
        }

        layout.flexibleWidth = flexibleWidth;
        layout.flexibleHeight = flexibleHeight;
    }

    internal static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

public readonly struct GoodSamaritanModMenuButton
{
    public string Label { get; }
    public Action OnClick { get; }

    public GoodSamaritanModMenuButton(string label, Action onClick)
    {
        Label = label;
        OnClick = onClick;
    }
}

internal sealed class GoodSamaritanModMenuStyle
{
    internal TMP_FontAsset Font;
    internal Sprite ButtonSprite;
    internal Color TextColor = new(0.98f, 0.98f, 0.88f, 1f);
    internal Color MutedTextColor = new(0.82f, 0.84f, 0.78f, 1f);
    internal Color PanelColor = new(0.03f, 0.035f, 0.04f, 0.86f);
    internal Color RowColor = new(0.08f, 0.085f, 0.09f, 0.78f);
    internal Color ButtonColor = new(0.10f, 0.105f, 0.11f, 0.95f);
    internal Color ControlColor = new(0.18f, 0.19f, 0.20f, 1f);
    internal Color AccentColor = new(0.95f, 0.78f, 0.22f, 1f);

    internal static GoodSamaritanModMenuStyle FromSettings(SettingsUi settings)
    {
        var style = new GoodSamaritanModMenuStyle();
        if (settings != null)
        {
            var sourceButton = settings.generalTabButton ?? settings.voiceChatTabButton ?? settings.playersTabButton ?? settings.bansTabButton;
            if (sourceButton != null)
            {
                var image = ((Component)sourceButton).GetComponent<Image>();
                if (image != null)
                {
                    style.ButtonSprite = image.sprite;
                    style.ButtonColor = image.color;
                }
            }

            var tmp = ((Component)settings).GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                style.Font = tmp.font;
                style.TextColor = tmp.color;
            }
        }

        return style;
    }
}

public sealed class GoodSamaritanMenuController : MonoBehaviour
{
    internal static GoodSamaritanMenuController Active { get; private set; }

    private SettingsUi settingsUi;
    private GameObject modsTabButtonObject;
    private GameObject modsTabRoot;
    private RectTransform pageNavRoot;
    private RectTransform pageContentRoot;
    private GoodSamaritanModMenuStyle style;
    private float nextSettingsSearchTime;
    private bool registeredBuiltinPage;
    private int observedRevision = -1;
    private string selectedPageId;

    public GoodSamaritanMenuController(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        Active = this;
        RegisterBuiltinPage();
    }

    public void OnDestroy()
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    public void Update()
    {
        RegisterBuiltinPage();

        if (Input.GetKeyDown(KeyCode.F8))
        {
            OpenModsSettings();
        }

        if (Time.unscaledTime < nextSettingsSearchTime)
        {
            return;
        }

        nextSettingsSearchTime = Time.unscaledTime + 0.75f;
        TryInstallSettingsTab();

        if (modsTabRoot != null && modsTabRoot.activeSelf && observedRevision != GoodSamaritanModMenuApi.Revision)
        {
            RebuildModsPage(false);
        }
    }

    internal void HideModsTab()
    {
        if (modsTabRoot != null)
        {
            modsTabRoot.SetActive(false);
        }
    }

    internal void OpenModsSettings()
    {
        TryInstallSettingsTab();
        if (settingsUi == null || modsTabRoot == null)
        {
            return;
        }

        settingsUi.IsOpen = true;
        ShowModsTab();
    }

    private void RegisterBuiltinPage()
    {
        if (registeredBuiltinPage)
        {
            return;
        }

        GoodSamaritanModMenuApi.RegisterPage("good_samaritan_npc", "Good Samaritan NPC", GoodSamaritanConfigGui.Build);
        registeredBuiltinPage = true;
    }

    private void TryInstallSettingsTab()
    {
        var found = SettingsUi.Instance;
        if (found == null)
        {
            found = Object.FindObjectOfType<SettingsUi>();
        }

        if (found == null)
        {
            return;
        }

        if (settingsUi == found && modsTabRoot != null && modsTabButtonObject != null)
        {
            return;
        }

        settingsUi = found;
        style = GoodSamaritanModMenuStyle.FromSettings(settingsUi);
        InstallSettingsTab();
    }

    private void InstallSettingsTab()
    {
        if (settingsUi == null || settingsUi.generalTab == null)
        {
            return;
        }

        InstallModsTabButton();
        InstallModsTabContent();
        RebuildModsPage(true);
    }

    private void InstallModsTabButton()
    {
        if (modsTabButtonObject != null)
        {
            Object.Destroy(modsTabButtonObject);
            modsTabButtonObject = null;
        }

        var source = settingsUi.bansTabButton ?? settingsUi.playersTabButton ?? settingsUi.voiceChatTabButton ?? settingsUi.generalTabButton;
        if (source == null || ((Component)source).transform.parent == null)
        {
            return;
        }

        modsTabButtonObject = Object.Instantiate(((Component)source).gameObject, ((Component)source).transform.parent);
        modsTabButtonObject.name = "GoodSamaritanModsTabButton";
        modsTabButtonObject.transform.SetSiblingIndex(((Component)source).transform.GetSiblingIndex() + 1);
        SetAllText(modsTabButtonObject, "Mods");

        var button = modsTabButtonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((UnityAction)ShowModsTab);
        }
    }

    private void InstallModsTabContent()
    {
        if (modsTabRoot != null)
        {
            Object.Destroy(modsTabRoot);
            modsTabRoot = null;
        }

        var parent = settingsUi.generalTab.transform.parent;
        if (parent == null)
        {
            return;
        }

        modsTabRoot = GoodSamaritanModMenuBuilder.CreateUiObject("GoodSamaritanModsTab", parent);
        var rootRect = modsTabRoot.GetComponent<RectTransform>();
        CopyRect(settingsUi.generalTab.GetComponent<RectTransform>(), rootRect);
        modsTabRoot.SetActive(false);

        var background = modsTabRoot.AddComponent<Image>();
        background.color = style.PanelColor;

        var layout = modsTabRoot.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        var nav = GoodSamaritanModMenuBuilder.CreateUiObject("ModPageNav", modsTabRoot.transform);
        pageNavRoot = nav.GetComponent<RectTransform>();
        var navImage = nav.AddComponent<Image>();
        navImage.color = new Color(0.025f, 0.028f, 0.032f, 0.92f);
        var navLayout = nav.AddComponent<VerticalLayoutGroup>();
        navLayout.padding = new RectOffset(8, 8, 8, 8);
        navLayout.spacing = 8f;
        navLayout.childControlWidth = true;
        navLayout.childControlHeight = true;
        navLayout.childForceExpandWidth = true;
        navLayout.childForceExpandHeight = false;
        GoodSamaritanModMenuBuilder.AddLayout(nav, 220f, -1f, 0f, 1f);

        var scrollGo = GoodSamaritanModMenuBuilder.CreateUiObject("ModPageScroll", modsTabRoot.transform);
        var scrollRect = scrollGo.GetComponent<RectTransform>();
        var scrollImage = scrollGo.AddComponent<Image>();
        scrollImage.color = new Color(0.035f, 0.038f, 0.042f, 0.72f);
        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        GoodSamaritanModMenuBuilder.AddLayout(scrollGo, -1f, -1f, 1f, 1f);

        var viewport = GoodSamaritanModMenuBuilder.CreateUiObject("Viewport", scrollGo.transform);
        var viewportRect = viewport.GetComponent<RectTransform>();
        GoodSamaritanModMenuBuilder.Stretch(viewportRect);
        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        var content = GoodSamaritanModMenuBuilder.CreateUiObject("Content", viewport.transform);
        pageContentRoot = content.GetComponent<RectTransform>();
        pageContentRoot.anchorMin = new Vector2(0f, 1f);
        pageContentRoot.anchorMax = new Vector2(1f, 1f);
        pageContentRoot.pivot = new Vector2(0.5f, 1f);
        pageContentRoot.offsetMin = Vector2.zero;
        pageContentRoot.offsetMax = Vector2.zero;

        var contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(16, 16, 14, 14);
        contentLayout.spacing = 9f;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = pageContentRoot;
    }

    private void ShowModsTab()
    {
        TryInstallSettingsTab();
        if (settingsUi == null || modsTabRoot == null)
        {
            return;
        }

        SetOriginalTabsActive(false);
        modsTabRoot.SetActive(true);
        RebuildModsPage(false);
        settingsUi.PlayClickSfx();
    }

    private void SetOriginalTabsActive(bool active)
    {
        SetActive(settingsUi.generalTab, active);
        SetActive(settingsUi.voiceChatTab, active);
        SetActive(settingsUi.playersTab, active);
        SetActive(settingsUi.bansTab, active);
    }

    private void RebuildModsPage(bool force)
    {
        if (pageNavRoot == null || pageContentRoot == null)
        {
            return;
        }

        var pages = GoodSamaritanModMenuApi.Pages;
        if (pages.Count == 0)
        {
            selectedPageId = null;
        }
        else if (string.IsNullOrWhiteSpace(selectedPageId) || !ContainsPage(selectedPageId))
        {
            selectedPageId = pages[0].Id;
        }

        if (!force && observedRevision == GoodSamaritanModMenuApi.Revision)
        {
            RebuildActivePage();
            return;
        }

        observedRevision = GoodSamaritanModMenuApi.Revision;
        ClearChildren(pageNavRoot);

        for (int i = 0; i < pages.Count; i++)
        {
            var page = pages[i];
            var button = CreateNavButton(page);
            GoodSamaritanModMenuBuilder.AddLayout(((Component)button).gameObject, -1f, 42f, 1f, 0f);
        }

        RebuildActivePage();
    }

    [HideFromIl2Cpp]
    private Button CreateNavButton(GoodSamaritanModMenuPage page)
    {
        var builder = new GoodSamaritanModMenuBuilder(pageNavRoot, style, () => RebuildModsPage(true));
        var button = builder.CreateButton(pageNavRoot, page.Title, () =>
        {
            selectedPageId = page.Id;
            RebuildModsPage(true);
        });

        if (string.Equals(page.Id, selectedPageId, StringComparison.OrdinalIgnoreCase))
        {
            var image = ((Component)button).GetComponent<Image>();
            if (image != null)
            {
                image.color = style.AccentColor;
            }
        }

        return button;
    }

    private void RebuildActivePage()
    {
        if (pageContentRoot == null)
        {
            return;
        }

        ClearChildren(pageContentRoot);

        var page = FindPage(selectedPageId);
        if (page == null)
        {
            var builder = new GoodSamaritanModMenuBuilder(pageContentRoot, style, RebuildActivePage);
            builder.AddText("No mod config pages registered.");
            return;
        }

        var pageBuilder = new GoodSamaritanModMenuBuilder(pageContentRoot, style, RebuildActivePage);
        page.Build(pageBuilder);
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageContentRoot);
    }

    [HideFromIl2Cpp]
    private GoodSamaritanModMenuPage FindPage(string id)
    {
        var pages = GoodSamaritanModMenuApi.Pages;
        for (int i = 0; i < pages.Count; i++)
        {
            if (string.Equals(pages[i].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                return pages[i];
            }
        }

        return null;
    }

    private bool ContainsPage(string id)
    {
        return FindPage(id) != null;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.gameObject.SetActive(false);
            Object.Destroy(child.gameObject);
        }
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    private static void SetAllText(GameObject go, string label)
    {
        var tmps = go.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < tmps.Length; i++)
        {
            tmps[i].text = label;
        }

        var texts = go.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = label;
        }
    }

    private static void CopyRect(RectTransform source, RectTransform target)
    {
        if (source == null || target == null)
        {
            GoodSamaritanModMenuBuilder.Stretch(target);
            return;
        }

        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
        target.localScale = source.localScale;
    }
}

internal static class GoodSamaritanConfigGui
{
    internal static void Build(GoodSamaritanModMenuBuilder builder)
    {
        var s = GoodSamaritanPlugin.Settings;

        builder.AddSection("NPC Witness Behavior");
        builder.AddButtonRow(
            new GoodSamaritanModMenuButton("Easy", () => ApplyPreset(builder, "Easy")),
            new GoodSamaritanModMenuButton("Normal", () => ApplyPreset(builder, "Normal")),
            new GoodSamaritanModMenuButton("Hard", () => ApplyPreset(builder, "Hard")));
        builder.AddText($"Current preset: {s.NpcSuspicionPreset.Value}", 17);

        AddToggle(builder, s.Enabled, "Enabled");
        AddToggle(builder, s.ConvertExistingNpcs, "Convert existing NPCs");
        AddFloat(builder, s.ExistingNpcChance, "Existing NPC chance", 0f, 1f);
        AddInt(builder, s.ExtraSpawnCount, "Extra spawn count", 0, 20);
        AddFloat(builder, s.ScanIntervalSeconds, "Scan interval seconds", 0.1f, 5f);
        AddFloat(builder, s.WitnessRadius, "Witness radius", 3f, 40f);
        AddFloat(builder, s.WitnessFovDegrees, "Witness FOV degrees", 20f, 180f);
        AddFloat(builder, s.ReportCooldownSeconds, "Report cooldown seconds", 1f, 45f);
        AddFloat(builder, s.TargetCooldownSeconds, "Target cooldown seconds", 1f, 60f);
        AddFloat(builder, s.HighlightSeconds, "Highlight seconds", 0.5f, 12f);

        builder.AddSection("Detection Rules");
        AddCustomToggle(builder, s.EnableDirectTargetReports, "Direct target reports");
        AddCustomToggle(builder, s.DetectRevealingActions, "Reveal actions");
        AddCustomToggle(builder, s.DetectCarriedContraband, "Carried contraband");
        AddCustomToggle(builder, s.DetectHiddenContraband, "Hidden contraband");
        AddCustomToggle(builder, s.DetectContrabandPickup, "Contraband pickup");
        AddCustomToggle(builder, s.DetectCivilianAttacks, "Civilian attacks");
        AddCustomToggle(builder, s.DetectJumping, "Jumping");
        AddCustomToggle(builder, s.DetectLineCutting, "Likely line cutting");

        builder.AddSection("Playable Witnesses");
        AddToggle(builder, s.EnablePlayableWitnessPlayers, "Enable playable witness players");
        AddInt(builder, s.MaxPlayableWitnessPlayers, "Max playable witnesses", 0, 8);
        AddFloat(builder, s.PlayableWitnessChance, "Playable witness chance", 0f, 1f);

        builder.AddSection("Client Feedback");
        AddToggle(builder, s.EnableCustomClientMarker, "Custom marker");
        AddToggle(builder, s.EnableVoiceLine, "Voice line");
        builder.AddText($"Language: {s.Language.Value}", 17);
        builder.AddButtonRow(
            new GoodSamaritanModMenuButton("Auto", () => SetLanguage(builder, "Auto")),
            new GoodSamaritanModMenuButton("zh-Hans", () => SetLanguage(builder, "zh-Hans")),
            new GoodSamaritanModMenuButton("en", () => SetLanguage(builder, "en")),
            new GoodSamaritanModMenuButton("ja", () => SetLanguage(builder, "ja")));
    }

    private static void ApplyPreset(GoodSamaritanModMenuBuilder builder, string preset)
    {
        GoodSamaritanPlugin.Settings.ApplyPreset(preset);
        GoodSamaritanPlugin.Settings.Save();
        builder.RequestRebuild();
    }

    private static void SetLanguage(GoodSamaritanModMenuBuilder builder, string language)
    {
        GoodSamaritanPlugin.Settings.Language.Value = language;
        GoodSamaritanPlugin.Settings.Save();
        builder.RequestRebuild();
    }

    private static void AddToggle(GoodSamaritanModMenuBuilder builder, ConfigEntry<bool> entry, string label)
    {
        builder.AddToggle(label, entry.Value, next =>
        {
            if (entry.Value == next)
            {
                return;
            }

            entry.Value = next;
            GoodSamaritanPlugin.Settings.Save();
        });
    }

    private static void AddCustomToggle(GoodSamaritanModMenuBuilder builder, ConfigEntry<bool> entry, string label)
    {
        builder.AddToggle(label, entry.Value, next =>
        {
            if (entry.Value == next)
            {
                return;
            }

            entry.Value = next;
            GoodSamaritanPlugin.Settings.NpcSuspicionPreset.Value = "Custom";
            GoodSamaritanPlugin.Settings.Save();
        });
    }

    private static void AddFloat(GoodSamaritanModMenuBuilder builder, ConfigEntry<float> entry, string label, float min, float max)
    {
        builder.AddFloatSlider(label, entry.Value, min, max, next =>
        {
            if (Mathf.Approximately(entry.Value, next))
            {
                return;
            }

            entry.Value = next;
            GoodSamaritanPlugin.Settings.Save();
        });
    }

    private static void AddInt(GoodSamaritanModMenuBuilder builder, ConfigEntry<int> entry, string label, int min, int max)
    {
        builder.AddIntSlider(label, entry.Value, min, max, next =>
        {
            if (entry.Value == next)
            {
                return;
            }

            entry.Value = next;
            GoodSamaritanPlugin.Settings.Save();
        });
    }
}

[HarmonyPatch(typeof(SettingsUi), nameof(SettingsUi.ShowGeneralTab))]
internal static class GoodSamaritanSettingsGeneralTabPatch
{
    private static void Postfix()
    {
        GoodSamaritanMenuController.Active?.HideModsTab();
    }
}

[HarmonyPatch(typeof(SettingsUi), nameof(SettingsUi.ShowVoiceChatTab))]
internal static class GoodSamaritanSettingsVoiceChatTabPatch
{
    private static void Postfix()
    {
        GoodSamaritanMenuController.Active?.HideModsTab();
    }
}

[HarmonyPatch(typeof(SettingsUi), nameof(SettingsUi.ShowPlayersTab))]
internal static class GoodSamaritanSettingsPlayersTabPatch
{
    private static void Postfix()
    {
        GoodSamaritanMenuController.Active?.HideModsTab();
    }
}

[HarmonyPatch(typeof(SettingsUi), nameof(SettingsUi.ShowBansTab))]
internal static class GoodSamaritanSettingsBansTabPatch
{
    private static void Postfix()
    {
        GoodSamaritanMenuController.Active?.HideModsTab();
    }
}
