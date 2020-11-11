using UnityEditor;
using UnityEngine;

/// <summary>
/// A surface for CustomNavMeshAgents to move on.
/// </summary>
[DisallowMultipleComponent]
public class CustomNavMeshSurface : CustomMonoBehaviour
{
    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the surface's mesh is updated.
    /// </summary>
    public delegate void OnMeshUpdate();
    /// <summary>
    /// Subscribe a function to be called after the surface's mesh is updated.
    /// </summary>
    public event OnMeshUpdate onMeshUpdate;

    [HideInInspector, SerializeField] Mesh mesh;
    /// <summary>
    /// The surface's mesh. The mesh filter's mesh should be changed through this property. 
    /// Avoid changing the MeshFilter's mesh directly; otherwise, the onMeshUpdate won't 
    /// be invoked and the HiddenNavMeshSurface won't udpate its own mesh.
    /// </summary>
    public Mesh Mesh
    {
        get 
        {
            if (mesh == null && MeshFilter != null)
            {
                mesh = MeshFilter.sharedMesh;
            }
            return mesh; 
        }
        set
        {
            mesh = value;
            if (MeshFilter != null)
            {
                MeshFilter.sharedMesh = mesh;
                onMeshUpdate?.Invoke();
            }
        }
    }

    HiddenNavMeshSurface hiddenSurface;
    HiddenNavMeshSurface HiddenSurface
    {
        get
        {
            if(hiddenSurface == null)
            {
                hiddenSurface = transform.GetComponentInImmediateChildren<HiddenNavMeshSurface>();
            }
            return hiddenSurface;
        }
        set
        {
            hiddenSurface = value;
        }
    }

    MeshFilter meshFilter;
    MeshFilter MeshFilter
    {
        get
        {
            if(meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                if(meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = mesh;
                }
            }
            return meshFilter;
        }
    }

#if UNITY_EDITOR
    // only method called as a prefab in the Assets folder (except OnCustomDestroy)
    private void OnValidate()
    {
        if(onPrefabAssetRemoveComponent == null)
        {
            onPrefabAssetRemoveComponent = TryDestroyHiddenSurfaceChild;
        }

        if (PrefabUtility.IsPartOfPrefabAsset(this))
        {
            var filter = GetComponent<MeshFilter>();
            if (filter != null)
            {
                if (mesh == null)
                {
                    mesh = filter.sharedMesh;
                }
            }

            if(HiddenSurface != null)
            {
                var hiddenMeshFilter = HiddenSurface.GetComponent<MeshFilter>();
                if (hiddenMeshFilter != null && hiddenMeshFilter.sharedMesh != mesh)
                {
                    hiddenMeshFilter.sharedMesh = mesh; // will trigger warning
                }
            }
        }
        else
        {
            // the following code is used to remove extra hidden nav mesh surfaces in prefab instances where 
            // the custom nav mesh surface in the prefab asset was disabled and it didn't have a child hidden 
            // surface, then you enabled the custom nav mesh surface in the prefab instance and then did the 
            // same for the prefab asset resulting in a prefab instance with two hidden nav mesh surfaces
            var childrenSurfaces = transform.GetComponentsInImmediateChildren<HiddenNavMeshSurface>();
            if (childrenSurfaces.Length > 1)
            {
                for (int i = 0; i < childrenSurfaces.Length; i++)
                {
                    // remove the one that isn't linked to the prefab
                    if (!PrefabUtility.IsPartOfPrefabInstance(childrenSurfaces[i]))
                    {
                        var go = childrenSurfaces[i].gameObject;
                        EditorApplication.delayCall += () => DestroyImmediate(go); // avoid warning
                    }
                }
            }
        }
    }
#endif

    protected override void OnCustomEnable()
    {
        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }

        TryCreatingHiddenSurface();
    }

    protected override void OnCustomDisable()
    {
        TryDisablingHiddenSurface();
    }

    protected override void OnCustomDestroy()
    {
        MeshFilter.hideFlags = HideFlags.None;

        if (gameObject.activeInHierarchy) // used to avoid destroying things twice, when gameObject is destroyed
        {
#if UNITY_EDITOR
            if (!PrefabUtility.IsPartOfAnyPrefab(this) ||
                UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                if (HiddenSurface != null)
                {
                    Undo.DestroyObjectImmediate(HiddenSurface.gameObject);
                }
            }
#endif
        }
    }

    void TryCreatingHiddenSurface()
    {
        if (HiddenSurface == null)
        {
            var hiddenObject = new GameObject("(Hidden) " + name);
            hiddenObject.transform.SetParent(transform, false);
            hiddenObject.hideFlags = HideFlags.NotEditable;

#if UNITY_EDITOR
            var staticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            GameObjectUtility.SetStaticEditorFlags(hiddenObject, staticFlags);

            var navMeshArea = GameObjectUtility.GetNavMeshArea(gameObject);
            GameObjectUtility.SetNavMeshArea(hiddenObject, navMeshArea);
#else
        hiddenObject.isStatic = gameObject.isStatic;
#endif

            HiddenSurface = hiddenObject.AddComponent<HiddenNavMeshSurface>();
        }
        else
        {
            HiddenSurface.gameObject.name = "(Hidden) " + name;
            HiddenSurface.gameObject.SetActive(true);
            HiddenSurface.gameObject.hideFlags = HideFlags.NotEditable; // reapply hide flags after activating it
            HiddenSurface.enabled = true;
        }
    }

    // Why disable and not destroy? Destroying a child gameObject interferes both with 
    // the Undo function and with prefabs if this is part of one.
    void TryDisablingHiddenSurface()
    {
        if (HiddenSurface != null)
        {
            HiddenSurface.gameObject.name = "(Unused) " + name;
            HiddenSurface.gameObject.SetActive(false);
            HiddenSurface.enabled = false;
        }
    }

    static void TryDestroyHiddenSurfaceChild(GameObject root)
    {
        var hiddenSurface = root.transform.GetComponentInImmediateChildren<HiddenNavMeshSurface>();
        if(hiddenSurface != null)
        {
            DestroyImmediate(hiddenSurface.gameObject);
        }
    }
}
