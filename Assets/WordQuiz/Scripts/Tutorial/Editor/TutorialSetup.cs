using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// FretNoodle > Build Tutorial: (re)creates everything the tutorial needs.
/// - SDF font assets for M PLUS 1 Code, Lilita One and VT323 (the existing ones
///   are 512px bitmap atlases without — → ← · ’ “ ” × −, which blur at 148px),
///   with LiberationSans as the fallback for arrows
/// - the opaque crop of each guide character, so heights match the prototype
/// - Scenes/tutorial.unity, added to Build Settings after the main menu
/// - the main menu's "?" button, which now loads the tutorial
/// Safe to run again: it overwrites the scene and re-wires the button.
/// </summary>
public static class TutorialSetup
{
    const string Root = "Assets/WordQuiz/Tutorial";
    const string FontDir = Root + "/Fonts";
    const string SrcFonts = "Assets/WordQuiz/TextMesh Pro/Resources/Fonts & Materials/";
    const string ScenePath = "Assets/WordQuiz/Scenes/tutorial.unity";
    const string MenuPath = "Assets/WordQuiz/Scenes/notes-intervals-menu.unity";
    const string Characters = "Assets/WordQuiz/Art/game_assets/game_characters/";

    static readonly (string key, string file)[] Guides =
    {
        ("boy", "novice2 2.png"),           // Guitar Novice
        ("duchess", "disco_duchess.png"),
        ("mohawk", "mohawk_maven.png"),
        ("riff", "riff_rager.png"),
        ("bluesman", "midnight_bluesman.png"),
    };

    [MenuItem("FretNoodle/Build Tutorial")]
    public static void Build()
    {
        var mplus = Font("MPLUS1Code-VariableFont_wght.ttf", "MPLUS1Code Tutorial SDF");
        var lilita = Font("LilitaOne-Regular.ttf", "LilitaOne Tutorial SDF");
        var vt = Font("vt323/VT323-Regular.ttf", "VT323 Tutorial SDF");

        // M PLUS has no arrows (→ ←); the browser fell back to a system font.
        var arrows = Font("Assets/WordQuiz/TextMesh Pro/Fonts/LiberationSans.ttf", "LiberationSans Tutorial SDF");
        foreach (var fa in new[] { mplus, lilita, vt })
        {
            if (fa.fallbackFontAssetTable == null) fa.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            if (!fa.fallbackFontAssetTable.Contains(arrows)) fa.fallbackFontAssetTable.Add(arrows);
            EditorUtility.SetDirty(fa);
        }
        AssetDatabase.SaveAssets();

        BuildScene(mplus, lilita, vt);
        AddToBuildSettings();
        WireHelpButton();
        Debug.Log("Tutorial built: " + ScenePath);
    }

    static TMP_FontAsset Font(string ttf, string name)
    {
        string path = $"{FontDir}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory(FontDir);
        var font = AssetDatabase.LoadAssetAtPath<Font>(ttf.StartsWith("Assets/") ? ttf : SrcFonts + ttf);
        var fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                                               AtlasPopulationMode.Dynamic, true);
        fa.name = name;
        AssetDatabase.CreateAsset(fa, path);
        fa.atlasTextures[0].name = name + " Atlas";
        AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
        fa.material.name = name + " Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);

