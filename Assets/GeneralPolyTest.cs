using System;
using System.Collections;
using System.Collections.Generic;
using g3;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), ExecuteAlways]
public class GeneralPolyTest : MonoBehaviour
{
    public Mesh mesh;

    private void OnEnable()
    {
        mesh ??= new Mesh();

        List<Vector2d> points = new();

        foreach (Transform child in transform)
        {
            points.Add((Vector2)child.transform.position);
        }

        var poly  = new Polygon2d(points);
        var gPoly = new GeneralPolygon2d(poly);
        var tpg   = new TriangulatedPolygonGenerator();
        tpg.Polygon = gPoly;
        tpg.Generate();
        tpg.MakeMesh(mesh, true);
        mesh.RecalculateBounds();
        
        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;
    }
}