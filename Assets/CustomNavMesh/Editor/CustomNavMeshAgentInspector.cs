using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshAgent))]
public class CustomNavMeshAgentInspector : Editor
{
    static readonly GUIContent s_Text = new GUIContent();

    SerializedProperty m_AgentTypeID;
    SerializedProperty m_Radius;
    SerializedProperty m_Height;
    SerializedProperty m_WalkableMask;
    SerializedProperty m_Speed;
    SerializedProperty m_Acceleration;
    SerializedProperty m_AngularSpeed;
    SerializedProperty m_StoppingDistance;
    SerializedProperty m_AutoTraverseOffMeshLink;
    SerializedProperty m_AutoBraking;
    SerializedProperty m_AutoRepath;
    SerializedProperty m_BaseOffset;
    SerializedProperty m_ObstacleAvoidanceType;
    SerializedProperty m_AvoidancePriority;

    SerializedProperty m_BlockAfterDuration;
    SerializedProperty m_TimeToBlock;
    SerializedProperty m_BlockSpeedThreshold;
    SerializedProperty m_UnblockAfterDuration;
    SerializedProperty m_TimeToUnblock;
    SerializedProperty m_DistanceReductionThreshold;
    SerializedProperty m_UnblockAtSpeed;
    SerializedProperty m_UnblockSpeedThreshold;
    SerializedProperty m_CarvingMoveThreshold;
    SerializedProperty m_TimeToStationary;
    SerializedProperty m_CarveOnlyStationary;

    CustomNavMeshAgent[] Agents
    {
        get
        {
            return Array.ConvertAll(targets, obj => (CustomNavMeshAgent) obj);
        }
    }

    private class Styles
    {
        public readonly GUIContent m_AgentSteeringHeader = EditorGUIUtility.TrTextContent("Steering");
        public readonly GUIContent m_AgentAvoidanceHeader = EditorGUIUtility.TrTextContent("Obstacle Avoidance");
        public readonly GUIContent m_AgentPathFindingHeader = EditorGUIUtility.TrTextContent("Path Finding");
        public readonly GUIContent m_AgentBlockingHeader = EditorGUIUtility.TrTextContent("Blocking");
        public readonly GUIContent m_AgentUnblockingHeader = EditorGUIUtility.TrTextContent("Unblocking");
    }

    static Styles s_Styles;

    private void OnEnable()
    {
        var agent = target as CustomNavMeshAgent;
        var navMeshAgent = agent.GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            // prevent the user from changing the agent through the NavMeshAgent's inspector
            navMeshAgent.hideFlags = HideFlags.HideInInspector;
        }

        m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
        m_Radius = serializedObject.FindProperty("m_Radius");
        m_Height = serializedObject.FindProperty("m_Height");
        m_WalkableMask = serializedObject.FindProperty("m_WalkableMask");
        m_Speed = serializedObject.FindProperty("m_Speed");
        m_Acceleration = serializedObject.FindProperty("m_Acceleration");
        m_AngularSpeed = serializedObject.FindProperty("m_AngularSpeed");
        m_StoppingDistance = serializedObject.FindProperty("m_StoppingDistance");
        m_AutoTraverseOffMeshLink = serializedObject.FindProperty("m_AutoTraverseOffMeshLink");
        m_AutoBraking = serializedObject.FindProperty("m_AutoBraking");
        m_AutoRepath = serializedObject.FindProperty("m_AutoRepath");
        m_BaseOffset = serializedObject.FindProperty("m_BaseOffset");
        m_ObstacleAvoidanceType = serializedObject.FindProperty("m_ObstacleAvoidanceType");
        m_AvoidancePriority = serializedObject.FindProperty("m_AvoidancePriority");