        // fake bold must not widen a monospace font (Chrome's real bold doesn't)
        var so = new SerializedObject(fa);
        var spacing = so.FindProperty("m_BoldSpacing") ?? so.FindProperty("boldSpacing");
        if (spacing != null) spacing.floatValue = 0;
        // don't ship glyphs the editor happened to rasterise
        var clear = so.FindProperty("m_ClearDynamicDataOnBuild");
        if (clear != null) clear.boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        return fa;
    }

    /// Opaque bounds (alpha > 10%) of a sprite's texture, in texture pixels.
    static Rect Crop(Sprite s)
    {
        var tex = s.texture;
        var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(tex, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var read = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        read.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        var px = read.GetPixels32();
        int minX = tex.width, minY = tex.height, maxX = -1, maxY = -1;
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                if (px[y * tex.width + x].a > 25)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
        Object.DestroyImmediate(read);
        return maxX < 0 ? s.rect : new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    static void BuildScene(TMP_FontAsset mplus, TMP_FontAsset lilita, TMP_FontAsset vt)
    {
        var active = SceneManager.GetActiveScene();
        if (active.isDirty) EditorSceneManager.SaveScene(active);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = TutorialTheme.ink;
        cam.gameObject.AddComponent<landscape>();

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasGo = new GameObject("Canvas-tutorial", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var letterbox = new GameObject("letterbox", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        letterbox.SetParent(canvasGo.transform, false);
        letterbox.anchorMin = Vector2.zero;
        letterbox.anchorMax = Vector2.one;
        letterbox.sizeDelta = Vector2.zero;
        letterbox.GetComponent<Image>().color = TutorialTheme.ink;

        // the 1920x1080 stage, centred and scaled to fit by the controller
        var stage = new GameObject("stage", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
        stage.SetParent(canvasGo.transform, false);
        stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(0.5f, 0.5f);
        stage.sizeDelta = new Vector2(1920, 1080);

        var ctrl = new GameObject("TutorialController").AddComponent<TutorialController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("screensJson").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/tutorial_screens.json");
        so.FindProperty("mplusFont").objectReferenceValue = mplus;
        so.FindProperty("lilitaFont").objectReferenceValue = lilita;
        so.FindProperty("vt323Font").objectReferenceValue = vt;
        so.FindProperty("stage").objectReferenceValue = stage;
        so.FindProperty("gameSettings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/WordQuiz/Resources/GameSettings.asset");
        so.FindProperty("notesGameData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<NotesGameData>("Assets/WordQuiz/Resources/NotesGameData.asset");
        var guides = so.FindProperty("guides");
        guides.arraySize = Guides.Length;
        for (int i = 0; i < Guides.Length; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Characters + Guides[i].file);
            var g = guides.GetArrayElementAtIndex(i);
            g.FindPropertyRelative("key").stringValue = Guides[i].key;
            g.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            g.FindPropertyRelative("crop").rectValue = sprite != null ? Crop(sprite) : default;
            if (sprite == null) Debug.LogError("Tutorial: missing character art " + Guides[i].file);
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.CloseScene(scene, true);
        if (active.IsValid()) SceneManager.SetActiveScene(active);
    }

    static void AddToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == ScenePath)) return;
        int menu = scenes.FindIndex(s => s.path == MenuPath);
        scenes.Insert(menu + 1, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    /// The "?" on the main menu had a dead listener (missing target,
    /// toggle_settingsPanel). Point it at ButtonBehavior.LoadScene("tutorial"),
    /// the same way the mode buttons load their scenes.
    static void WireHelpButton()
    {
        var scene = SceneManager.GetSceneByPath(MenuPath);
        bool opened = false;
        if (!scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Additive);
            opened = true;
        }

        var roots = scene.GetRootGameObjects();
        var help = roots.SelectMany(r => r.GetComponentsInChildren<Button>(true))
                        .FirstOrDefault(b => b.name.Trim() == "help_button");
        var loader = roots.SelectMany(r => r.GetComponentsInChildren<ButtonBehavior>(true)).FirstOrDefault();
        if (help == null || loader == null)
        {
            Debug.LogError("Tutorial: could not find help_button or a ButtonBehavior in the main menu.");
            return;
        }

        while (help.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(help.onClick, 0);
        UnityEventTools.AddStringPersistentListener(help.onClick, loader.LoadScene, "tutorial");
        EditorUtility.SetDirty(help);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (opened) EditorSceneManager.CloseScene(scene, true);
    }
}
