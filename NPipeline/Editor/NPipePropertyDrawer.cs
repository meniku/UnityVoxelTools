using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NPipeSelectorAttribute))]
public class NPipePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if( Selection.gameObjects.Length > 1 )
        {
            EditorGUI.LabelField(position, label.text + ": Multiple object editing not yet supported, sorry");
            return;
        }

        NPipeSelectorAttribute selector = attribute as NPipeSelectorAttribute;
        UnityEngine.Object val = (UnityEngine.Object)typeof(NPipelineUtils)
            .GetMethod("DrawSourcePropertySelector")
            .MakeGenericMethod(selector.Type)
            .Invoke(null, new object[] {
                label, position, property.objectReferenceValue, null
            });

        property.objectReferenceValue = val;
    }
}