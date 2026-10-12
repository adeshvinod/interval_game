using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Runtime-bootstrapped toast that subscribes to EngagementManager events and
/// displays slide-down notifications.
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

    // ── UI construction ──────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        panel = panelGO.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot     = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(900, 200);
        panel.anchoredPosition = new Vector2(0, 220);

        var bg = panelGO.GetComponent<Image>();
        bg.color = new Color(10/255f, 29/255f, 39/255f, 0.95f);

        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(panel, false);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.55f);
        titleRT.anchorMax = new Vector2(1, 1f);
        titleRT.offsetMin = new Vector2(20, 0);
        titleRT.offsetMax = new Vector2(-20, -10);
        titleLabel = titleGO.GetComponent<TMP_Text>();
        titleLabel.text = "";
        titleLabel.fontSize = 56;
        titleLabel.fontStyle = FontStyles.Bold;
        titleLabel.alignment = TextAlignmentOptions.Center;
        titleLabel.color = new Color(254/255f, 226/255f, 176/255f);

        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
        bodyGO.transform.SetParent(panel, false);
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0, 0f);
        bodyRT.anchorMax = new Vector2(1, 0.55f);
        bodyRT.offsetMin = new Vector2(20, 10);
        bodyRT.offsetMax = new Vector2(-20, 0);
        bodyLabel = bodyGO.GetComponent<TMP_Text>();
        bodyLabel.text = "";
        bodyLabel.fontSize = 36;
        bodyLabel.alignment = TextAlignmentOptions.Center;
        bodyLabel.color = Color.white;
    }

    // ── Event handlers ───────────────────────────────────────────────────────
    void HandleAvatarEvolved(int oldStage, int newStage)
    {
        Submit("AVATAR EVOLVED!", $"You reached Stage {newStage}", new Color(249/255f, 117/255f, 75/255f));
    }


    void HandleLevelPassed(LearningMode mode, int level, int score)
    {
        Submit("CLEARED!", $"{mode} • Level {level} • {score} pts", new Color(181/255f, 207/255f, 174/255f));
    }

    void HandleSessionEnd()
    {
        // Flush deferred buffer to live queue, then start playback if idle.
        if (deferredBuffer.Count == 0) return;
        foreach (var item in deferredBuffer) liveQueue.Enqueue(item);
        deferredBuffer.Clear();
        if (!isShowing) StartCoroutine(ShowNext());
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
            titleLabel.color = tint;

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