        m_BlockAfterDuration = serializedObject.FindProperty("m_BlockAfterDuration");
        m_TimeToBlock = serializedObject.FindProperty("m_TimeToBlock");
        m_BlockSpeedThreshold = serializedObject.FindProperty("m_BlockSpeedThreshold");
        m_UnblockAfterDuration = serializedObject.FindProperty("m_UnblockAfterDuration");
        m_TimeToUnblock = serializedObject.FindProperty("m_TimeToUnblock");
        m_DistanceReductionThreshold = serializedObject.FindProperty("m_DistanceReductionThreshold");
        m_UnblockAtSpeed = serializedObject.FindProperty("m_UnblockAtSpeed");
        m_UnblockSpeedThreshold = serializedObject.FindProperty("m_UnblockSpeedThreshold");
        m_CarvingMoveThreshold = serializedObject.FindProperty("m_CarvingMoveThreshold");
        m_TimeToStationary = serializedObject.FindProperty("m_TimeToStationary");
        m_CarveOnlyStationary = serializedObject.FindProperty("m_CarveOnlyStationary");
    }

    public override void OnInspectorGUI()
    {
        if (s_Styles == null)
            s_Styles = new Styles();

        serializedObject.Update();

        AgentTypePopupInternal("Agent Type", m_AgentTypeID);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_BaseOffset);
        if(EditorGUI.EndChangeCheck())
        {
            foreach(var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Base Offset");
                agent.RecordNavMeshAgent();
                agent.BaseOffset = m_BaseOffset.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(s_Styles.m_AgentSteeringHeader, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Speed);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_Speed.floatValue < 0.0f) m_Speed.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Speed");
                agent.RecordNavMeshAgent();
                agent.Speed = m_Speed.floatValue;
            }           

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_AngularSpeed);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_AngularSpeed.floatValue < 0.0f) m_AngularSpeed.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Angular Speed");
                agent.RecordNavMeshAgent();
                agent.AngularSpeed = m_AngularSpeed.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Acceleration);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_Acceleration.floatValue < 0.0f) m_Acceleration.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Acceleration");
                agent.RecordNavMeshAgent();
                agent.Acceleration = m_Acceleration.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_StoppingDistance);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_StoppingDistance.floatValue < 0.0f) m_StoppingDistance.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Stopping Distance");
                agent.RecordNavMeshAgent();
                agent.StoppingDistance = m_StoppingDistance.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_AutoBraking);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Auto Braking");
                agent.RecordNavMeshAgent();
                agent.AutoBraking = m_AutoBraking.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(s_Styles.m_AgentAvoidanceHeader, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Radius);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_Radius.floatValue <= 0.0f) m_Radius.floatValue = 1e-05f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Radius");
                agent.RecordNavMeshAgent();
                agent.Radius = m_Radius.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Height);
        if (EditorGUI.EndChangeCheck())
        {
            if (m_Height.floatValue <= 0.0f) m_Height.floatValue = 1e-05f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Height");
                agent.RecordNavMeshAgent();
                agent.Height = m_Height.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_ObstacleAvoidanceType, Temp("Quality"));
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Obstacle Avoidance Type");
                agent.RecordNavMeshAgent();
                agent.ObstacleAvoidanceType = (ObstacleAvoidanceType)m_ObstacleAvoidanceType.enumValueIndex;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_AvoidancePriority, Temp("Priority"));
        if (EditorGUI.EndChangeCheck())
        {
            if (m_AvoidancePriority.intValue < 0) m_AvoidancePriority.intValue = 0;
            if (m_AvoidancePriority.intValue > 99) m_AvoidancePriority.intValue = 99;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Avoidance Priority");
                agent.RecordNavMeshAgent();
                agent.AvoidancePriority = m_AvoidancePriority.intValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(s_Styles.m_AgentPathFindingHeader, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_AutoTraverseOffMeshLink);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Auto Traverse Off Mesh Link");
                agent.RecordNavMeshAgent();
                agent.AutoTraverseOffMeshLink = m_AutoTraverseOffMeshLink.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_AutoRepath);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Auto Repath");
                agent.RecordNavMeshAgent();
                agent.AutoRepath = m_AutoRepath.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        //Initially needed data
        var areaNames = GameObjectUtility.GetNavMeshAreaNames();
        var currentMask = m_WalkableMask.longValue;
        var compressedMask = 0;

        //Need to find the index as the list of names will compress out empty areas
        for (var i = 0; i < areaNames.Length; i++)
        {
            var areaIndex = GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]);
            if (((1 << areaIndex) & currentMask) != 0)
                compressedMask = compressedMask | (1 << i);
        }

        //Refactor this to use the mask field that takes a label.
        float kSingleLineHeight = 18f;
        float kSpacing = 5;
        float kLabelFloatMinW = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + kSpacing;
        float kLabelFloatMaxW = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + kSpacing;

        float kH = kSingleLineHeight;
        var position = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, kH, kH, EditorStyles.layerMaskField);

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = m_WalkableMask.hasMultipleDifferentValues;
        var areaMask = EditorGUI.MaskField(position, "Area Mask", compressedMask, areaNames, EditorStyles.layerMaskField);
        EditorGUI.showMixedValue = false;

        if (EditorGUI.EndChangeCheck())
        {
            if (areaMask == ~0)
            {
                m_WalkableMask.longValue = 0xffffffff;
            }
            else
            {
                uint newMask = 0;
                for (var i = 0; i < areaNames.Length; i++)
                {
                    //If the bit has been set in the compacted mask
                    if (((areaMask >> i) & 1) != 0)
                    {
                        //Find out the 'real' layer from the name, then set it in the new mask
                        newMask = newMask | (uint)(1 << GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]));
                    }
                }
                m_WalkableMask.longValue = newMask;
            }

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Area Mask");
                agent.RecordNavMeshAgent();
                agent.AreaMask = m_WalkableMask.intValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(s_Styles.m_AgentBlockingHeader, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_BlockAfterDuration,
            new GUIContent("Block after Duration",
            "Should the hidden agent switch from agent to obstacle if it hasn't surpassed the " +
            "BlockSpeedTreshold for TimeToBlock seconds?")
            );
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Block After Duration");
                agent.BlockAfterDuration = m_BlockAfterDuration.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        bool guiEnabled = GUI.enabled;
        GUI.enabled = m_BlockAfterDuration.boolValue;
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_TimeToBlock, 
            new GUIContent("Time to Block", 
            "Time in seconds needed for the hidden agent to switch from agent to obstacle, " +
            "assuming it hasn't surpassed the BlockSpeedTreshold during the interval.")
            );

        if (EditorGUI.EndChangeCheck())
        {
            if (m_TimeToBlock.floatValue < 0.0f) m_TimeToBlock.floatValue = 0.0f;
            
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Time To Block");
                agent.TimeToBlock = m_TimeToBlock.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_BlockSpeedThreshold,
            new GUIContent("Block Speed Threshold",
            "Speed the agent can't surpass for TimeToBlock seconds for the hidden " +
            "agent to turn into an obstacle.")
            );
        if (EditorGUI.EndChangeCheck())
        {
            if (m_BlockSpeedThreshold.floatValue < 0.0f) m_BlockSpeedThreshold.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Block Speed Threshold");
                agent.BlockSpeedThreshold = m_BlockSpeedThreshold.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = guiEnabled;
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Obstacle Carving");
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_CarvingMoveThreshold,
            new GUIContent("Carving Move Threshold",
            "This refers to the hidden agent's obstacle when blocking. " +
            "Threshold distance for updating a moving carved hole.")
            );
        if (EditorGUI.EndChangeCheck())
        {
            if (m_CarvingMoveThreshold.floatValue < 0.0f) m_CarvingMoveThreshold.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Carving Move Threshold");
                agent.CarvingMoveThreshold = m_CarvingMoveThreshold.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_TimeToStationary,
            new GUIContent("Time To Stationary",
            "This refers to the hidden agent's obstacle when blocking. " +
            "Time to wait until the obstacle is treated as stationary (when CarveOnlyStationary is enabled).")
            );
        if (EditorGUI.EndChangeCheck())
        {
            if (m_TimeToStationary.floatValue < 0.0f) m_TimeToStationary.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Carving Time To Stationary");
                agent.CarvingTimeToStationary = m_TimeToStationary.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_CarveOnlyStationary,
            new GUIContent("Carve Only Stationary",
            "This refers to the hidden agent's obstacle when blocking. " +
            "Should the obstacle be carved when it is constantly moving?")
            );
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Carve Only Stationary");
                agent.CarveOnlyStationary = m_CarveOnlyStationary.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(s_Styles.m_AgentUnblockingHeader, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_UnblockAfterDuration,
            new GUIContent("Unblock after Duration",
            "Should the hidden agent try to find a new path every TimeToUnblock seconds, and switch from obstacle to agent " +
            "when it finds one which reduces its distance to the destination by DistanceReductionThreshold?")
            );
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Unblock After Duration");
                agent.UnblockAfterDuration = m_UnblockAfterDuration.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = m_UnblockAfterDuration.boolValue;
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_TimeToUnblock,
            new GUIContent("Time to Unblock",
            "Time in seconds needed for the hidden agent to check if it should change to " +
            "agent again, assuming it is currently in obstacle mode.")
            );

        if (EditorGUI.EndChangeCheck())
        {
            if (m_TimeToUnblock.floatValue < 0.0f) m_TimeToUnblock.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Time To Unblock");
                agent.TimeToUnblock = m_TimeToUnblock.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_DistanceReductionThreshold,
            new GUIContent("Distance Reduction Threshold",
            "When the agent checks if it should stop blocking, this is the minimum distance the newly calculated reacheable position " +
            "must be closer to the destination for it to change into an agent again.")
            );

        if (EditorGUI.EndChangeCheck())
        {
            if (m_DistanceReductionThreshold.floatValue < 0.0f) m_DistanceReductionThreshold.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Distance Reduction Threshold");
                agent.DistanceReductionThreshold = m_DistanceReductionThreshold.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = guiEnabled;
        EditorGUI.indentLevel--;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_UnblockAtSpeed,
            new GUIContent("Unblock At Speed",
            "Should the hidden agent switch from obstacle to agent if it surpasses the UnblockSpeedTreshold?")
            );
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Unblock At Speed");
                agent.UnblockAtSpeed = m_UnblockAtSpeed.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = m_UnblockAtSpeed.boolValue;
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(
            m_UnblockSpeedThreshold,
            new GUIContent("Unblock Speed Threshold",
            "Speed at which the hidden agent turns into an agent again " +
            "if it is currently in obstacle mode.")
            );
        if (EditorGUI.EndChangeCheck())
        {
            if (m_UnblockSpeedThreshold.floatValue < 0.0f) m_UnblockSpeedThreshold.floatValue = 0.0f;

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Unblock Speed Threshold");
                agent.UnblockSpeedThreshold = m_UnblockSpeedThreshold.floatValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = guiEnabled;
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    private void AgentTypePopupInternal(string labelName, SerializedProperty agentTypeID)
    {
        var index = -1;
        var count = NavMesh.GetSettingsCount();
        var agentTypeNames = new string[count + 2];
        for (var i = 0; i < count; i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            var name = NavMesh.GetSettingsNameFromID(id);
            agentTypeNames[i] = name;
            if (id == agentTypeID.intValue)
                index = i;
        }
        agentTypeNames[count] = "";
        agentTypeNames[count + 1] = "Open Agent Settings...";

        bool validAgentType = index != -1;
        if (!validAgentType)
        {
            EditorGUILayout.HelpBox("Agent Type invalid.", MessageType.Warning);
        }

        var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

        EditorGUI.BeginChangeCheck();
        index = EditorGUI.Popup(rect, labelName, index, agentTypeNames);
        if (EditorGUI.EndChangeCheck())
        {
            if (index >= 0 && index < count)
            {
                var id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                agentTypeID.intValue = id;
            }
            else if (index == count + 1)
            {
                UnityEditor.AI.NavMeshEditorHelpers.OpenAgentSettings(-1);
            }

            foreach (var agent in Agents)
            {
                Undo.RecordObject(agent, "Changed Agent Type");
                agent.RecordNavMeshAgent();
                agent.AgentTypeID = agentTypeID.intValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }

    static GUIContent Temp(string t)
    {
        s_Text.text = t;
        s_Text.tooltip = string.Empty;
        return s_Text;
    }
}
