﻿using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Custom navigation mesh agent.
/// </summary>
[DisallowMultipleComponent]
public class CustomNavMeshAgent : CustomMonoBehaviour
{
    Transform savedParent;
    Vector3 savedPosition;
    Vector3 savedRotation;
    Vector3 savedScale;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent is changed.
    /// </summary>
    public delegate void OnChange();
    /// <summary>
    /// Subscribe a function to be called after the agent is changed.
    /// </summary>
    public event OnChange onChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's radius, height or base offset is changed.
    /// </summary>
    public delegate void OnAgentMeshChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's radius, height or base offset is changed. Note: always invoked after onChange.
    /// </summary>
    public event OnAgentMeshChange onAgentMeshChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's position is changed.
    /// </summary>
    public delegate void OnAgentPositionChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's position is changed. Note: always invoked after onChange.
    /// </summary>
    public event OnAgentPositionChange onAgentPositionChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's parent is changed.
    /// </summary>
    public delegate void OnParentChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's parent is changed.
    /// </summary>
    public event OnParentChange onParentChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's local position is changed.
    /// </summary>
    public delegate void OnPositionChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's local position is changed.
    /// </summary>
    public event OnPositionChange onPositionChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's local rotation is changed.
    /// </summary>
    public delegate void OnRotationChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's local rotation is changed.
    /// </summary>
    public event OnRotationChange onRotationChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's local scale is changed.
    /// </summary>
    public delegate void OnScaleChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's local scale is changed.
    /// </summary>
    public event OnScaleChange onScaleChange;

