using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attached to each chord-name option button in the learnModeChords scene.
/// Clicking it tells chordsLearnMode to display that chord on the fretboard.
/// </summary>
public class chordsOption : MonoBehaviour
{
    [HideInInspector] public int chordTypeIndex; // index into ChordsGameData.AllChords
    [HideInInspector] public bool isSelected;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI label;

    private static readonly Color SelectedColor   = new Color(0.2f, 0.8f, 0.4f, 1f);
    private static readonly Color DeselectedColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);

    // Long names ("Harmonic Minor", "Major Scale") used to wrap onto a second
    // line and spill outside the button. Force one line and let TMP shrink the
    // font to fit instead.
    private const float LabelFontSizeMin = 10f;
    private const float LabelFontSizeMax = 34f;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) button = GetComponentInChildren<Button>();
        if (button != null) button.onClick.AddListener(OnClicked);

        if (chordTypeIndex >= 0 && chordTypeIndex < ChordsGameData.ChordNames.Length)
            SetLabel(ChordsGameData.ChordNames[chordTypeIndex]);

        SetVisualState(false);
    }

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }

    public void Setup(int index)
    {
        chordTypeIndex = index;
        if (index >= 0 && index < ChordsGameData.ChordNames.Length)
            SetLabel(ChordsGameData.ChordNames[index]);
    }

    /// Writes the text and keeps it inside the button on one line.
    private void SetLabel(string text)
    {
        if (label == null) return;

        label.text = text;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode     = TextOverflowModes.Overflow;
        label.enableAutoSizing = true;
        label.fontSizeMin      = LabelFontSizeMin;
        label.fontSizeMax      = LabelFontSizeMax;
        label.alignment        = TextAlignmentOptions.Center;
    }

    private void OnClicked()
    {
        if (chordsLearnMode.instance != null)
            chordsLearnMode.instance.SelectChord(chordTypeIndex);
    }

    public void SetVisualState(bool selected)
    {
        isSelected = selected;
        if (backgroundImage != null)
            backgroundImage.color = selected ? SelectedColor : DeselectedColor;
    }
}
