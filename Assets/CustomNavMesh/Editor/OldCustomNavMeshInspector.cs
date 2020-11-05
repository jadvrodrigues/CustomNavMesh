using UnityEditor;

[CustomEditor(typeof(OldCustomNavMesh))]
public class OldCustomNavMeshInspector : Editor
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
            OldCustomNavMesh.HiddenTranslation = hiddenTranslation.vector3Value;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(renderHidden);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Render Hidden");
            OldCustomNavMesh.RenderHidden = renderHidden.boolValue;
        }
    }
}
