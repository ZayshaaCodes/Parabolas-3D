using UnityEngine;

namespace ProcGen
{
    [ExecuteAlways]
    public class MarchingSquares : MonoBehaviour
    {
        public MarchingSquaresGrid Grid;

        [Range(1,100)] public int cellCount = 10;

        private void OnEnable()
        {
            Grid = new MarchingSquaresGrid(cellCount);
            OnValidate();
        }

        private void OnValidate()
        {
            if (Grid?.CellCountPerSide != cellCount) Grid?.Resize(cellCount);
        }

        private void OnDrawGizmos()
        {
            Grid.GrawGizmos();
        }
        
    }
}