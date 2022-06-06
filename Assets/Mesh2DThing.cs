using System.Globalization;
using System.IO.Compression;
using g3;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class BoxGrid
{
    private readonly float[][] grid;
    private readonly int size;
    private float spacing;

    private QuadMeshes qMeshes = new QuadMeshes();

    public BoxGrid(int size)
    {
        spacing   = 1;
        this.size = size + 1;
        grid      = new float[this.size][];
        for (int y = 0; y < this.size; y++)
        {
            grid[y] = new float[this.size];
            for (int x = 0; x < this.size; x++)
            {
                grid[y][x] = noise.cnoise(new float2(x, y) * .4f);
            }
        }
    }

    public void DrawGizmos()
    {
        for (int j = 0; j < size; j++)
        for (int i = 0; i < size; i++)
        {
            var val = grid[i][j];
            Gizmos.color = val > 0 ? Color.white : Color.black;
            // Gizmos.color = Color.Lerp(Color.white, Color.clear, math.unlerp(-1f, 1f, val));
            var pos = new Vector3(i * spacing, j * spacing);
            Gizmos.DrawSphere(pos, .2f);
            // Handles.Label(pos, $"{val:f3}");
        }

        for (int i = 0; i < size; i++)
        {
        }
    }
}

[RequireComponent(typeof(MeshFilter)), ExecuteAlways]
public class Mesh2DThing : MonoBehaviour
{
    private Mesh mesh;

    private BoxGrid _grid;

    private void OnEnable()
    {
        _grid = new BoxGrid(100);

        if (mesh == null) mesh = new Mesh { name = "Cubed Mesh" };
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        // _grid.DrawGizmos();

        ImplicitOperator2d op = new ImplicitDifference2d();
        op.AddChild(new ImplicitPoint2d(0, 0, 2));
        op.AddChild(new ImplicitPoint2d(2, 0, 2));
        

        Gizmos.color = Color.white;
        for (int y = -50; y <50; y++)
        for (int x = -50; x < 50; x++)
        {
            var pos = new Vector3(x / 5f, y / 5f);
            var value   = op.Value(pos.x, pos.y);
            // Gizmos.color = Color.Lerp(Color.black, Color.white, value);
            if (value > .5f)
            {
                Gizmos.DrawSphere(pos, .1f);
            }
        }
    }
}