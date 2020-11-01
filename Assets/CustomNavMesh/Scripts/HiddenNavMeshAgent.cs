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
                    // update existing nav mesh agent
                    CustomNavMeshAgent.TransferAgentValues(CustomAgent, agent);
                }
            }
            return agent;
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
            CustomNavMeshAgent.TransferAgentValues(CustomAgent, Agent);
            Agent.baseOffset = CustomAgent.Height / 2.0f; // keep mesh centered
        }
    }

    void UpdateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && CustomAgent != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(meshFilter, "");
#endif
            float radius = CustomAgent.Radius;
            float height = CustomAgent.Height;
            Vector3 scale = new Vector3(radius * 2f, height / 2f, radius * 2f);

            //adjust for CustomAgent local scale (so it looks like the NavMeshAgent cyllinder in the CustomAgent)
            var xScale = Mathf.Abs(CustomAgent.transform.localScale.x);
            var zScale = Mathf.Abs(CustomAgent.transform.localScale.z);

            if (xScale == 0.0f) xScale = 1.0f;
            if (zScale == 0.0f) zScale = 1.0f;

            if (xScale < zScale)
                scale.x *= zScale / xScale;
            else 
                scale.z *= xScale / zScale;

            meshFilter.sharedMesh = PrimitiveType.Cylinder.CreateScaledMesh(scale);
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

            if (transform.localScale != customTransform.localScale)
            {
                transform.localScale = customTransform.localScale;
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
