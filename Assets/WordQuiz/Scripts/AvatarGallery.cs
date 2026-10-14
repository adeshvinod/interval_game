using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The panel that opens when the player taps their avatar on the main menu.
///
/// Shows all 14 avatars in a horizontally scrollable strip. Earned ones are in
/// full colour with their name; the rest are **pure black silhouettes with a
/// "?"**, so the shape teases what is coming without giving the art away.
///
/// Slots are built at runtime from EngagementManager.AvatarSprites, so adding a
/// character means editing that array only.
/// </summary>
public class AvatarGallery : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform content;      // ScrollRect content strip
    [SerializeField] private ScrollRect   scrollRect;
    [SerializeField] private TextMeshProUGUI caption;
    [SerializeField] private TextMeshProUGUI progressLabel;

    [Header("Built at runtime, one per avatar")]
    [SerializeField] private Image[] slotImages;
    [SerializeField] private TextMeshProUGUI[] slotNames;
    [SerializeField] private GameObject[] slotLocks;     // the "?" overlay

    // Pure black: Image tint multiplies, so black x art = solid silhouette
    // while the alpha channel still carries the shape.
    static readonly Color Silhouette = Color.black;
    static readonly Color Earned     = Color.white;
    static readonly Color NameEarned = new Color(0xF2 / 255f, 0x78 / 255f, 0x4F / 255f, 1f);
    static readonly Color NameLocked = new Color(1f, 1f, 1f, 0.40f);

    /// While open this is a modal, so the canvas it lives on is lifted above
    /// the menu's other canvases and dropped back on close.
    const int ModalSortingOrder = 200;
    Canvas owningCanvas;
    int previousSortingOrder;

    void Awake()
    {
        owningCanvas = GetComponentInParent<Canvas>();
        if (owningCanvas != null) previousSortingOrder = owningCanvas.sortingOrder;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Open()
    {
        Refresh();
        if (owningCanvas != null)
        {
            previousSortingOrder = owningCanvas.sortingOrder;
            owningCanvas.sortingOrder = ModalSortingOrder;
        }
        if (panelRoot != null) panelRoot.SetActive(true);
        ScrollToCurrent();
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (owningCanvas != null) owningCanvas.sortingOrder = previousSortingOrder;
    }

    public void Refresh()
    {
        int stage = EngagementManager.GetAvatarStage();

        for (int i = 0; i < slotImages.Length; i++)
        {
            bool earned = (i + 1) <= stage;

            if (slotImages[i] != null)
            {
                slotImages[i].color = earned ? Earned : Silhouette;
                slotImages[i].preserveAspect = true;
            }
            if (slotLocks != null && i < slotLocks.Length && slotLocks[i] != null)
                slotLocks[i].SetActive(!earned);

            if (slotNames != null && i < slotNames.Length && slotNames[i] != null)
            {
                // Names stay visible even when locked - knowing you are two
                // clears away from "Count Shredula" is the hook.
                slotNames[i].text  = EngagementManager.AvatarNameForStage(i + 1).ToUpper();
                slotNames[i].color = earned ? NameEarned : NameLocked;
            }
        }

        if (progressLabel != null)
            progressLabel.text = $"{stage} / {EngagementManager.AvatarCount} UNLOCKED";

        if (caption != null)
        {
            string requirement = EngagementManager.NextAvatarRequirement();
            caption.text = string.IsNullOrEmpty(requirement)
                ? "Every level cleared. You are " + EngagementManager.GetAvatarName() + "."
                : requirement;
        }
    }

    /// Park the strip on the newest unlocked avatar rather than at the start.
    void ScrollToCurrent()
    {
        if (scrollRect == null || slotImages == null || slotImages.Length < 2) return;
        int stage = EngagementManager.GetAvatarStage();
        float t = (stage - 1) / (float)(slotImages.Length - 1);
        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(t);
    }
}
