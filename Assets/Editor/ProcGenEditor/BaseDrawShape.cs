using System;
using ProcGen;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Editor.ProcGenEditor
{
    
    [Serializable]
    public struct DrawSettings
    {
        public Vector3 position;
        public float drawSize;
        public float cellSize;
        public bool value;
    }
    
    [Serializable]
    public abstract class BaseDrawShape
    {
        public abstract void DrawHandle(DrawSettings dInfo);
        public abstract void Fill(MarchingSquaresGrid grid, DrawSettings dInfo, bool value);
    }

    public class DrawCircle : BaseDrawShape
    {
        public override void DrawHandle(DrawSettings dInfo)
        {
            var drawWorldRad = dInfo.drawSize * dInfo.cellSize / 2;

            Handles.DrawWireDisc(dInfo.position, Vector3.forward, drawWorldRad);
        }

        public int GetIntersections( )
        {
            return 0;
        }

        public override void Fill(MarchingSquaresGrid grid, DrawSettings dInfo, bool value)
        {
            var drawWorldRad = dInfo.drawSize * dInfo.cellSize / 2;
            var eSpan        = grid.GetEdgeSpanAroundPoint(dInfo.position, dInfo.drawSize / 2);
            eSpan = grid.ClampToEdgeBounds(eSpan);

            grid.LoopGridPointsSpan(eSpan, (i, j) =>
            {
                if (drawWorldRad * drawWorldRad > math.distancesq(grid.pointGrid[i, j], dInfo.position))
                {
                    grid.valueGrid[i, j] = value;
                }
            });
            
            
        }
    }

}