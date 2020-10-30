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

        TrySubscribe();
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
            Vector3 scale = new Vector3(radius * 2f, height, radius * 2f);
            meshFilter.sharedMesh = PrimitiveType.Capsule.CreateScaledMesh(scale);
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
            transform.position = customTransform.position + CustomNavMesh.HiddenTranslation;
            transform.rotation = customTransform.rotation;

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
            transform.position = CustomAgent.transform.position + CustomNavMesh.HiddenTranslation;
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
                CustomAgent.onSizeChange += UpdateMesh;
                CustomAgent.onTransformChange += UpdateTransform;
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
                CustomAgent.onSizeChange -= UpdateMesh;
                CustomAgent.onTransformChange -= UpdateTransform;
            }

            CustomNavMesh.onRenderHiddenUpdate -= UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate -= UpdatePosition;

            subscribed = false;
        }
    }
}
