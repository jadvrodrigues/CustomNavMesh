using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hidden navigation mesh agent.
/// </summary>
[DisallowMultipleComponent, AddComponentMenu("")] // remove from Add Component list
public class HiddenNavMeshAgent : CustomMonoBehaviour
{
    /// <summary>
    /// When trying to unblock, this multiplier is applied to the agent's radius and
    /// then the result is added to the surface's agent radius to get the maximum 
    /// sampling distance for the current position.
    /// </summary>
    const float posSamplingModifier = 2.0f;

    /// <summary>
    /// When trying to unblock, this multiplier is applied to the agent's radius and
    /// then the result is added to the surface's agent radius to get the maximum 
    /// sampling distance for the destination position.
    /// </summary>
    const float destSamplingModifier = 6.0f;

    bool subscribed; // used to avoid subscribing twice
    Vector3 lastPosition;

    Vector3? destination;
    float timer; // count the time since agent changed mode or block mode is refreshed

    /// <summary>
    /// Access the current velocity of the hidden agent component. Returns Vector3.zero 
    /// if it's currently in block mode or the agent is simply not moving. 
    /// </summary>
    public Vector3 Velocity
    {
        get
        {
            if (Agent.enabled && CustomAgent != null)
            {
                float magnitude = Agent.velocity.magnitude;
                if (magnitude <= CustomAgent.Speed)
                {
                    return Agent.velocity;
                }
                else
                {
                    return Agent.velocity / magnitude * CustomAgent.Speed;
                }
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    // Why serialize the game object? Because the CustomNavMeshAgent would reset to null.
    [SerializeField, HideInInspector] GameObject customAgentGameObject;
    CustomNavMeshAgent customAgent;
    CustomNavMeshAgent CustomAgent
    {
        get
        {
            if (customAgent == null && customAgentGameObject != null)
            {
                customAgent = customAgentGameObject.GetComponent<CustomNavMeshAgent>();
            }
            return customAgent;
        }
    }

    NavMeshAgent agent;
    NavMeshAgent Agent
    {
        get
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    agent = gameObject.AddComponent<NavMeshAgent>();
                }
                else
                {
                    UpdateAgent();
                }
            }
            return agent;
        }
    }

    NavMeshObstacle obstacle;
    NavMeshObstacle Obstacle
    {
        get
        {
            if (obstacle == null)
            {
                obstacle = GetComponent<NavMeshObstacle>();
                if (obstacle == null)
                {
                    obstacle = gameObject.AddComponent<NavMeshObstacle>();
                }
                else
                {
                    UpdateAgent();
                    UpdateMesh(); // Obstacle size is set in UpdateMesh
                }
            }
            return obstacle;
        }
    }

    bool isBlocking;
    bool IsBlocking
    {
        get { return isBlocking; }
        set
        {
            if (Application.isPlaying)
            {
                if (value)
                {
                    SwitchToObstacle();
                }
                else
                {
                    SwitchToAgent();

                    if (destination.HasValue)
                    {
                        agent.SetDestination(destination.Value);
                    }
                }
            }
        }
    }

    float surfaceAgentRadius = 0.0f;
    float SurfaceAgentRadius
    {
        get
        {
            if (surfaceAgentRadius == 0.0f)
            {
                surfaceAgentRadius = NavMesh.GetSettingsByID(CustomAgent.AgentTypeID).agentRadius;
            }
            return surfaceAgentRadius;
        }
    }

    public bool SetDestination(Vector3 target)
    {
        SwitchToAgent();

        destination = target;
        return agent.SetDestination(target);
    }

    /// <summary>
    /// Clears current path and sets destination to null, which means that this agent 
    /// will not start looking for a new path until SetDestination is called.
    /// </summary>
    public void ResetPath()
    {
        destination = null;
        if (Agent.enabled)
        {
            Agent.ResetPath();
        }
    }

    /// <summary>
    /// Link this agent to a custom agent and update everything. To be called from CustomNavMeshAgent 
    /// after this component is added. Note: in the agent and in the obstacle classes, this 
    /// initialization is done in OnCustomEnable; however, in this case the CustomAgent 
    /// property is still null during OnCustomEnable.
    /// </summary>
    public void LinkWithCustomAgent(CustomNavMeshAgent customAgent)
    {
        customAgentGameObject = (customAgent != null) ? customAgent.gameObject : null;
        this.customAgent = customAgent;

        UpdateAgent();
        UpdateMesh();
        UpdateVisibility();

        UpdateParent();
        UpdatePosition();
        UpdateRotation();
        UpdateScale();

        TrySubscribe();
    }

