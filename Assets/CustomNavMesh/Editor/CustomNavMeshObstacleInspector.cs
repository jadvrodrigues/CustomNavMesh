using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshObstacle))]
public class CustomNavMeshObstacleInspector : Editor
{
    SerializedProperty m_Shape;
    SerializedProperty m_Center;
    SerializedProperty m_Size;
    SerializedProperty m_Carve;
    SerializedProperty m_MoveThreshold;
    SerializedProperty m_TimeToStationary;
    SerializedProperty m_CarveOnlyStationary;

    CustomNavMeshObstacle[] Obstacles
    {
        get
        {
            return Array.ConvertAll(targets, obj => (CustomNavMeshObstacle)obj);
        }
    }

    private void OnEnable()
    {
        var obstacle = target as CustomNavMeshObstacle;
        var navMeshObstacle = obstacle.GetComponent<NavMeshObstacle>();
        if (navMeshObstacle != null)
        {
            // prevent the user from changing the obstacle through the NavMeshObstacle's inspector
            navMeshObstacle.hideFlags = HideFlags.HideInInspector;
        }

        m_Shape = serializedObject.FindProperty("m_Shape");
        m_Center = serializedObject.FindProperty("m_Center");
        m_Size = serializedObject.FindProperty("m_Size");
        m_Carve = serializedObject.FindProperty("m_Carve");
        m_MoveThreshold = serializedObject.FindProperty("m_MoveThreshold");
        m_TimeToStationary = serializedObject.FindProperty("m_TimeToStationary");
        m_CarveOnlyStationary = serializedObject.FindProperty("m_CarveOnlyStationary");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Shape);
        if (EditorGUI.EndChangeCheck()) // if shape changed
        {
            Vector3 size = Vector3.zero;
            if (m_Shape.enumValueIndex == 0) // capsule
            {
                size = new Vector3(1.0f, 2.0f, 1.0f);
            }
            else if (m_Shape.enumValueIndex == 1) // box
            {
                size = Vector3.one;
            }

            foreach (var obstacle in Obstacles)
            {
                Undo.RecordObject(obstacle, "Changed Obstacle Shape");
                obstacle.RecordNavMeshObstacle();
                obstacle.Shape = (NavMeshObstacleShape)m_Shape.enumValueIndex;
                obstacle.Center = Vector3.zero;
                obstacle.Size = size;
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Center);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obstacle in Obstacles)
            {
                Undo.RecordObject(obstacle, "Changed Obstacle Center");
                obstacle.RecordNavMeshObstacle();
                obstacle.Center = m_Center.vector3Value;
            }

            serializedObject.ApplyModifiedProperties();
        }

        if (m_Shape.enumValueIndex == 0)
        {
            EditorGUI.BeginChangeCheck();
            float radius = EditorGUILayout.FloatField("Radius", m_Size.vector3Value.x / 2.0f);
            float height = EditorGUILayout.FloatField("Height", m_Size.vector3Value.y);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obstacle in Obstacles)
                {
                    Undo.RecordObject(obstacle, "Changed Obstacle Size");
                    obstacle.RecordNavMeshObstacle();
                    obstacle.Size = new Vector3(radius * 2.0f, height, radius * 2.0f);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
        else if (m_Shape.enumValueIndex == 1)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 size = m_Size.vector3Value;
            size = EditorGUILayout.Vector3Field("Size", size);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obstacle in Obstacles)
                {
                    Undo.RecordObject(obstacle, "Changed Obstacle Size");
                    obstacle.RecordNavMeshObstacle();
                    obstacle.Size = size;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Carve);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obstacle in Obstacles)
            {
                Undo.RecordObject(obstacle, "Changed Obstacle Carving");
                obstacle.RecordNavMeshObstacle();
                obstacle.Carving = m_Carve.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        if (m_Carve.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MoveThreshold);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obstacle in Obstacles)
                {
                    Undo.RecordObject(obstacle, "Changed Obstacle Carving Move Threshold");
                    obstacle.RecordNavMeshObstacle();
                    obstacle.CarvingMoveThreshold = m_MoveThreshold.floatValue;
                }

                serializedObject.ApplyModifiedProperties();
            }


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_TimeToStationary);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obstacle in Obstacles)
                {
                    Undo.RecordObject(obstacle, "Changed Obstacle Carving Time To Stationary");
                    obstacle.RecordNavMeshObstacle();
                    obstacle.CarvingTimeToStationary = m_TimeToStationary.floatValue;
                }

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_CarveOnlyStationary);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obstacle in Obstacles)
                {
                    Undo.RecordObject(obstacle, "Changed Obstacle Carving Only Stationary");
                    obstacle.RecordNavMeshObstacle();
                    obstacle.CarveOnlyStationary = m_CarveOnlyStationary.boolValue;
                }

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel--;
        }
    }
}