    [SerializeField] int m_AgentTypeID;
    /// <summary>
    /// The type ID for the agent.
    /// </summary>
    public int AgentTypeID
    {
        get { return m_AgentTypeID; }
        set { m_AgentTypeID = value; NavMeshAgent.agentTypeID = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_Radius = 0.5f;
    /// <summary>
    /// The avoidance radius for the agent.
    /// </summary>
    public float Radius
    {
        get { return m_Radius; }
        set { m_Radius = value; NavMeshAgent.radius = value; onChange?.Invoke(); onAgentMeshChange?.Invoke(); }
    }

    [SerializeField] float m_Height = 2.0f;
    /// <summary>
    /// The height of the agent for purposes of passing under obstacles, etc.
    /// </summary>
    public float Height
    {
        get { return m_Height; }
        set
        {
            m_Height = value; NavMeshAgent.height = value;
            onChange?.Invoke(); onAgentMeshChange?.Invoke(); onAgentPositionChange?.Invoke();
        }
    }

    [SerializeField] int m_WalkableMask = -1;
    /// <summary>
    /// Specifies which NavMesh areas are passable. Changing areaMask will make the path stale (see isPathStale).
    /// </summary>
    public int AreaMask
    {
        get { return m_WalkableMask; }
        set { m_WalkableMask = value; NavMeshAgent.areaMask = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_Speed = 3.5f;
    /// <summary>
    /// Maximum movement speed when following a path.
    /// </summary>
    public float Speed
    {
        get { return m_Speed; }
        set { m_Speed = value; NavMeshAgent.speed = value; onChange?.Invoke(); }
    }

    public Vector3 Velocity
    {
        get { return HiddenAgent ? HiddenAgent.Velocity : Vector3.zero; }
        set { if (HiddenAgent) HiddenAgent.Velocity = value; }
    }

    [SerializeField] float m_Acceleration = 8.0f;
    /// <summary>
    /// The maximum acceleration of an agent as it follows a path, given in units / sec^2.
    /// </summary>
    public float Acceleration
    {
        get { return m_Acceleration; }
        set { m_Acceleration = value; NavMeshAgent.acceleration = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_AngularSpeed = 120.0f;
    /// <summary>
    /// Maximum turning speed in (deg/s) while following a path.
    /// </summary>
    public float AngularSpeed
    {
        get { return m_AngularSpeed; }
        set { m_AngularSpeed = value; NavMeshAgent.angularSpeed = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_StoppingDistance;
    /// <summary>
    /// Stop within this distance from the target position.
    /// </summary>
    public float StoppingDistance
    {
        get { return m_StoppingDistance; }
        set { m_StoppingDistance = value; NavMeshAgent.stoppingDistance = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_AutoTraverseOffMeshLink = true;
    /// <summary>
    /// Should the agent move across OffMeshLinks automatically?
    /// </summary>
    public bool AutoTraverseOffMeshLink
    {
        get { return m_AutoTraverseOffMeshLink; }
        set { m_AutoTraverseOffMeshLink = value; NavMeshAgent.autoTraverseOffMeshLink = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_AutoBraking = true;
    /// <summary>
    /// Should the agent brake automatically to avoid overshooting the destination point?
    /// </summary>
    public bool AutoBraking
    {
        get { return m_AutoBraking; }
        set { m_AutoBraking = value; NavMeshAgent.autoBraking = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_AutoRepath = true;
    /// <summary>
    /// Should the agent attempt to acquire a new path if the existing path becomes invalid?
    /// </summary>
    public bool AutoRepath
    {
        get { return m_AutoRepath; }
        set { m_AutoRepath = value; NavMeshAgent.autoRepath = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_BaseOffset = 1.0f;
    /// <summary>
    /// The relative vertical displacement of the owning GameObject.
    /// </summary>
    public float BaseOffset
    {
        get { return m_BaseOffset; }
        set { m_BaseOffset = value; NavMeshAgent.baseOffset = value; onChange?.Invoke(); onAgentMeshChange?.Invoke(); }
    }

    [SerializeField] ObstacleAvoidanceType m_ObstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    /// <summary>
    /// The level of quality of avoidance.
    /// </summary>
    public ObstacleAvoidanceType ObstacleAvoidanceType
    {
        get { return m_ObstacleAvoidanceType; }
        set { m_ObstacleAvoidanceType = value; NavMeshAgent.obstacleAvoidanceType = value; onChange?.Invoke(); }
    }

    [SerializeField] int m_AvoidancePriority = 50;
    /// <summary>
    /// The avoidance priority level.
    /// </summary>
    public int AvoidancePriority
    {
        get { return m_AvoidancePriority; }
        set { m_AvoidancePriority = value; NavMeshAgent.avoidancePriority = value; onChange?.Invoke(); }
    }

    /// <summary>
    /// Should the agent update the transform orientation?
    /// </summary>
    public bool UpdateRotation
    {
        get { return NavMeshAgent.updateRotation; }
        set { NavMeshAgent.updateRotation = value; }
    }

    [SerializeField] bool m_BlockAfterDuration = true;
    /// <summary>
    /// Should the hidden agent switch from agent to obstacle if it hasn't surpassed 
    /// the BlockSpeedTreshold for TimeToBlock seconds?
    /// </summary>
    public bool BlockAfterDuration
    {
        get { return m_BlockAfterDuration; }
        set { m_BlockAfterDuration = value; }
    }

    [SerializeField] float m_TimeToBlock = 1.0f;
    /// <summary>
    /// Time in seconds needed for the hidden agent to switch from agent to obstacle, 
    /// assuming it hasn't surpassed the BlockSpeedTreshold during the interval.
    /// </summary>
    public float TimeToBlock
    {
        get { return m_TimeToBlock; }
        set { m_TimeToBlock = value; }
    }

    [SerializeField] float m_BlockSpeedThreshold = 1.0f;
    /// <summary>
    /// Speed the agent can't surpass for TimeToBlock seconds for the hidden agent 
    /// to turn into an obstacle.
    /// </summary>
    public float BlockSpeedThreshold
    {
        get { return m_BlockSpeedThreshold; }
        set { m_BlockSpeedThreshold = value; }
    }

    [SerializeField] bool m_UnblockAfterDuration = true;
    /// <summary>
    /// Should the hidden agent try to find a new path every TimeToUnblock seconds, and switch from obstacle to agent 
    /// when it finds a path which reduces its distance to the destination by DistanceReductionThreshold?
    /// </summary>
    public bool UnblockAfterDuration
    {
        get { return m_UnblockAfterDuration; }
        set { m_UnblockAfterDuration = value; }
    }

    [SerializeField] float m_TimeToUnblock = 1.0f;
    /// <summary>
    /// Time in seconds needed for the hidden agent to check if it should change to agent again, 
    /// assuming it is currently in obstacle mode.
    /// </summary>
    public float TimeToUnblock
    {
        get { return m_TimeToUnblock; }
        set { m_TimeToUnblock = value; }
    }

    [SerializeField] float m_DistanceReductionThreshold = 0.5f;
    /// <summary>
    /// When the agent checks if it should stop blocking, this is the minimum distance the newly calculated reacheable position 
    /// must be closer to the destination (comparing with the current position) for it to change into an agent again.
    /// </summary>
    public float DistanceReductionThreshold
    {
        get { return m_DistanceReductionThreshold; }
        set { m_DistanceReductionThreshold = value; }
    }

    [SerializeField] bool m_UnblockAtSpeed = true;
    /// <summary>
    /// Should the hidden agent switch from obstacle to agent if it surpasses 
    /// the UnblockSpeedTreshold?
    /// </summary>
    public bool UnblockAtSpeed
    {
        get { return m_UnblockAtSpeed; }
        set { m_UnblockAtSpeed = value; }
    }

    [SerializeField] float m_UnblockSpeedThreshold = 1.0f;
    /// <summary>
    /// Speed at which the hidden agent turns into an agent again if it is currently 
    /// in obstacle mode.
    /// </summary>
    public float UnblockSpeedThreshold
    {
        get { return m_UnblockSpeedThreshold; }
        set { m_UnblockSpeedThreshold = value; }
    }

    [SerializeField] float m_CarvingMoveThreshold = 0.1f;
    /// <summary>
    /// This refers to the hidden agent's obstacle when blocking. Threshold distance 
    /// for updating a moving carved hole.
    /// </summary>
    public float CarvingMoveThreshold
    {
        get { return m_CarvingMoveThreshold; }
        set { m_CarvingMoveThreshold = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_TimeToStationary = 0.5f;
    /// <summary>
    /// This refers to the hidden agent's obstacle when blocking. Time to wait until 
    /// the obstacle is treated as stationary (when CarveOnlyStationary is enabled).
    /// </summary>
    public float CarvingTimeToStationary
    {
        get { return m_TimeToStationary; }
        set { m_TimeToStationary = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_CarveOnlyStationary = true;
    /// <summary>
    /// This refers to the hidden agent's obstacle when blocking. Should the obstacle be carved 
    /// when it is constantly moving?
    /// </summary>
    public bool CarveOnlyStationary
    {
        get { return m_CarveOnlyStationary; }
        set { m_CarveOnlyStationary = value; onChange?.Invoke(); }
    }

    // Why serialize the game object? Because the HiddenNavMeshAgent would reset to null.
    [SerializeField, HideInInspector] GameObject hiddenAgentGameObject;
    HiddenNavMeshAgent hiddenAgent;
    HiddenNavMeshAgent HiddenAgent
    {
        get
        {
            if (hiddenAgent == null && hiddenAgentGameObject != null)
            {
                hiddenAgent = hiddenAgentGameObject.GetComponent<HiddenNavMeshAgent>();
            }
            return hiddenAgent;
        }
        set
        {
            hiddenAgentGameObject = (value != null) ? value.gameObject : null;
            hiddenAgent = value;
        }
    }

    NavMeshAgent navMeshAgent;
    NavMeshAgent NavMeshAgent
    {
        get
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
                if (navMeshAgent == null)
                {
                    navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
                }
                else
                {
                    // update existing nav mesh agent
                    TransferAgentValues(this, navMeshAgent);
                }
            }
            return navMeshAgent;
        }
    }

    /// <summary>
    /// Apply relative movement to current position.
    /// If the agent has a path it will be adjusted.
    /// </summary>
    /// <param name="offset">The relative movement vector.</param>
    public void Move(Vector3 offset)
    {
        if (NavMeshAgent.enabled) NavMeshAgent.Move(offset);

        // update hidden position now (instead of waiting for the next frame)
        // so the agent's path can get recalculated sooner
        if (HiddenAgent) HiddenAgent.transform.position = transform.position + CustomNavMesh.HiddenTranslation;
    }

    /// <summary>
    /// Clears the current path. When the path is cleared, the agent will 
    /// not start looking for a new path until SetDestination is called.
    /// </summary>
    public void ResetPath()
    {
        if(HiddenAgent) HiddenAgent.ResetPath();
    }

    /// <summary>
    /// Warps agent to the provided position. If the agent has a path it will not be reset.
    /// </summary>
    /// <param name="newPosition">New position to warp the agent to.</param>
    /// <returns>True if agent is successfully warped, otherwise false.</returns>
    public bool Warp(Vector3 newPosition)
    {
        bool result = NavMeshAgent.Warp(newPosition);

        // update hidden position now (instead of waiting for the next frame)
        // so the agent's path can get recalculated sooner
        if (HiddenAgent) HiddenAgent.transform.position = transform.position + CustomNavMesh.HiddenTranslation;

        return result;
    }

    /// <summary>
    /// Sets or updates the destination thus triggering the calculation for a new path.
    /// </summary>
    /// <param name="target">The target point to navigate to.</param>
    /// <returns>True if the destination was requested successfully, otherwise false.</returns>
    public bool SetDestination(Vector3 target)
    {
        if (!HiddenAgent) return false;
        return HiddenAgent.SetDestination(target + CustomNavMesh.HiddenTranslation);
    }

    /// <summary>
    /// Transfers the parameters of a CustomNavMeshAgent to a NavMeshAgent.
    /// </summary>
    /// <param name="sourceAgent">The CustomNavMeshAgent to copy</param>
    /// <param name="destAgent">The destination NavMeshAgent</param>
    public static void TransferAgentValues(CustomNavMeshAgent sourceAgent, NavMeshAgent destAgent)
    {
        destAgent.agentTypeID = sourceAgent.AgentTypeID;
        destAgent.radius = sourceAgent.Radius;
        destAgent.height = sourceAgent.Height;
        destAgent.areaMask = sourceAgent.AreaMask;
        destAgent.speed = sourceAgent.Speed;
        destAgent.acceleration = sourceAgent.Acceleration;
        destAgent.angularSpeed = sourceAgent.AngularSpeed;
        destAgent.stoppingDistance = sourceAgent.StoppingDistance;
        destAgent.autoTraverseOffMeshLink = sourceAgent.AutoTraverseOffMeshLink;
        destAgent.autoBraking = sourceAgent.AutoBraking;
        destAgent.autoRepath = sourceAgent.AutoRepath;
        destAgent.baseOffset = sourceAgent.BaseOffset;
        destAgent.obstacleAvoidanceType = sourceAgent.ObstacleAvoidanceType;
        destAgent.avoidancePriority = sourceAgent.AvoidancePriority;
    }

    /// <summary>
    /// Records any changes done on the NavMeshAgent after this call.
    /// </summary>
    public void RecordNavMeshAgent()
    {
#if UNITY_EDITOR
        Undo.RecordObject(NavMeshAgent, "");
#endif
    }

    // hide Start method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomStart is only called in Play mode
    new void Start()
    {
        savedPosition = transform.localPosition;
        savedRotation = transform.localRotation.eulerAngles;
        savedScale = transform.localScale;
    }

    // hide Update method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomUpdate is only called in Play mode
    new void Update()
    {
        if (transform.hasChanged)
        {
            if (savedPosition != transform.localPosition)
            {
                savedPosition = transform.localPosition;
                onPositionChange?.Invoke();
            }

            if (savedParent != transform.parent)
            {
                savedParent = transform.parent;
                onParentChange?.Invoke();
            }

            if (savedRotation != transform.localRotation.eulerAngles)
            {
                savedRotation = transform.localRotation.eulerAngles;
                onRotationChange?.Invoke();
            }

            if (savedScale != transform.localScale)
            {
                savedScale = transform.localScale;
                onScaleChange?.Invoke();
            }

            transform.hasChanged = false;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        if (HiddenAgent && Time.deltaTime != 0)
        {
            Vector3 desiredVelocity = Vector3.ClampMagnitude(HiddenAgent.Velocity, Speed);
            NavMeshAgent.Move(desiredVelocity * Time.deltaTime);
        }
    }

    protected override void OnCustomEnable()
    {
        // calling the NavMeshObstacle property will add an obstacle if gameObject doesn't have it
        // the custom inspector will only hide after a split second so update the flags now 
        NavMeshAgent.hideFlags = HideFlags.HideInInspector;
        NavMeshAgent.enabled = true;
        
        TryCreatingHiddenAgent();

        savedParent = transform.parent;

        // When leaving the prefab isolation mode, the agent should be disabled and
        // enabled so that the hidden agent can be spawned in the scene
        PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;

        void OnPrefabStageClosing(PrefabStage prefabStage)
        {
            GameObject sceneObject = prefabStage.openedFromInstanceObject;
            if (sceneObject != null)
            {
                sceneObject.SetActive(false);
                sceneObject.SetActive(true);
            }
        }
    }

    protected override void OnCustomDisable()
    {
        NavMeshAgent.enabled = false;

        TryDestroyingHiddenAgent();
    }

    protected override void OnCustomDestroy()
    {
        // in case OnDisable wasn't called (e.g. destroyed during the application's first frame)
        TryDestroyingHiddenAgent();

        if (gameObject.activeInHierarchy) // used to avoid destroying things twice, when gameObject is destroyed
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(NavMeshAgent);
#else
            DestroyImmediate(NavMeshAgent);
#endif
        }
    }

    void TryCreatingHiddenAgent()
    {
        // do not create the hidden agent in prefab mode
        if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;

        if (HiddenAgent == null || !HiddenAgent.IsLinkedWith(this))
        {
            var hiddenObject = new GameObject("(Hidden) " + name);
            hiddenObject.hideFlags = HideFlags.NotEditable;
            hiddenObject.transform.SetParent(transform.parent);

#if UNITY_EDITOR
            var staticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            GameObjectUtility.SetStaticEditorFlags(hiddenObject, staticFlags);
#else
            hiddenObject.isStatic = gameObject.isStatic;
#endif
            HiddenAgent = hiddenObject.AddComponent<HiddenNavMeshAgent>();

#if UNITY_EDITOR
            // set dirty; otherwise, if it's part of a prefab instance and scripts are 
            // reloaded, the hiddenAgentGameObject reference will be lost
            EditorUtility.SetDirty(this);
#endif
            HiddenAgent.LinkWithCustomAgent(this);
        }
    }

    void TryDestroyingHiddenAgent()
    {
        if (HiddenAgent != null)
        {
            if (Application.isPlaying)
            {
                Destroy(HiddenAgent.gameObject);
            }
            else
            {
                // avoid "Cannot destroy GameObject while it's parent is being activated or deactivated" when leaving scene
                if (SceneManager.GetActiveScene().isLoaded)
                {
                    DestroyImmediate(HiddenAgent.gameObject);
                }
            }
        }
    }
}
