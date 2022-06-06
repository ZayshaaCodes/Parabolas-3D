using Unity.Mathematics;
using UnityEngine;

namespace MarchingQuads
{
    public struct CellData
    {
        public int2 cellId;
        public int2x4 cornersIds; // indexes for each corner
        public int shapeType;
        public bool4 edges;
        public float3x4 edgePositions;

        private readonly MarchingQuadsGrid _grid;

        public CellData(int x, int y, MarchingQuadsGrid grid) : this(new(x, y), grid)
        {
        }

        public CellData(int2 p, MarchingQuadsGrid grid)
        {
            _grid  = grid;
            cellId = p;


            cornersIds = new int2x4 // clockwise from base point
            {
                c0 = new(p.x, p.y),
                c1 = new(p.x, p.y + 1),
                c2 = new(p.x + 1, p.y + 1),
                c3 = new(p.x + 1, p.y)
            };

            edges = new bool4( // clockwise from base point
                grid.crossingEdges[p.x, p.y][1],
                grid.crossingEdges[p.x, p.y + 1][0],
                grid.crossingEdges[p.x + 1, p.y][1],
                grid.crossingEdges[p.x, p.y][0]
            );

            edgePositions = new float3x4(
                grid.crossingEdgePoses[p.x, p.y][1],
                grid.crossingEdgePoses[p.x, p.y + 1][0],
                grid.crossingEdgePoses[p.x + 1, p.y][1],
                grid.crossingEdgePoses[p.x, p.y][0]);

            shapeType = 0;
            for (int i = 0; i < 4; i++)
                if (edges[i])
                    shapeType += 1 << i;
        }


        public override string ToString()
        {
            return $"{(edges.x ? 1 : 0)}, {(edges.y ? 1 : 0)}, {(edges.z ? 1 : 0)}, {(edges.w ? 1 : 0)}";
        }

        public void DrawGizmo()
        {
            // Gizmos.matrix = Matrix4x4.Translate(posData[cellId.x, cellId.y]);

            for (int i = 0; i < 4; i++)
            {
                var p1 = _grid.pointGrid[cornersIds[i].x, cornersIds[i].y];
                var p2 = _grid.pointGrid[cornersIds[(i + 1) % 4].x, cornersIds[(i + 1) % 4].y];
                Gizmos.color = Color.blue;
                if (edges[i])
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(edgePositions[i], 1f);
                }

                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}