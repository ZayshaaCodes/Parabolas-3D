using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MarchingQuads
{
    public class MarchingQuadsGrid
    {
        public int CellCountPerSide = 10;
        public int verCountPerSide => CellCountPerSide + 1;

        public float3[,] pointGrid;
        public float[,] valueGrid;
        public bool2[,] crossingEdges;
        public float3x2[,] crossingEdgePoses;

        public float cellUnitSize = 5;

        CellData GetCellData(int x, int y) => new(x, y, this);
        CellData GetCellData(int2 p) => GetCellData(p.x, p.y);

        public MarchingQuadsGrid()
        {
            InitializeGrid();
            SetValueData((i, j) => noise.cnoise(new float2(i * .3f, j * .3f)));
        }

        public void InitializeGrid()
        {
            var pCount = verCountPerSide;
            pointGrid         = new float3[pCount, pCount];
            valueGrid         = new float[pCount, pCount];
            crossingEdges     = new bool2[pCount, pCount];
            crossingEdgePoses = new float3x2[pCount, pCount];

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

                var val   = valueGrid[i, j];
                var valOn = val > 0;

                // update horizontal edges
                if (i < c - 1)
                {
                    var rightPos = pointGrid[i + 1, j];

                    var rightVal   = valueGrid[i + 1, j];
                    var rightValOn = rightVal > 0;

                    var isCrossing = rightValOn != valOn;
                    crossingEdges[i, j].x      = isCrossing;
                    crossingEdgePoses[i, j].c0 = math.lerp(pos, rightPos, math.unlerp(val, rightVal, 0));
                }

                if (j < c - 1)
                {
                    var upPos = pointGrid[i, j + 1];

                    var upVal   = valueGrid[i, j + 1];
                    var ipValOn = upVal > 0;

                    var isCrossing = ipValOn != valOn;
                    crossingEdges[i, j].y = isCrossing;
                    if (isCrossing)
                        crossingEdgePoses[i, j].c1 = math.lerp(pos, upPos, math.unlerp(val, upVal, 0));
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
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(-.2f, .2f, v));

                Gizmos.DrawWireCube(point, Vector3.one * 1);
                
                Gizmos.color = Color.red;
                if (i < c - 1 && crossing.x) Gizmos.DrawWireSphere(crossingPos.c0, .25f);
                if (j < c - 1 && crossing.y) Gizmos.DrawWireSphere(crossingPos.c1, .25f);
            });
            
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