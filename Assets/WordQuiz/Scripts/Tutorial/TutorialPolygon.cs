using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A filled convex polygon for uGUI, in the RectTransform's own space with the
/// origin at the top-left and y pointing down (the CSS clip-path convention).
/// Used for the HUD's centre trapezoid.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TutorialPolygon : MaskableGraphic
{
    public Vector2[] points = new Vector2[0];

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points == null || points.Length < 3) return;
        var r = rectTransform.rect;
        foreach (var p in points)
            vh.AddVert(new Vector3(r.xMin + p.x, r.yMax - p.y), color, Vector2.zero);
        for (int i = 1; i < points.Length - 1; i++) vh.AddTriangle(0, i, i + 1);
    }
}
