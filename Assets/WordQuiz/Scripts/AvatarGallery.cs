using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The panel that opens when the player taps their avatar on the main menu.
///
/// Shows all five avatars side by side. Earned ones are in full colour; the
/// rest are dark silhouettes, so the player can see what is coming without
/// being told what it looks like. A caption spells out the rule, which is the
/// part that was not obvious: the avatar only evolves when the SAME level is
/// cleared in all three modes.
/// </summary>
public class AvatarGallery : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    /// While open this is a modal, so the canvas it lives on is lifted above
    /// the menu's other canvases (help / settings sit at order 1) and dropped
    /// back on close. Doing it here beats a nested Canvas, which Unity kept
    /// creating as a WorldSpace root instead of an overriding child.
    const int ModalSortingOrder = 200;
    Canvas owningCanvas;
    int previousSortingOrder;

    [Header("One per stage, in order (5)")]
    [SerializeField] private Image[] slotImages;
    [SerializeField] private TextMeshProUGUI[] slotNames;

    [Header("Explanation line at the bottom")]
    [SerializeField] private TextMeshProUGUI caption;

    /// Locked avatars are flattened to this, so only the shape reads.
    static readonly Color Silhouette = new Color(0.10f, 0.20f, 0.23f, 1f);
    static readonly Color Earned     = Color.white;
    static readonly Color NameEarned = new Color(0xF2 / 255f, 0x78 / 255f, 0x4F / 255f, 1f);
    static readonly Color NameLocked = new Color(1f, 1f, 1f, 0.45f);

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
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (owningCanvas != null) owningCanvas.sortingOrder = previousSortingOrder;
    }

    public void Refresh()
    {
        int stage = EngagementManager.GetAvatarStage();   // 1..5

        if (slotImages != null)
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] == null) continue;
                bool earned = (i + 1) <= stage;
                slotImages[i].color = earned ? Earned : Silhouette;
                slotImages[i].preserveAspect = true;
            }

        if (slotNames != null)
            for (int i = 0; i < slotNames.Length; i++)
            {
                if (slotNames[i] == null) continue;
                bool earned = (i + 1) <= stage;
                // Don't spoil the names of avatars that are still locked.
                slotNames[i].text = earned ? EngagementManager.AvatarNameForStage(i + 1).ToUpper() : "? ? ?";
                slotNames[i].color = earned ? NameEarned : NameLocked;
            }

        if (caption != null)
        {
            int needed = EngagementManager.LevelNeededForNextAvatar();
            var missing = EngagementManager.ModesMissingForNextAvatar();

            if (needed == 0)
                caption.text = "Every level cleared. You are " +
                               EngagementManager.AvatarNameForStage(stage) + ".";
            else if (missing.Count == 0)
                caption.text = $"Clear Level {needed} in all three modes to evolve.";
            else
                caption.text =
                    $"Your avatar evolves when you clear Level {needed} in ALL THREE modes.\n" +
                    $"Still to clear: {string.Join(", ", missing).ToUpper()}";
        }
    }
}
