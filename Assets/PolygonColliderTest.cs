using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class PolygonColliderTest : MonoBehaviour
{
    void OnEnable()
    {
        var pc = GetComponent<PolygonCollider2D>();
        pc.pathCount = 2;

        pc.SetPath(0, new[]
        {
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1,  1),
            new Vector2(1,  -1),
        });

        pc.SetPath(1, new[]
        {
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(2,  1),
            new Vector2(2,  -1),
        });
    }
}