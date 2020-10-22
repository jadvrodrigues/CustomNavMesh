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
            m_Center.vector3Value = Vector3.zero;
            if (m_Shape.enumValueIndex == 0) // capsule
            {
                m_Size.vector3Value = new Vector3(1.0f, 2.0f, 1.0f);
            }
            else if (m_Shape.enumValueIndex == 1) // box
            {
                m_Size.vector3Value = Vector3.one;
            }
            serializedObject.ApplyModifiedProperties();
            //(target as NavMeshObstacle).FitExtents();
            serializedObject.Update();
        }

        EditorGUILayout.PropertyField(m_Center);

        if (m_Shape.enumValueIndex == 0)
        {
            // NavMeshObstacleShape : kObstacleShapeCapsule
            EditorGUI.BeginChangeCheck();
            float radius = EditorGUILayout.FloatField("Radius", m_Size.vector3Value.x / 2.0f);
            float height = EditorGUILayout.FloatField("Height", m_Size.vector3Value.y);
            if (EditorGUI.EndChangeCheck())
            {
                m_Size.vector3Value = new Vector3(radius * 2.0f, height, radius * 2.0f);
            }
        }
        else if (m_Shape.enumValueIndex == 1)
        {
            // NavMeshObstacleShape : kObstacleShapeBox
            EditorGUI.BeginChangeCheck();
            Vector3 size = m_Size.vector3Value;
            size = EditorGUILayout.Vector3Field("Size", size);
            if (EditorGUI.EndChangeCheck())
            {
                m_Size.vector3Value = size;
            }
        }

        EditorGUILayout.PropertyField(m_Carve);

        if (m_Carve.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_MoveThreshold);
            EditorGUILayout.PropertyField(m_TimeToStationary);
            EditorGUILayout.PropertyField(m_CarveOnlyStationary);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