    /// <summary>
    /// Checks if this hidden agent is linked with the specified custom agent.
    /// </summary>
    /// <param name="customAgent">The custom agent.</param>
    /// <returns>True if the specified agent is indeed the linked one; otherwise, false.</returns>
    public bool IsLinkedWith(CustomNavMeshAgent customAgent)
    {
        return CustomAgent == customAgent;
    }

    protected override void OnCustomEnable()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = CustomNavMesh.HiddenAgentMaterial;
        }

        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = gameObject.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
        }
        obstacle.enabled = false;

        // if the Agent property has to add a new NavMeshAgent, it has to be disabled 
        // because it might be wrongly placed in the world if called from OnEnable
        Agent.enabled = false;
    }

    protected override void OnCustomDisable()
    {
        TryUnsubscribe();
    }

    // hide Start method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomStart is only called in Play mode
    new void Start()
    {
        if(Obstacle.enabled == false)
        {
            Agent.enabled = true;
        }

        TrySubscribe();
    }

    protected override void OnCustomStart()
    {
        lastPosition = transform.position;
    }

    protected override void OnCustomUpdate()
    {
        Vector3 agentPos = CustomAgent.transform.position;
        Vector3 translation = CustomNavMesh.HiddenTranslation;

        // why not call UpdatePosition? it is slower, does unnecessary calculations
        transform.position = new Vector3(
            agentPos.x + translation.x,
            transform.position.y,
            agentPos.z + translation.z);

        float currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;

        if(currentSpeed < CustomAgent.UnblockSpeedThreshold) // if it did not surpass the speed threshold
        {
            timer += Time.deltaTime;

            if(IsBlocking)
            {
                if (timer >= CustomAgent.BlockRefreshInterval)
                {
                    timer = 0.0f;
                    TryUnblock();
                }
            }
            else
            {
                if (timer >= CustomAgent.TimeToBlock)
                {
                    IsBlocking = true;
                }
            }
        }
        else
        {
            timer = 0.0f;

            if (IsBlocking)
            {
                IsBlocking = false;
            }
        }

        lastPosition = transform.position;
    }

    void SwitchToAgent()
    {
        isBlocking = false;

        obstacle.enabled = false;
        agent.enabled = true;

        timer = 0.0f;
        GetComponent<MeshRenderer>().sharedMaterial = CustomNavMesh.HiddenAgentMaterial;
    }

    void SwitchToObstacle()
    {
        isBlocking = true;

        agent.enabled = false;
        obstacle.enabled = true;

        timer = 0.0f;
        GetComponent<MeshRenderer>().sharedMaterial = CustomNavMesh.HiddenBlockingAgentMaterial;
    }

    void TryUnblock()
    {
        if (destination.HasValue)
        {
            // The centered agent position where it touches the surface
            Vector3 agentSurfacePos = new Vector3(
                transform.position.x,
                transform.position.y - CustomAgent.Height / 2.0f * transform.localScale.y,
                transform.position.z);

            NavMeshQueryFilter queryFilter = new NavMeshQueryFilter();
            queryFilter.agentTypeID = customAgent.AgentTypeID;
            queryFilter.areaMask = NavMesh.AllAreas;

            // Try finding a valid position in surface around agentSurfacePos (the position itself is occupied by it's own obstacle)
            float maxDistance = SurfaceAgentRadius + CustomAgent.Radius * posSamplingModifier;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, maxDistance, queryFilter))
            {
                NavMeshPath path = new NavMeshPath();

                // Try finding a valid position in surface around destination
                maxDistance = SurfaceAgentRadius + CustomAgent.Radius * destSamplingModifier;
                if (NavMesh.SamplePosition(destination.Value, out NavMeshHit destinationHit, maxDistance, queryFilter))
                {
                    // Check if there's a path to the destination
                    if (NavMesh.CalculatePath(hit.position, destinationHit.position, queryFilter, path))
                    {
                        // Check if last calculated path position is closer, so the agent can leave block mode
                        Vector3 lastPathPos = path.corners[path.corners.Length - 1];
                        if (Vector3.Distance(lastPathPos, destination.Value) + CustomAgent.MinDistanceBoostToStopBlock < Vector3.Distance(agentSurfacePos, destination.Value))
                        {
                            SwitchToAgent();
                            Agent.SetPath(path);
                        }
                    }
                }
            }
        }
    }

    void UpdateAgent()
    {
        if (CustomAgent != null && Agent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(Agent, "");
#endif
            // Update my NavMeshAgent
            CustomNavMeshAgent.TransferAgentValues(CustomAgent, Agent);
            Agent.baseOffset = CustomAgent.Height / 2.0f; // keep mesh centered

#if UNITY_EDITOR         
            Undo.RecordObject(Obstacle, "");
#endif
            // Update my NavMeshObstacle
            Obstacle.carvingMoveThreshold = CustomAgent.CarvingMoveThreshold;
            Obstacle.carvingTimeToStationary = CustomAgent.CarvingTimeToStationary;
            Obstacle.carveOnlyStationary = CustomAgent.CarveOnlyStationary;
        }
    }

    void UpdateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(Obstacle, "");
