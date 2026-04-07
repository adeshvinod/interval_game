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

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) button = GetComponentInChildren<Button>();
        if (button != null) button.onClick.AddListener(OnClicked);

        // Set display name
        if (label != null && chordTypeIndex >= 0 && chordTypeIndex < ChordsGameData.ChordNames.Length)
            label.text = ChordsGameData.ChordNames[chordTypeIndex];

        SetVisualState(false);
    }

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }

    public void Setup(int index)
    {
        chordTypeIndex = index;
        if (label != null) label.text = ChordsGameData.ChordNames[index];
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
