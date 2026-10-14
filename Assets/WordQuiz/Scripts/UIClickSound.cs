using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gives every menu / navigation button a click sound, without wiring anything
/// per scene: it bootstraps itself, survives scene loads, and attaches a
/// listener to each Button whenever a scene finishes loading.
///
/// Gameplay controls are deliberately skipped - the fretboard buttons and the
/// chord options already play musical audio, and layering a UI click over a
/// guitar note sounds wrong.
/// </summary>
public class UIClickSound : MonoBehaviour
{
    static UIClickSound instance;

    /// Components whose buttons make their own sound already.
    static readonly string[] SkipComponents =
    {
        "prog_button",      // fretboard - plays the note
        "chordsOption",     // chord learn mode - plays the chord
        "interval_option",  // interval options - play the interval
        "option_note",
    };

    /// Handlers that produce musical audio of their own, matched on the
    /// onClick method name so this does not depend on GameObject naming.
    /// Root shifting sounds the new root note / chord, so a UI click on top
    /// would double up.
    static readonly string[] SkipMethods =
    {
        "shiftroot",   // intervals learn mode
        "ShiftRoot",   // chords learn mode
    };

    UISounds sounds;
    AudioSource source;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance != null) return;
        var go = new GameObject("[UIClickSound]");
        instance = go.AddComponent<UIClickSound>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        sounds = Resources.Load<UISounds>("UISounds");
        if (sounds == null || sounds.click == null)
        {
            Debug.LogWarning("UIClickSound: no UISounds asset (or no clip) in a Resources folder; menu clicks will be silent.");
            return;
        }

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = sounds.click;
        source.volume = sounds.clickVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
        Hook();   // the scene we booted into
    }

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Hook();

    /// Attach to every Button in the active scene, including ones sitting in
    /// panels that start inactive (gameover cards, the settings sheet).
    void Hook()
    {
        if (source == null) return;

        int hooked = 0, skipped = 0;
        foreach (var btn in Resources.FindObjectsOfTypeAll<Button>())
        {
            // FindObjectsOfTypeAll also returns prefabs and assets.
            if (!btn.gameObject.scene.IsValid()) continue;

            if (ShouldSkip(btn)) { skipped++; continue; }

            btn.onClick.RemoveListener(Play);   // idempotent across reloads
            btn.onClick.AddListener(Play);
            hooked++;
        }
#if UNITY_EDITOR
        Debug.Log($"UIClickSound: {hooked} buttons hooked, {skipped} skipped (they play their own audio).");
#endif
    }

    static bool ShouldSkip(Button btn)
    {
        // Check the button and its parents: the fretboard buttons carry
        // prog_button on the same object, chord options on a parent.
        foreach (var mb in btn.GetComponentsInParent<MonoBehaviour>(true))
            if (mb != null && SkipComponents.Contains(mb.GetType().Name)) return true;
        foreach (var mb in btn.GetComponentsInChildren<MonoBehaviour>(true))
            if (mb != null && SkipComponents.Contains(mb.GetType().Name)) return true;

        for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
            if (SkipMethods.Contains(btn.onClick.GetPersistentMethodName(i))) return true;

        return false;
    }

    void Play()
    {
        if (source == null || source.clip == null) return;
        // PlayOneShot so rapid taps overlap instead of cutting each other off.
        source.PlayOneShot(source.clip, sounds.clickVolume);
    }
}
