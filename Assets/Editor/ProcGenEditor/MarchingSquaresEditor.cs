using System;
using ProcGen;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.ProcGenEditor
{
    public class MarchingSquaresEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        private BaseDrawShape drawShape = new DrawCircle();

        [SerializeField, Range(0, 5)] private float _cellDrawSize = 1;
        [SerializeField] private Color color;

        private MarchingSquares _squares;

        private Plane zPlane = new Plane(Vector3.forward, Vector3.zero);
        private int _blockingId;

        [MenuItem("Window/Editors/Marching Squares")]
        public static void ShowExample()
        {
            MarchingSquaresEditor wnd = GetWindow<MarchingSquaresEditor>();
            wnd.titleContent = new GUIContent("MarchingSquaresEditor");
            wnd.position     = new Rect(100, 100, 500, 900);
        }

        private void OnEnable()
        {
            _blockingId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(_blockingId);

            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
        }

        float GetOtherLeg(float leg, float r)
        {
            return r * math.cos(math.asin(leg / r));
        }


        private void OnSceneGui(SceneView sceneView)
        {
            if (!_squares) return;
            
            var mousePos = Event.current.mousePosition;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
            var screenRay = sceneView.camera.ScreenPointToRay(mousePos);
            if (zPlane.Raycast(screenRay, out float hitDistance))
            {
                var snappedMousePos = (float3)screenRay.GetPoint(hitDistance);
                snappedMousePos = math.floor(snappedMousePos / 2.5f) * 2.5f;

                var edgeSpan = _squares.Grid.GetEdgeSpanAroundPoint(snappedMousePos, _cellDrawSize / 2);
                edgeSpan = _squares.Grid.ClampToEdgeBounds(edgeSpan);

                var drawSettings = new DrawSettings() { position = snappedMousePos, drawSize = _cellDrawSize, cellSize = _squares.Grid.cellUnitSize };

                drawShape?.DrawHandle(drawSettings);

                if (Event.current.isMouse
                    && Event.current.button == 0
                    && Event.current.type == EventType.MouseDown
                    && !Event.current.alt)
                {
                    GUIUtility.hotControl = _blockingId;
                    drawShape?.Fill(_squares.Grid, drawSettings, !Event.current.shift);
                }
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject is { } sel)
                _squares = sel.GetComponent<MarchingSquares>();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            VisualElement ui   = m_VisualTreeAsset.Instantiate();

            if (ui.Q<Button>("clearButton") is { } clearButton)
            {
                clearButton.clickable = new Clickable(() => { _squares.Grid.LoopGridPoints((i, j, c) => { _squares.Grid.valueGrid[i, j] = false; }); });
            }

            root.Add(ui);
            root.Bind(new SerializedObject(this));
        }
    }


}