using System;
using System.Collections.Generic;
using g3;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

internal class QuadMeshes
{
    Dictionary<bool4, QuadMesh> meshDict = new();

    public QuadMeshes()
    {
        for (int d = 0; d < 2; d++)
        for (int c = 0; c < 2; c++)
        for (int b = 0; b < 2; b++)
        for (int a = 0; a < 2; a++)
        {
            var newBool = new bool4(a > 0, b > 0, c > 0, d > 0);
            meshDict.Add(newBool, new QuadMesh(newBool));
        }

        var val = meshDict[new(true, false, true, false)];
    }

    public void DrawGizmos()
    {
        foreach (var kvp in meshDict)
        {
            Gizmos.matrix  = Matrix4x4.Translate(new(2, 0, 0)) * Gizmos.matrix;
            Handles.matrix = Gizmos.matrix;
            // kvp.Value.DrawGizmo();
        }
    }
}

class QuadMesh
{
    public static Vector3[] Corners =
    {
        new(0, 0),
        new(0, 1),
        new(1, 1),
        new(1, 0)
    };

    private bool[] _ids = new bool[8];
    private int2[] outsideEdges = new int2[2];

    public QuadMesh(bool4 b)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!b[i]) continue;

            _ids[i * 2]           = true;
            _ids[i * 2 + 1]       = true;
            _ids[(i * 2 + 7) % 8] = true;
        }
    }

    public void GenerateQuadData()
    {
        var poly = GetPolygon();

        TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();

        var generalPoly = new GeneralPolygon2d(poly);
        tpg.Polygon = generalPoly;

        tpg.Generate();

        Mesh mesh = new Mesh();
        tpg.MakeMesh(mesh);

        foreach (var meshVertex in mesh.vertices)
        {
            // Debug.Log(meshVertex);
        }

        for (int i = 0; i < mesh.triangles.Length / 3; i++)
        {
            var id1 = mesh.triangles[i * 3];
            var id2 = mesh.triangles[i * 3 + 1];
            var id3 = mesh.triangles[i * 3 + 2];
        }

        var data = new SquareData();
        data.verts = mesh.vertices;
        data.tris  = mesh.triangles;
        var jsonString = JsonUtility.ToJson(data);
    }

    [Serializable]
    public struct SquareData
    {
        public Vector3[] verts;
        public int[] tris;
    }

    private Polygon2d GetPolygon()
    {
        List<Vector2d> newPoly = new();
        for (int i = 0; i < 8; i++)
        {
            if (!_ids[i]) continue;
            newPoly.Add((Vector2)GetIdPosition(i));
        }


        return new Polygon2d(newPoly);
    }

    private Vector3 GetIdPosition(int i)
    {
        if (i is < 0 or >= 8) return Vector3.zero;

        return (i & 1) == 0
                   ? Corners[i / 2]
                   : Vector3.Lerp(Corners[(i - 1) / 2],
                                  Corners[(i + 1) % 8 / 2],
                                  .5f);
    }
}