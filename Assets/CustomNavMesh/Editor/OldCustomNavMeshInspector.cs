using UnityEditor;

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
        EditorGUILayout.PropertyField(hiddenTranslation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Hidden Translation");
            CustomNavMesh.HiddenTranslation = hiddenTranslation.vector3Value;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(renderHidden);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Render Hidden");
            CustomNavMesh.RenderHidden = renderHidden.boolValue;
        }
    }
}
