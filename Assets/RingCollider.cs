using System;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D)), ExecuteAlways]
public class RingCollider : MonoBehaviour
{
    private PolygonCollider2D _pc2d;

    public int pointCount = 64;

    public float id = 2;
    public float thickness = 2;

    private void OnValidate()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        _pc2d = GetComponent<PolygonCollider2D>();

        Vector2[] path1 = new Vector2[pointCount];
        Vector2[] path2 = new Vector2[pointCount];

        var step = Mathf.PI / pointCount *2;

        for (int i = 0; i < pointCount; i++)
        {
            var angle = i * step;
            path1[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (id / 2);
            path2[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (id / 2 + thickness);
        }

        _pc2d.pathCount = 2;
        _pc2d.SetPath(0, path1);
        _pc2d.SetPath(1, path2);
    }
}