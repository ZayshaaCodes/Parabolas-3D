using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ProcGen
{
    public class MarchingSquaresGrid
    {
        public int CellCountPerSide = 50;
        public int vertCountPerSide => CellCountPerSide + 1;

        public float3[,] pointGrid;
        public bool[,] valueGrid;
        public bool2[,] crossingEdges;
        public float3x2[,] crossingEdgePoses;

        // public bool2[,] crossingInnerEdges;
        // public float3x2[,] crossingInnerEdgePoses;

        public float cellUnitSize = 5;
        public float widthAndHeight => cellUnitSize * CellCountPerSide;

        public CellData GetCellData(int x, int y) => new(x, y, this);
        public CellData GetCellData(int2 p) => GetCellData(p.x, p.y);

        public MarchingSquaresGrid()
        {
            InitializeGrid();
        }

        public MarchingSquaresGrid(int cellCount)
        {
            CellCountPerSide = cellCount;
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            var pCount = vertCountPerSide;
            pointGrid         = new float3[pCount, pCount];
            valueGrid         = new bool[pCount, pCount];
            crossingEdges     = new bool2[pCount, pCount];
            crossingEdgePoses = new float3x2[pCount, pCount];

            // crossingInnerEdges     = new bool2[pCount, pCount];
            // crossingInnerEdgePoses = new float3x2[pCount, pCount];

            LoopGridPoints((i, j, c) => { pointGrid[i, j] = new Vector3(i, j) * cellUnitSize; });
        }

        public void Resize(int cellCount)
        {
            CellCountPerSide = cellCount;
            InitializeGrid();
        }

        public void SetValueData(Func<int, int, bool> function)
        {
            LoopGridPoints((i, j, c) => { valueGrid[i, j] = function(i, j); });
        }

        public int2x2 GetCellSpanAroundPoint(float3 worldPos, float cellUnitRadius)
        {
            var cell = WorldToCellPos(worldPos);

            var min = (int2)math.floor(cell - new float2(cellUnitRadius));
            min = math.max(min, int2.zero);
            var max = (int2)math.floor(cell + new float2(cellUnitRadius));
            max = math.min(max, new int2(CellCountPerSide - 1));
            max = math.max(max, 0);
            min = math.min(min, max);

            return new(min, max);
        }

        public int2x2 GetEdgeSpanAroundPoint(float3 worldPos, float drawSize)
        {
            var cellpos = WorldToCellPos(worldPos);
            var min     = (int2)math.ceil(cellpos - drawSize);
            var max     = (int2)math.ceil(cellpos + drawSize);

            return new int2x2(min, max);
        }

        public int2x2 ClampToEdgeBounds(int2x2 value)
        {
            return ClampToBounds(value, 0, vertCountPerSide);
        }
        
        public int2x2 ClampToCellBounds(int2x2 value)
        {
            return ClampToBounds(value, 0, CellCountPerSide);
        }
        
        public int2x2 ClampToBounds(int2x2 value, int min, int max)
        {
            value.c0 = math.max(min, value.c0);
            value.c1 = math.min(max, value.c1);
            value.c0 = math.min(value.c0, value.c1);
            value.c1 = math.max(value.c0, value.c1);

            return value;
        }

        public int2 WorldToCellId(Vector3 worldPosition) => (int2)math.floor(WorldToCellPos(worldPosition));
        public float2 WorldToCellPos(Vector3 worldPosition) => ((float3)worldPosition / cellUnitSize).xy;
        public bool IdInBounds(int2 id) => id.x >= 0 && id.x < CellCountPerSide && id.y >= 0 && id.y < CellCountPerSide;

        public void GrawGizmos()
        {
            LoopGridPoints((i, j, c) =>
            {
                float3   point       = pointGrid[i, j];
                bool     v           = valueGrid[i, j];
                bool2    crossing    = crossingEdges[i, j];
                float3x2 crossingPos = crossingEdgePoses[i, j];

                // Gizmos.color = v > bias ? Color.white : Color.black;
                Gizmos.color = v ? Color.white : new Color(0f, 0f, 0f, 0.2f);
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

            // DrawMouseOverGizmo();
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

        public void LoopGridPoints(Action<int, int, int> function, bool subOne = false)
        {
            var c = subOne ? CellCountPerSide : CellCountPerSide + 1;

            for (int j = 0; j < c; j++)
            for (int i = 0; i < c; i++)
            {
                function(i, j, c);
            }
        }
        
        public void LoopGridPointsSpan(int2x2 span, Action<int, int> function)
        {
            for (int j = span.c0.y; j < span.c1.y; j++)
            for (int i = span.c0.x; i < span.c1.x; i++)
            {
                function(i, j);
            }
        }

        public void LoopGridEdgeSpan(int2x2 span, Action<int, int, float3> function, bool inludeBorders = false)
        {

            for (int i = span.c0.x; i < span.c1.x; i++)
            {
                var pos = pointGrid[i, 0];
                function(i, -1, pos);
            }

            for (int j = span.c0.y; j < span.c1.y; j++)
            {
                var pos = pointGrid[0, j];
                function(-1, j, pos);
            }
        }

        public void LoopGridCellsRange(int2x2 span, Action<int, int, CellData> function)
        {
            for (int j = span.c0.y; j < span.c1.y; j++)
            for (int i = span.c0.x; i < span.c1.x; i++)
            {
                var cd = GetCellData(i, j);
                function(i, j, cd);
            }
        }
    }
}