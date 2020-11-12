using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomAgentController)), CanEditMultipleObjects]
public class CustomAgentControllerInspector : Editor
{
    SerializedProperty targetPosition;
    SerializedProperty moveOffset;
    SerializedProperty newPosition;

    private void OnEnable()
    {
        targetPosition = serializedObject.FindProperty("targetPosition");
        moveOffset = serializedObject.FindProperty("moveOffset");
        newPosition = serializedObject.FindProperty("newPosition");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(targetPosition);
        if (GUILayout.Button("Set Destination"))
        {
            foreach(CustomAgentController controller in targets)
                controller.SetDestination(targetPosition.vector3Value);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(moveOffset);
        if (GUILayout.Button("Move"))
        {
            foreach (CustomAgentController controller in targets)
                controller.Move(moveOffset.vector3Value);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(newPosition);
        if (GUILayout.Button("Warp"))
        {
            foreach (CustomAgentController controller in targets)
                controller.Warp(newPosition.vector3Value);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("Reset Path"))
        {
            foreach (CustomAgentController controller in targets)
                controller.ResetPath();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