#endif
            var height = CustomAgent.Height;
            var radius = CustomAgent.Radius;

            var scale = CustomAgent.transform.localScale;
            var realHeight = height * Mathf.Abs(scale.y);
            var realRadius = radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));

            if (realHeight / 2.0f < realRadius)
            {
                Obstacle.shape = NavMeshObstacleShape.Box;
                Obstacle.size = new Vector3(radius * 2.0f, height, radius * 2.0f);
            }
            else
            {
                Obstacle.shape = NavMeshObstacleShape.Capsule;
                Obstacle.height = height;
                Obstacle.radius = radius;
            }

#if UNITY_EDITOR
            Undo.RecordObject(meshFilter, "");
#endif

            if(Agent.enabled)
            {
                Vector3 meshScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
                meshFilter.sharedMesh = PrimitiveType.Cylinder.CreateScaledMesh(meshScale);
            }
            else
            {
                if(Obstacle.shape == NavMeshObstacleShape.Box)
                {
                    Vector3 meshScale = new Vector3(radius * 2f, height, radius * 2f);
                    meshFilter.sharedMesh = PrimitiveType.Cube.CreateScaledMesh(meshScale);
                }
                else
                {
                    Vector3 meshScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
                    meshFilter.sharedMesh = PrimitiveType.Capsule.CreateScaledMesh(meshScale);
                }
            }
        }
    }

    void UpdateVisibility()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(meshRenderer, "");
#endif
            meshRenderer.enabled = CustomNavMesh.RenderHidden;
        }
    }

    void UpdateParent()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.parent = CustomAgent.transform.parent;
        }
    }

    void UpdatePosition()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.position = customAgent.transform.position + CustomNavMesh.HiddenTranslation +
                (customAgent.Height / 2.0f * Mathf.Sign(customAgent.transform.localScale.y) - customAgent.BaseOffset) *
                Vector3.up * customAgent.transform.localScale.y;
        }
    }

    void UpdateRotation()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.rotation = CustomAgent.transform.rotation;
        }
    }


    void UpdateScale()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            var customTransform = CustomAgent.transform;

            UpdatePosition();

            var radiusScale = Mathf.Max(Mathf.Abs(customTransform.localScale.x), Mathf.Abs(customTransform.localScale.z));
            Vector3 newScale = new Vector3(radiusScale, customTransform.localScale.y, radiusScale);

            if (transform.localScale != newScale)
            {
                transform.localScale = newScale;
                UpdateMesh();
            }
        }
    }

    void TrySubscribe()
    {
        if (!subscribed)
        {
            if (CustomAgent != null)
            {
                CustomAgent.onChange += UpdateAgent;
                CustomAgent.onAgentMeshChange += UpdateMesh;
                CustomAgent.onAgentPositionChange += UpdatePosition;

                CustomAgent.onParentChange += UpdateParent;
                CustomAgent.onRotationChange += UpdateRotation;
                CustomAgent.onScaleChange += UpdateScale;

                // only subscribe if it's currently outside of play mode because in play 
                // mode the position is already set in the OnCustomUpdate method
                if (!Application.isPlaying)
                {
                    CustomAgent.onPositionChange += UpdatePosition;
                }
            }

            CustomNavMesh.onRenderHiddenUpdate += UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate += UpdatePosition;

            subscribed = true;
        }
    }

    void TryUnsubscribe()
    {
        if (subscribed)
        {
            if (CustomAgent != null)
            {
                CustomAgent.onChange -= UpdateAgent;
                CustomAgent.onAgentMeshChange -= UpdateMesh;
                CustomAgent.onAgentPositionChange -= UpdatePosition;

                CustomAgent.onParentChange -= UpdateParent;
                CustomAgent.onRotationChange -= UpdateRotation;
                CustomAgent.onScaleChange -= UpdateScale;

                if (!Application.isPlaying)
                {
                    CustomAgent.onPositionChange -= UpdatePosition;
                }
            }

            CustomNavMesh.onRenderHiddenUpdate -= UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate -= UpdatePosition;

            subscribed = false;
        }
    }
}
