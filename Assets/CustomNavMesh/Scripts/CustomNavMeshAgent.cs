﻿using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Custom navigation mesh agent.
/// </summary>
[DisallowMultipleComponent]
public class CustomNavMeshAgent : CustomMonoBehaviour
{
    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent is changed.
    /// </summary>
    public delegate void OnChange();
    /// <summary>
    /// Subscribe a function to be called after the agent is changed.
    /// </summary>
    public event OnChange onChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's radius or height is changed.
    /// </summary>
    public delegate void OnSizeChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's radius or height is changed.
    /// </summary>
    public event OnSizeChange onSizeChange;

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the agent's transform is changed.
    /// </summary>
    public delegate void OnTransformChange();
    /// <summary>
    /// Subscribe a function to be called after the agent's transform is changed.
    /// </summary>
    public event OnSizeChange onTransformChange;

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
        set { m_Radius = value; NavMeshAgent.radius = value; onChange?.Invoke(); onSizeChange?.Invoke(); }
    }

    [SerializeField] float m_Height = 2.0f;
    /// <summary>
    /// The height of the agent for purposes of passing under obstacles, etc.
    /// </summary>
    public float Height
    {
        get { return m_Height; }
        set { m_Height = value; NavMeshAgent.height = value; onChange?.Invoke(); onSizeChange?.Invoke(); }
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

    [SerializeField] float m_BaseOffset;
    /// <summary>
    /// The relative vertical displacement of the owning GameObject.
    /// </summary>
    public float BaseOffset
    {
        get { return m_BaseOffset; }
        set { m_BaseOffset = value; NavMeshAgent.baseOffset = value; onChange?.Invoke(); }
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

    HiddenNavMeshAgent hiddenAgent;
    HiddenNavMeshAgent HiddenAgent
    {
        get
        {
            if (hiddenAgent == null)
            {
                CustomNavMesh.TryGetHiddenAgent(this, out hiddenAgent);
            }
            return hiddenAgent;
        }
        set
        {
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

    // hide Update method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomUpdate is only called in Play mode
    new void Update()
    {
        if (transform.hasChanged)
        {
            onTransformChange?.Invoke();
            transform.hasChanged = false;
        }
    }

    protected override void OnCustomEnable()
    {
        // calling the NavMeshObstacle property will add an obstacle if gameObject doesn't have it
        // the custom inspector will only hide after a split second so update the flags now 
        NavMeshAgent.hideFlags = HideFlags.HideInInspector;
        NavMeshAgent.enabled = true;

        TryCreatingHiddenAgent();
    }

    protected override void OnCustomDisable()
    {
        NavMeshAgent.enabled = false;

        TryDestroyingHiddenAgent();
    }

    protected override void OnCustomDestroy()
    {
        if (gameObject.activeInHierarchy) // used to avoid destroying things twice, when gameObject is destroyed
        {
            Undo.DestroyObjectImmediate(NavMeshAgent);
        }
    }

    void TryCreatingHiddenAgent()
    {
        if (HiddenAgent == null)
        {
            var hiddenObject = new GameObject("(Hidden) " + name);
            hiddenObject.hideFlags = HideFlags.NotEditable;
            hiddenObject.transform.SetParent(transform.parent);
            hiddenObject.transform.position = transform.position;
            hiddenObject.transform.rotation = transform.rotation;
            hiddenObject.transform.localScale = transform.localScale;

#if UNITY_EDITOR
            var staticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            GameObjectUtility.SetStaticEditorFlags(hiddenObject, staticFlags);
#else
        hiddenObject.isStatic = gameObject.isStatic;
#endif

            HiddenAgent = hiddenObject.AddComponent<HiddenNavMeshAgent>();
            // CustomNavMesh.RegisterCustomAgent(this, HiddenAgent);
        }
    }

    void TryDestroyingHiddenAgent()
    {
        if (HiddenAgent != null)
        {
            DestroyImmediate(HiddenAgent.gameObject);
        }
    }
}