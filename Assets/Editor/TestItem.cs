using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
#endif

[Serializable]
public class TestItem
{
    public string name;

    public string test = "42";
    public List<int> coolSubItems;

    public override string ToString()
    {
        return $"{name} : {test} : {string.Join(", ", coolSubItems)}";
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TestItem))]
public class TestItemDrawer : PropertyDrawer
{
    private static VisualTreeAsset _itemTreeAsset;
    
    static TestItemDrawer()
    {
        _itemTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ListItem.uxml");
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var tempContainer = _itemTreeAsset.Instantiate(property.propertyPath);

        if (tempContainer.Q<Button>() is {} button)
        {
            button.clickable = new Clickable(() =>
            {
                var pathMatch = Regex.Match(tempContainer.bindingPath, @"([\w\-\[\]\.]+)\.Array\.data\[(\d+)\]");
                if (!pathMatch.Success) return;
                
                var arrayPath = pathMatch.Groups[1].Value;
                var index     = int.Parse(pathMatch.Groups[2].Value);
                var target    = property.serializedObject.targetObject;

                var field = target.GetType().GetField(arrayPath);
                var array = field.GetValue(target) as List<TestItem>;
            
                array?.RemoveAt(index);
                // Debug.Log(array.Count);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        return tempContainer;
    }
}
#endif