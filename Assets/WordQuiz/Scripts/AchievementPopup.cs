using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Runtime-bootstrapped toast that subscribes to EngagementManager events and
/// displays slide-down notifications (level cleared, next-avatar goal, avatar
/// evolved). There is no XP or currency to report.
///
/// Crucially: while EngagementManager.IsInPlaySession is true (between
/// StartSession and EndSession), incoming events are *deferred* into a buffer
/// so they don't distract the player mid-gameplay. On OnSessionEnd the buffer
/// is flushed and shown sequentially.
///
/// Auto-spawns BeforeSceneLoad, persists via DontDestroyOnLoad, builds its own
/// Canvas in code so no scene wiring is needed.
public class AchievementPopup : MonoBehaviour
{
    static AchievementPopup instance;

    Canvas        canvas;
    RectTransform panel;
    TMP_Text      titleLabel;
    TMP_Text      bodyLabel;

    // Active queue (toasts to be shown right now)
    readonly Queue<(string title, string body, Color tint)> liveQueue = new();
    // Deferred queue (events that fired during gameplay)
    readonly List<(string title, string body, Color tint)> deferredBuffer = new();
    bool isShowing;

    const float SLIDE_DURATION = 0.35f;
    const float HOLD_DURATION  = 2.5f;
    const float GAP_BETWEEN    = 0.2f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance != null) return;
        var go = new GameObject("[AchievementPopup]");
        instance = go.AddComponent<AchievementPopup>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        BuildUI();
        EngagementManager.OnAvatarEvolved       += HandleAvatarEvolved;
        EngagementManager.OnLevelPassed         += HandleLevelPassed;
        EngagementManager.OnSessionEnd          += HandleSessionEnd;
    }

    void OnDestroy()
    {
        EngagementManager.OnAvatarEvolved       -= HandleAvatarEvolved;
        EngagementManager.OnLevelPassed         -= HandleLevelPassed;
        EngagementManager.OnSessionEnd          -= HandleSessionEnd;
    }

    // ── Game palette ─────────────────────────────────────────────────────────
    // Sampled from the main menu so the toast belongs to the same game:
    // dark teal icon buttons, coral action buttons, off-white page.
    static readonly Color Teal  = new Color(0x1B/255f, 0x4D/255f, 0x55/255f, 1f);
    static readonly Color Coral = new Color(0xF2/255f, 0x78/255f, 0x4F/255f, 1f);
    static readonly Color Cream = new Color(0xF4/255f, 0xF1/255f, 0xEC/255f, 1f);

    static TMP_FontAsset gameFont;
    static Sprite roundedSprite;

    static void LoadAssets()
    {
        if (gameFont == null)
        {
            // MPLUS1Code is what the main-menu buttons use, so the toast reads
            // as part of the same app. Keys verified against the TMP Resources
            // folder - a wrong key silently falls back to LiberationSans.
            gameFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/MPLUS1Code-VariableFont_wght")
                    ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/MPLUS2-Regular")
                    ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/vt323/VT323-Regular");
            if (gameFont == null) Debug.LogWarning("AchievementPopup: no game font found, using TMP default.");
        }
        if (roundedSprite == null)
        {
            // 9-sliced rounded rect already used elsewhere in the game.
            var all = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var sp in all)
                if (sp != null && sp.name == "SquareCurveCorner") { roundedSprite = sp; break; }
        }
    }

    // ── UI construction ──────────────────────────────────────────────────────
    void BuildUI()
    {
        LoadAssets();

        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // The game plays in landscape, so match the landscape reference the
        // rest of the UI is authored against.
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        panel = panelGO.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot     = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(760, 150);
        panel.anchoredPosition = new Vector2(0, 40);

        var bg = panelGO.GetComponent<Image>();
        bg.color = Teal;
        if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

        // A coral rule under the title ties it to the menu buttons.
        var ruleGO = new GameObject("Rule", typeof(RectTransform), typeof(Image));
        ruleGO.transform.SetParent(panel, false);
        var ruleRT = ruleGO.GetComponent<RectTransform>();
        ruleRT.anchorMin = new Vector2(0.5f, 1f);
        ruleRT.anchorMax = new Vector2(0.5f, 1f);
        ruleRT.pivot     = new Vector2(0.5f, 1f);
        ruleRT.sizeDelta = new Vector2(120, 5);
        ruleRT.anchoredPosition = new Vector2(0, -74);
        var ruleImg = ruleGO.GetComponent<Image>();
        ruleImg.color = Coral;
        ruleImg.raycastTarget = false;

        titleLabel = MakeLabel("Title", panel, new Vector2(0, 0.52f), new Vector2(1, 1f),
                               new Vector2(28, 0), new Vector2(-28, -14), 46f, Coral);
        bodyLabel  = MakeLabel("Body",  panel, new Vector2(0, 0f),     new Vector2(1, 0.52f),
                               new Vector2(28, 16), new Vector2(-28, -6), 30f, Cream);
    }

    TMP_Text MakeLabel(string name, RectTransform parent, Vector2 aMin, Vector2 aMax,
                       Vector2 offMin, Vector2 offMax, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;

        var t = go.GetComponent<TMP_Text>();
        if (gameFont != null) t.font = gameFont;
        t.text = "";
        t.alignment = TextAlignmentOptions.Center;
        t.color = color;
        t.raycastTarget = false;
        // Long sentences must shrink rather than spill out of the panel.
        t.enableAutoSizing = true;
        t.fontSizeMax = size;
        t.fontSizeMin = size * 0.5f;
        return t;
    }

    // ── Event handlers ───────────────────────────────────────────────────────
    void HandleAvatarEvolved(int oldStage, int newStage)
    {
        // Only reached once the same level is cleared in all three modes.
        Submit("AVATAR EVOLVED!",
               $"You are now {EngagementManager.AvatarNameForStage(newStage)}",
               new Color(249/255f, 117/255f, 75/255f));
    }


    void HandleLevelPassed(LearningMode mode, int level, int score)
    {
        Submit($"{mode.ToString().ToUpper()} LEVEL {level} CLEARED!",
               $"{score} pts",
               new Color(181/255f, 207/255f, 174/255f));

        // Clearing one mode is not enough to evolve the avatar, so spell out
        // what is still missing. Nothing is shown once the avatar is maxed, or
        // when this clear completed the set (HandleAvatarEvolved covers that).
        string missing = EngagementManager.DescribeMissingModes();
        int needed = EngagementManager.LevelNeededForNextAvatar();
        if (!string.IsNullOrEmpty(missing) && needed > 0)
        {
            Submit("NEXT AVATAR",
                   $"Clear Level {needed} in {missing} to become " +
                   $"{EngagementManager.AvatarNameForStage(EngagementManager.GetAvatarStage() + 1)}",
                   new Color(115/255f, 193/255f, 206/255f));
        }
    }

    void HandleSessionEnd()
    {
        // Flush deferred buffer to live queue, then start playback if idle.
        if (deferredBuffer.Count == 0) return;
        foreach (var item in deferredBuffer) liveQueue.Enqueue(item);
        deferredBuffer.Clear();
        if (!isShowing) StartCoroutine(ShowNext());
    }

    /// Post a toast from anywhere (e.g. tapping the avatar on the main menu).
    /// Respects the in-session deferral like every other notification.
    public static void Notify(string title, string body)
    {
        if (instance != null) instance.Submit(title, body, new Color(115/255f, 193/255f, 206/255f));
    }

    // ── Submission routing ───────────────────────────────────────────────────
    void Submit(string title, string body, Color tint)
    {
        if (EngagementManager.IsInPlaySession)
        {
            // Defer until session ends — don't distract the player.
            deferredBuffer.Add((title, body, tint));
        }
        else
        {
            liveQueue.Enqueue((title, body, tint));
            if (!isShowing) StartCoroutine(ShowNext());
        }
    }

    // ── Animation ────────────────────────────────────────────────────────────
    IEnumerator ShowNext()
    {
        isShowing = true;
        while (liveQueue.Count > 0)
        {
            var (title, body, tint) = liveQueue.Dequeue();
            titleLabel.text = title;
            bodyLabel.text  = body;

            yield return Slide(220, -20, SLIDE_DURATION);
            yield return new WaitForSecondsRealtime(HOLD_DURATION);
            yield return Slide(-20, 220, SLIDE_DURATION);
            yield return new WaitForSecondsRealtime(GAP_BETWEEN);
        }
        isShowing = false;
    }

    IEnumerator Slide(float fromY, float toY, float duration)
    {
        float t = 0f;
        var pos = panel.anchoredPosition;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = 1f - Mathf.Pow(1f - k, 3f);
            pos.y = Mathf.Lerp(fromY, toY, k);
            panel.anchoredPosition = pos;
            yield return null;
        }
        pos.y = toY;
        panel.anchoredPosition = pos;
    }
}
