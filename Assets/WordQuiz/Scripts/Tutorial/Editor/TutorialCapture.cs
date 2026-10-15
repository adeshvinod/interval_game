using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FretNoodle > Capture Tutorial Screens (Play Mode, tutorial scene open):
/// renders tutorial screens at exactly 1920x1080 into TutorialCaptures/NN.png,
/// for side-by-side comparison with the design handoff's reference images.
/// </summary>
public static class TutorialCapture
{
    [MenuItem("FretNoodle/Capture Tutorial Screens")]
    static void CaptureAll()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "TutorialCaptures");
        Capture(dir, null);
        EditorUtility.RevealInFinder(dir);
    }

    /// Renders the given screen indices (0-based; null = all) into dir.
    public static string Capture(string dir, int[] indices)
    {
        var ctrl = Object.FindAnyObjectByType<TutorialController>();
        if (!Application.isPlaying || ctrl == null) return "Enter Play Mode in the tutorial scene first.";
        Directory.CreateDirectory(dir);

        var canvas = ctrl.Stage.GetComponentInParent<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        var cam = new GameObject("capture camera").AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = TutorialTheme.ink;
        var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;
        var mode = canvas.renderMode;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 10;
        var handle = typeof(CanvasScaler).GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Instance);

        int start = ctrl.Index;
        indices = indices ?? Enumerable.Range(0, ctrl.ScreenCount).ToArray();
        foreach (int i in indices)
        {
            ctrl.Show(i);
            handle.Invoke(scaler, null);
            Canvas.ForceUpdateCanvases();
            ctrl.FitStageNow();
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(dir, $"{i + 1:00}.png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        ctrl.Show(start);
        canvas.renderMode = mode;
        cam.targetTexture = null;
        Object.DestroyImmediate(cam.gameObject);
        rt.Release();
        return $"Captured {indices.Length} screens to {dir}";
    }
}
