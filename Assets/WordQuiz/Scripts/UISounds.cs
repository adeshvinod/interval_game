using UnityEngine;

/// <summary>
/// Interface sounds, kept in one asset so they can be swapped without touching
/// code. Lives in a Resources folder because <see cref="UIClickSound"/> is
/// bootstrapped at runtime and has no scene to be wired from.
/// </summary>
[CreateAssetMenu(fileName = "UISounds", menuName = "Settings/UI Sounds")]
public class UISounds : ScriptableObject
{
    [Tooltip("Played when any menu / navigation button is pressed.")]
    public AudioClip click;

    [Range(0f, 1f)] public float clickVolume = 0.6f;
}
