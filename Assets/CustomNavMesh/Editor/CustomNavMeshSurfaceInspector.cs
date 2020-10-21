using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshSurface))]
public class CustomNavMeshSurfaceInspector : Editor
{
    CustomNavMeshSurface surface;
    // cache to then unhide it; why manipulate the hide flags in the custom editor? having it on 
    // OnValidate won't work instantly when dragging the prefab to the scene, that's why
    MeshFilter meshFilter;

    SerializedProperty mesh;

    private void OnEnable()
    {
        surface = target as CustomNavMeshSurface;
        meshFilter = surface.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            // prevent the user from changing the mesh through the MeshFilter's inspector
            meshFilter.hideFlags = HideFlags.HideInInspector;
        }

        mesh = serializedObject.FindProperty("mesh");
    }

    private void OnDisable()
    {
        if (target == null && meshFilter != null)
        {
            meshFilter.hideFlags = HideFlags.None;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(mesh);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Surface Mesh");

            var meshFilter = surface.GetComponent<MeshFilter>();
            if(meshFilter != null) Undo.RecordObject(meshFilter, "");

            surface.Mesh = (Mesh) mesh.objectReferenceValue;
        }

        serializedObject.ApplyModifiedProperties(); // needed to create a prefab override
    }
}
