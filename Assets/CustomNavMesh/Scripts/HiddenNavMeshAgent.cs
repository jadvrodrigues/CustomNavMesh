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
                }
            }
            return obstacle;
        }
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
        UpdateTransform();

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

    void UpdateTransform()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            var customTransform = CustomAgent.transform;
            transform.parent = customTransform.parent; // prevent from being changed
            transform.rotation = customTransform.rotation;

            UpdatePosition();

            var radiusScale = Mathf.Max(Mathf.Abs(customTransform.localScale.x), Mathf.Abs(customTransform.localScale.z));
            Vector3 newScale = new Vector3(radiusScale, customTransform.localScale.y, radiusScale);

            if (transform.localScale != newScale)
            {
                transform.localScale = newScale;
                UpdateMesh();
            }

            transform.hasChanged = false;
        }
    }

    void UpdatePosition()
    {
        if (CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.position = CustomAgent.transform.position + CustomNavMesh.HiddenTranslation + 
                (CustomAgent.Height / 2.0f * Mathf.Sign(CustomAgent.transform.localScale.y) - CustomAgent.BaseOffset) * 
                Vector3.up * CustomAgent.transform.localScale.y;

            transform.hasChanged = false;
        }
    }

    void TrySubscribe()
    {
        if (!subscribed)
        {
            if (CustomAgent != null)
            {
                CustomAgent.onChange += UpdateAgent;
                CustomAgent.onTransformChange += UpdateTransform;
                CustomAgent.onAgentMeshChange += UpdateMesh;
                CustomAgent.onAgentPositionChange += UpdatePosition;
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
                CustomAgent.onTransformChange -= UpdateTransform;
                CustomAgent.onAgentMeshChange -= UpdateMesh;
                CustomAgent.onAgentPositionChange -= UpdatePosition;
            }

            CustomNavMesh.onRenderHiddenUpdate -= UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate -= UpdatePosition;

            subscribed = false;
        }
    }
}
