using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class TestWindow : EditorWindow
{
    public List<TestItem> coolItems;

    private static SerializedObject _windowSo;

    [MenuItem("Test/Window")]
    public static void ShowExample()
    {
        TestWindow wnd = GetWindow<TestWindow>();
        wnd.titleContent = new GUIContent("TestWindow");
        wnd.position     = new Rect(0, 0, 300, 500);
    }

    public void CreateGUI()
    {
        coolItems = new List<TestItem>()
        {
            new() { name = "Hello", coolSubItems  = new() { 1, 2 } },
            new() { name = "Hello2", coolSubItems = new() { 2, 3 } },
            new() { name = "Hello3", coolSubItems = new() { 2, 3 } },
            new() { name = "Hello4", coolSubItems = new() { 2, 3 } }
        };

        _windowSo = new SerializedObject(this);
        var root = rootVisualElement;

        var uiTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TestWindow.uxml");
        if (uiTreeAsset == null) return;

        root.Add(uiTreeAsset.Instantiate());
        root.Bind(_windowSo);
    }
}