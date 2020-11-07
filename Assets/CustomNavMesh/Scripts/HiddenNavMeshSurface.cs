using UnityEditor;
using UnityEngine;

/// <summary>
/// A surface for HiddenNavMeshAgents to move on.
/// </summary>
[DisallowMultipleComponent, AddComponentMenu("")] // remove from Add Component list
public class HiddenNavMeshSurface : CustomMonoBehaviour
{
    bool subscribed; // used to avoid subscribing twice

    CustomNavMeshSurface customSurface;
    CustomNavMeshSurface CustomSurface
    {
        get
        {
            if (customSurface == null && transform != null && transform.parent != null)
            {
                customSurface = transform.parent.GetComponent<CustomNavMeshSurface>();
            }
            return customSurface;
        }
        set
        {
            customSurface = value;
        }
    }

    protected override void OnCustomEnable()
    {
        // destroy self if it doesn't have a parent with a CustomNavMeshSurface component
        if (transform.parent != null)
        {
            if (CustomSurface == null)
            {
                DestroyImmediate(this);
                return;
            }
            else if(!CustomSurface.enabled)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            DestroyImmediate(this);
            return;
        }

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = CustomNavMesh.HiddenSurfaceMaterial;
        }

        UpdateMesh();
        UpdateVisibility();
        UpdatePosition();

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

    // hide Update method because this has to be triggered both inside and outside 
    // of Play mode and the inherited OnCustomUpdate is only called in Play mode
    new void Update()
    {
        // since this is a child of the gameObject with the custom surface, 
        // it automatically follows its position and rotation; however, if 
        // the scale of any of the parents change, the position has to be
        // updated; this also prevents parent from being changed
        if (transform.hasChanged)
        {
            if (CustomSurface != null)
            {
                transform.parent = CustomSurface.transform; // prevent from being changed
                transform.position = CustomSurface.transform.position + CustomNavMesh.HiddenTranslation;
            }

            transform.hasChanged = false;
        }
    }

    void UpdateMesh()
    {
        if (CustomSurface != null)
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(meshFilter, "");
#endif
                meshFilter.sharedMesh = CustomSurface.Mesh;
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

    void UpdatePosition()
    {
        if (CustomSurface != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.position = CustomSurface.transform.position + CustomNavMesh.HiddenTranslation;
            transform.hasChanged = false;
        }
    }

    void TrySubscribe()
    {
        if (!subscribed)
        {
            if(CustomSurface != null) CustomSurface.onMeshUpdate += UpdateMesh;

            CustomNavMesh.onRenderHiddenUpdate += UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate += UpdatePosition;

            subscribed = true;
        }
    }

    void TryUnsubscribe()
    {
        if (subscribed)
        {
            if (CustomSurface != null) CustomSurface.onMeshUpdate -= UpdateMesh;

            CustomNavMesh.onRenderHiddenUpdate -= UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate -= UpdatePosition;

            subscribed = false;
        }
    }
}
