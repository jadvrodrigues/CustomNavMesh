using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshObstacle))]
public class CustomNavMeshObstacleInspector : Editor
{
    CustomNavMeshObstacle obstacle;
    NavMeshObstacle navMeshObstacle;

    SerializedProperty m_Shape;
    SerializedProperty m_Center;
    SerializedProperty m_Size;
    SerializedProperty m_Carve;
    SerializedProperty m_MoveThreshold;
    SerializedProperty m_TimeToStationary;
    SerializedProperty m_CarveOnlyStationary;

    private void OnEnable()
    {
        obstacle = target as CustomNavMeshObstacle;
        navMeshObstacle = obstacle.GetComponent<NavMeshObstacle>();
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

    private void OnDisable()
    {
        if (target == null && navMeshObstacle != null) // if component/gameObject was destroyed
        {
            DestroyImmediate(navMeshObstacle);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Shape);
        if (EditorGUI.EndChangeCheck()) // if shape changed
        {
            Undo.RecordObject(target, "Changed Obstacle Shape");
            Undo.RecordObject(navMeshObstacle, "");

            obstacle.Shape = (NavMeshObstacleShape) m_Shape.enumValueIndex;
            obstacle.Center = Vector3.zero;
            if (m_Shape.enumValueIndex == 0) // capsule
            {
                obstacle.Size = new Vector3(1.0f, 2.0f, 1.0f);
            }
            else if (m_Shape.enumValueIndex == 1) // box
            {
                obstacle.Size = Vector3.one;
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Center);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Obstacle Center");
            Undo.RecordObject(navMeshObstacle, "");

            obstacle.Center = m_Center.vector3Value;

            serializedObject.ApplyModifiedProperties();
        }

        if (m_Shape.enumValueIndex == 0)
        {
            EditorGUI.BeginChangeCheck();
            float radius = EditorGUILayout.FloatField("Radius", m_Size.vector3Value.x / 2.0f);
            float height = EditorGUILayout.FloatField("Height", m_Size.vector3Value.y);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Obstacle Size");
                Undo.RecordObject(navMeshObstacle, "");

                obstacle.Size = new Vector3(radius * 2.0f, height, radius * 2.0f);

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
                Undo.RecordObject(target, "Changed Obstacle Size");
                Undo.RecordObject(navMeshObstacle, "");

                obstacle.Size = size;

                serializedObject.ApplyModifiedProperties();
            }
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Carve);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Obstacle Carving");
            Undo.RecordObject(navMeshObstacle, "");

            obstacle.Carving = m_Carve.boolValue;

            serializedObject.ApplyModifiedProperties();
        }

        if (m_Carve.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MoveThreshold);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Obstacle Carving Move Threshold");
                Undo.RecordObject(navMeshObstacle, "");

                obstacle.CarvingMoveThreshold = m_MoveThreshold.floatValue;

                serializedObject.ApplyModifiedProperties();
            }


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_TimeToStationary);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Obstacle Carving Time To Stationary");
                Undo.RecordObject(navMeshObstacle, "");

                obstacle.CarvingTimeToStationary = m_TimeToStationary.floatValue;

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_CarveOnlyStationary);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Obstacle Carving Only Stationary");
                Undo.RecordObject(navMeshObstacle, "");

                obstacle.CarveOnlyStationary = m_CarveOnlyStationary.boolValue;

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel--;
        }
    }
}
