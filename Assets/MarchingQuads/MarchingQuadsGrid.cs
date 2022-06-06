using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MarchingQuads
{
    public class MarchingQuadsGrid
    {
        public int CellCountPerSide = 50;
        public int verCountPerSide => CellCountPerSide + 1;

        public float3[,] pointGrid;
        public float[,] valueGrid;
        public bool2[,] crossingEdges;
        public float3x2[,] crossingEdgePoses;

        public bool2[,] crossingInnerEdges;
        public float3x2[,] crossingInnerEdgePoses;

        public float cellUnitSize = 5;

        CellData GetCellData(int x, int y) => new(x, y, this);
        CellData GetCellData(int2 p) => GetCellData(p.x, p.y);

        public MarchingQuadsGrid()
        {
            InitializeGrid();
        }

        public MarchingQuadsGrid(int cellCount)
        {
            CellCountPerSide = cellCount;
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            var pCount = verCountPerSide;
            pointGrid         = new float3[pCount, pCount];
            valueGrid         = new float[pCount, pCount];
            crossingEdges     = new bool2[pCount, pCount];
            crossingEdgePoses = new float3x2[pCount, pCount];

            crossingInnerEdges     = new bool2[pCount, pCount];
            crossingInnerEdgePoses = new float3x2[pCount, pCount];

            LoopGridPoints((i, j, c) => { pointGrid[i, j] = new Vector3(i, j) * cellUnitSize; });
        }

        public void Resize(int cellCount)
        {
            CellCountPerSide = cellCount;
            InitializeGrid();
        }

        public void SetValueData(Func<int, int, float> function)
        {
            LoopGridPoints((i, j, c) => { valueGrid[i, j] = function(i, j); });
            UpdateEdgeData();
        }

        private void UpdateEdgeData()
        {
            LoopGridPoints((i, j, c) =>
            {
                var pos = pointGrid[i, j];

                var val        = valueGrid[i, j];
                var valOn      = val > 0;
                var valInnerOn = val > -.1f;

                // update horizontal edges
                if (i < c - 1)
                {
                    var otherPos   = pointGrid[i + 1, j];
                    var otherVal   = valueGrid[i + 1, j];
                    var otherValOn = otherVal > 0;

                    var isCrossing = otherValOn != valOn;
                    crossingEdges[i, j].x      = isCrossing;
                    crossingEdgePoses[i, j].c0 = math.lerp(pos, otherPos, math.unlerp(val, otherVal, 0));

                    var otherValInnerOn = otherVal > -.1f;
                    var isInnerCrossing = otherValInnerOn != valInnerOn;
                    crossingInnerEdges[i, j].x      = isInnerCrossing;
                    crossingInnerEdgePoses[i, j].c0 = math.lerp(pos, otherPos, math.unlerp(val, otherVal, -.1f));
                }

                if (j < c - 1)
                {
                    var otherPos   = pointGrid[i, j + 1];
                    var otherVal   = valueGrid[i, j + 1];
                    var otherValOn = otherVal > 0;

                    var isCrossing = otherValOn != valOn;
                    crossingEdges[i, j].y      = isCrossing;
                    crossingEdgePoses[i, j].c1 = math.lerp(pos, otherPos, math.unlerp(val, otherVal, 0));

                    var otherValInnerOn = otherVal > -.1f;
                    var isInnerCrossing = otherValInnerOn != valInnerOn;
                    crossingInnerEdges[i, j].y      = isInnerCrossing;
                    crossingInnerEdgePoses[i, j].c1 = math.lerp(pos, otherPos, math.unlerp(val, otherVal, -.1f));
                }
            });
        }

        public int2 WorldToCellId(Vector3 worldPosition)
        {
            var flooredPos = worldPosition / cellUnitSize;

            return new int2(Mathf.FloorToInt(flooredPos.x), Mathf.FloorToInt(flooredPos.y));
        }

        public bool IdInBounds(int2 id)
        {
            return id.x >= 0 && id.x < CellCountPerSide && id.y >= 0 && id.y < CellCountPerSide;
        }

        public void GrawGizmos()
        {
            LoopGridPoints((i, j, c) =>
            {
                float3   point       = pointGrid[i, j];
                float    v           = valueGrid[i, j];
                bool2    crossing    = crossingEdges[i, j];
                float3x2 crossingPos = crossingEdgePoses[i, j];

                // Gizmos.color = v > bias ? Color.white : Color.black;
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(-.2f, 0, v));

                Gizmos.DrawWireCube(point, Vector3.one * 1);

                Gizmos.color = Color.red;
                if (i < c - 1 && crossing.x) Gizmos.DrawWireSphere(crossingPos.c0, .25f);
                if (j < c - 1 && crossing.y) Gizmos.DrawWireSphere(crossingPos.c1, .25f);
            });

            LoopGridPoints((i, j, c) =>
            {
                var cell = GetCellData(i, j);
                cell.DrawGizmo();
            }, true);

            DrawMouseOverGizmo();
        }

        private void DrawMouseOverGizmo()
        {
            var mPos = Event.current.mousePosition;
            var cam  = SceneView.lastActiveSceneView.camera;
            mPos.y = cam.scaledPixelHeight - mPos.y;

            var   screenRay = cam.ScreenPointToRay(mPos);
            Plane zPlane    = new Plane(Vector3.forward, 0);
            if (zPlane.Raycast(screenRay, out var hit))
            {
                Gizmos.DrawWireSphere(screenRay.GetPoint(hit), 1f);
                var hitPos = screenRay.GetPoint(hit);
                var cellId = WorldToCellId(hitPos);

                if (IdInBounds(cellId))
                {
                    var cell = GetCellData(cellId);
                    cell.DrawGizmo();
                }
            }
        }

        void LoopGridPoints(Action<int, int, int> function, bool subOne = false)
        {
            var c = subOne ? CellCountPerSide : CellCountPerSide + 1;

            for (int j = 0; j < c; j++)
            for (int i = 0; i < c; i++)
            {
                function(i, j, c);
            }
        }
    }
}