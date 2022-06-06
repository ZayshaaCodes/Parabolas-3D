using System;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingQuads
{
    [ExecuteAlways]
    public class MarchingQuads : MonoBehaviour
    {
        private MarchingQuadsGrid grid;
        

        [Range(0, 2)] public float scale = 1;
        [Range(10,100)] public int cellCount = 10;

        private void OnEnable()
        {
            grid = new MarchingQuadsGrid();
        }

        private void OnValidate()
        {
            if (grid.CellCountPerSide != cellCount)
            {
                grid.Resize(cellCount);
            }

            grid.SetValueData((x, y) => noise.cnoise(new float2(x * scale, y * scale)));
        }

        private void OnDrawGizmos()
        {
            grid.GrawGizmos(); 
        }
    }
}