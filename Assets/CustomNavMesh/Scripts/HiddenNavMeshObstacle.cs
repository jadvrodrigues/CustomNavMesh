using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// An obstacle for HiddenNavMeshAgents to avoid.
/// </summary>
[DisallowMultipleComponent, AddComponentMenu("")] // remove from Add Component list
public class HiddenNavMeshObstacle : CustomMonoBehaviour
{
    bool subscribed; // used to avoid subscribing twice

    CustomNavMeshObstacle customObstacle;
    CustomNavMeshObstacle CustomObstacle
    {
        get
        {
            if (customObstacle == null && transform.parent != null)
            {
                customObstacle = transform.parent.GetComponent<CustomNavMeshObstacle>();
            }
            return customObstacle;
        }
        set
        {
            customObstacle = value;
        }
    }

    NavMeshObstacle navMeshObstacle;
    NavMeshObstacle NavMeshObstacle
    {
        get
        {
            if (navMeshObstacle == null)
            {
                navMeshObstacle = GetComponent<NavMeshObstacle>();
                if (navMeshObstacle == null)
                {
                    navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
                }
                else
                {
                    // update existing nav mesh obstacle
                    CustomNavMeshObstacle.TransferObstacleValues(CustomObstacle, navMeshObstacle);
                }
            }
            return navMeshObstacle;
        }
    }

    protected override void OnCustomEnable()
    {
        // destroy self if it doesn't have a parent with a CustomNavMeshObstacle component
        if (transform.parent != null)
        {
            if (CustomObstacle == null)
            {
                DestroyImmediate(this);
                return;
            }
            else if (!CustomObstacle.enabled)
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
            meshRenderer.sharedMaterial = CustomNavMesh.HiddenObstacleMaterial;
        }

        UpdateObstacle();
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
            if (CustomObstacle != null)
            {
                transform.parent = CustomObstacle.transform; // prevent from being changed
                transform.position = CustomObstacle.transform.position + CustomNavMesh.HiddenTranslation;
            }

            transform.hasChanged = false;
        }
    }

    void UpdateObstacle()
    {
        if (CustomObstacle != null && NavMeshObstacle != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(NavMeshObstacle, "");
#endif
            CustomNavMeshObstacle.TransferObstacleValues(CustomObstacle, NavMeshObstacle);

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(meshFilter, "");
#endif
                switch(NavMeshObstacle.shape)
                {
                    case NavMeshObstacleShape.Box:
                        meshFilter.sharedMesh = PrimitiveType.Cube.CreateScaledMesh(NavMeshObstacle.size);
                        break;
                    case NavMeshObstacleShape.Capsule:
                        float radius = NavMeshObstacle.radius;
                        float height = NavMeshObstacle.height;
                        Vector3 scale = new Vector3 (radius * 2f, height, radius * 2f);
                        meshFilter.sharedMesh = PrimitiveType.Capsule.CreateScaledMesh(scale);
                        break;
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

    void UpdatePosition()
    {
        if (CustomObstacle != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(transform, "");
#endif
            transform.position = CustomObstacle.transform.position + CustomNavMesh.HiddenTranslation;
            transform.hasChanged = false;
        }
    }

    void TrySubscribe()
    {
        if (!subscribed)
        {
            if (CustomObstacle != null) CustomObstacle.onChange += UpdateObstacle;

            CustomNavMesh.onRenderHiddenUpdate += UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate += UpdatePosition;

            subscribed = true;
        }
    }

    void TryUnsubscribe()
    {
        if (subscribed)
        {
            if (CustomObstacle != null) CustomObstacle.onChange -= UpdateObstacle;

            CustomNavMesh.onRenderHiddenUpdate -= UpdateVisibility;
            CustomNavMesh.onHiddenTranslationUpdate -= UpdatePosition;

            subscribed = false;
        }
    }
}
