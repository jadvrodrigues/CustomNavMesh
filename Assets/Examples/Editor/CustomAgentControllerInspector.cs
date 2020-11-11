using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomAgentController))]
public class CustomAgentControllerInspector : Editor
{
    CustomAgentController controller;

    SerializedProperty targetPosition;

    private void OnEnable()
    {
        controller = target as CustomAgentController;

        targetPosition = serializedObject.FindProperty("targetPosition");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(targetPosition);
        if (GUILayout.Button("Set Destination"))
        {
            controller.SetDestination(targetPosition.vector3Value);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
