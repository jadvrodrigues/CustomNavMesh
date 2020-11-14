using UnityEngine;
using UnityEditor;
using System;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshSurface))]
public class CustomNavMeshSurfaceInspector : Editor
{
    SerializedProperty mesh;

    CustomNavMeshSurface[] surfaces
    {
        get
        {
            return Array.ConvertAll(targets, obj => (CustomNavMeshSurface)obj);
        }
    }

    private void OnEnable()
    {
        var surface = target as CustomNavMeshSurface;
        surface.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;

        mesh = serializedObject.FindProperty("mesh");
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

            foreach (var surface in surfaces)
            {
                var meshFilter = surface.GetComponent<MeshFilter>();
                if (meshFilter != null) Undo.RecordObject(meshFilter, "");

                surface.Mesh = (Mesh)mesh.objectReferenceValue;
            }

            serializedObject.ApplyModifiedProperties(); // needed to create a prefab override
        }
    }
}
