using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hidden navigation mesh agent.
/// </summary>
[DisallowMultipleComponent, AddComponentMenu("")] // remove from Add Component list
public class HiddenNavMeshAgent : CustomMonoBehaviour
{
    bool subscribed; // used to avoid subscribing twice

    Vector3? destination;
    float timeStopped;

    /// <summary>
    /// Access the current velocity of the hidden agent component.
    /// Returns Vector3.zero if it's currently in block mode. 
    /// </summary>
    public Vector3 Velocity
    {
        get
        {
            if (Agent.enabled)
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

    CustomNavMeshAgent customAgent;
    CustomNavMeshAgent CustomAgent
    {
        get
        {
            if (customAgent == null)
            {
                CustomNavMesh.TryGetCustomAgent(this, out customAgent);
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
            if (Application.isPlaying && isBlocking != value)
            {
                isBlocking = value;
                if (value)
                {
                    agent.enabled = false;
                    obstacle.enabled = true;
                }
                else
                {
                    obstacle.enabled = false;
                    agent.enabled = true;

                    timeStopped = 0.0f;

                    if (destination.HasValue)
                    {
                        agent.SetDestination(destination.Value);
                    }
                }
            }
        }
    }

    Vector3? lastPosition;
    Vector3? LastPosition
    {
        get
        {
            if(!lastPosition.HasValue)
            {
                lastPosition = transform.position;
            }
            return lastPosition;
        }
        set
        {
            lastPosition = value;
        }
    }

    public bool SetDestination(Vector3 target)
    {
        IsBlocking = false;
        destination = target;
        return agent.SetDestination(target);
    }

    /// <summary>
    /// Update everything. To be called from CustomNavMesh when this get registered. Note: in the 
    /// agent and in the obstacle classes, this initialization is done in OnCustomEnable; however, 
    /// in this case the CustomAgent is still null during OnCustomEnable.
    /// </summary>
    public void OnRegister()
    {
        UpdateAgent();
        UpdateMesh();
        UpdateVisibility();

        UpdateParent();
        UpdatePosition();
        UpdateRotation();
        UpdateScale();

        TrySubscribe();
    }

    protected override void OnCustomEnable()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
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

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    protected override void OnCustomDisable()
    {
        TryUnsubscribe();
    }

    // hide Start method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomStart is only called in Play mode
    new void Start()
    {
        TrySubscribe();
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

        if (Vector3.Distance(transform.position, LastPosition.Value) / Time.deltaTime < CustomAgent.UnblockSpeedThreshold)
        {
            if (!IsBlocking)
            {
                timeStopped += Time.deltaTime;

                if (timeStopped >= CustomAgent.TimeToBlock)
                {
                    IsBlocking = true;
                }
            }
        }
        else
        {
            timeStopped = 0.0f;

            if (IsBlocking)
            {
                IsBlocking = false;
            }
        }

        LastPosition = transform.position;
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
