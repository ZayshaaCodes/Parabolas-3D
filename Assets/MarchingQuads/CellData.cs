using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace MarchingQuads
{
    public struct CellData
    {
        public int2 CellId;
        public float3x4 Corners;  // indexes for each corner
        public float4 CornerVals; // indexes for each corner
        public int ShapeType;
        public bool4 Edges;
        public float3x4 EdgePositions;

        public bool slopeLeft;

        public bool4 InnerEdges;
        public float3x4 InnerEdgePositions;

        public CellData(int x, int y, MarchingQuadsGrid grid) : this(new(x, y), grid)
        {
        }

        public CellData(int2 p, MarchingQuadsGrid grid)
        {
            CellId = p;

            Corners = new float3x4() // clockwise from base point
            {
                c0 = grid.pointGrid[p.x, p.y],
                c1 = grid.pointGrid[p.x, p.y + 1],
                c2 = grid.pointGrid[p.x + 1, p.y + 1],
                c3 = grid.pointGrid[p.x + 1, p.y]
            };

            CornerVals = new float4() // clockwise from base point
            {
                x = grid.valueGrid[p.x, p.y],
                y = grid.valueGrid[p.x, p.y + 1],
                z = grid.valueGrid[p.x + 1, p.y + 1],
                w = grid.valueGrid[p.x + 1, p.y]
            };

            Edges = new bool4( // clockwise from base point
                grid.crossingEdges[p.x, p.y][1],
                grid.crossingEdges[p.x, p.y + 1][0],
                grid.crossingEdges[p.x + 1, p.y][1],
                grid.crossingEdges[p.x, p.y][0]
            );


            EdgePositions = new float3x4(
                grid.crossingEdgePoses[p.x, p.y][1],
                grid.crossingEdgePoses[p.x, p.y + 1][0],
                grid.crossingEdgePoses[p.x + 1, p.y][1],
                grid.crossingEdgePoses[p.x, p.y][0]);


            InnerEdges = new bool4( // clockwise from base point
                grid.crossingInnerEdges[p.x, p.y][1],
                grid.crossingInnerEdges[p.x, p.y + 1][0],
                grid.crossingInnerEdges[p.x + 1, p.y][1],
                grid.crossingInnerEdges[p.x, p.y][0]
            );

            InnerEdgePositions = new float3x4(
                grid.crossingInnerEdgePoses[p.x, p.y][1],
                grid.crossingInnerEdgePoses[p.x, p.y + 1][0],
                grid.crossingInnerEdgePoses[p.x + 1, p.y][1],
                grid.crossingInnerEdgePoses[p.x, p.y][0]
            );

            slopeLeft = false;

            ShapeType = 0;
            for (int i = 0; i < 4; i++)
                if (Edges[i])
                    ShapeType += 1 << i;
        }


        public override string ToString()
        {
            return $"{(Edges.x ? 1 : 0)}, {(Edges.y ? 1 : 0)}, {(Edges.z ? 1 : 0)}, {(Edges.w ? 1 : 0)}";
        }

        public void DrawGizmo()
        {
            DrawEdge(Edges,      EdgePositions, 0);
            DrawEdge(InnerEdges, InnerEdgePositions ,-.1f);
        }

        private void DrawEdge(bool4 edgeState, float3x4 edgePositions, float bias)
        {
            int c = 0;
            for (int i = 0; i < 4; i++)
            {
                if (edgeState[i]) c++;
            }

            switch (c)
            {
                case 4:
                    Gizmos.color = Color.yellow;

                    var (p1, p2, p3, p4)
                        = (edgePositions[0], edgePositions[1], edgePositions[2], edgePositions[3]);
                    var (v1, v2, v3, v4)
                        = (CornerVals[0], CornerVals[1], CornerVals[2], CornerVals[3]);
                    if (v1 > bias)
                    {
                        Gizmos.DrawLine(p1, p2);
                        Gizmos.DrawLine(p3, p4);
                    }
                    else
                    {
                        Gizmos.DrawLine(p1, p4);
                        Gizmos.DrawLine(p2, p3);
                    }
                    break;
                case 2:
                    int p1Id = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (edgeState[i])
                        {
                            if (p1Id < 0) p1Id = i;
                            else
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(edgePositions[p1Id], edgePositions[i]);
                            }
                        }
                    }

                    break;
            }
        }
    }
}