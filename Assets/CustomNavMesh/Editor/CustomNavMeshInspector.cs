using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomNavMesh))]
public class CustomNavMeshInspector : Editor
{
    SerializedProperty hiddenTranslation;
    SerializedProperty renderHidden;

    private void OnEnable()
    {
        hiddenTranslation = serializedObject.FindProperty("hiddenTranslation");
        renderHidden = serializedObject.FindProperty("renderHidden");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(hiddenTranslation, new GUIContent( "Hidden Translation", 
                "The distance between the shown surface and the hidden surface where most of " +
                "the CustomNavMesh's calculations are done. Make sure it is big enough so " +
                "both the custom surface and the hidden one don't overlap.")
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Hidden Translation");
            CustomNavMesh.HiddenTranslation = hiddenTranslation.vector3Value;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(renderHidden, new GUIContent("Render Hidden",
                "Whether the hidden nav mesh components are visible or not. Disable it if you " +
                "don't want them to be visible and you're not going to bake the NavMesh.")
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Render Hidden");
            CustomNavMesh.RenderHidden = renderHidden.boolValue;
        }

        if(!renderHidden.boolValue)
        {
            EditorGUILayout.HelpBox("Make sure the Render Hidden is set to true " +
                "if you're going to bake the NavMesh.", MessageType.Warning);
        }
    }
}
