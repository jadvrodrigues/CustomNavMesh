using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[ExecuteAlways, AddComponentMenu("")] // remove from Add Component list
public class CustomNavMesh : MonoBehaviour
{
    static CustomNavMesh instance;
    static CustomNavMesh Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (CustomNavMesh)FindObjectOfType(typeof(CustomNavMesh));

                if (instance == null)
                {
                    if(!SceneManager.GetActiveScene().isLoaded) return null; // null if leaving the scene

                    var singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<CustomNavMesh>();
                    singletonObject.name = typeof(CustomNavMesh).ToString() + " (Singleton)";
                    singletonObject.transform.SetAsFirstSibling();
                    singletonObject.transform.hideFlags = HideFlags.HideInInspector;
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the 
    /// CustomNavMesh's hidden translation is updated.
    /// </summary>
    public delegate void OnHiddenTranslationUpdate();

    event OnHiddenTranslationUpdate m_onHiddenTranslationUpdate;
    /// <summary>
    /// Subscribe functions to be called after the CustomNavMesh's hidden translation is updated.
    /// This is reset every time you enter Play mode.
    /// </summary>
    public static event OnHiddenTranslationUpdate onHiddenTranslationUpdate
    {
        add
        {
            if (Instance != null) Instance.m_onHiddenTranslationUpdate += value;
        }
        remove
        {
            if (Instance != null) Instance.m_onHiddenTranslationUpdate -= value;
        }
    }

    [SerializeField, HideInInspector] Vector3 hiddenTranslation = new Vector3(1000.0f, 0.0f, 0.0f);
    /// <summary>
    /// The distance between the shown surface and the hidden surface where most of 
    /// the CustomNavMesh's calculations are done.
    /// </summary>
    public static Vector3 HiddenTranslation
    {
        get
        {
            return Instance.hiddenTranslation;
        }
        set
        {
            if (Instance.hiddenTranslation != value)
            {
                Instance.hiddenTranslation = value;
                Instance.m_onHiddenTranslationUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the 
    /// CustomNavMesh's render hidden is updated.
    /// </summary>
    public delegate void OnRenderHiddenUpdate();

    event OnRenderHiddenUpdate m_onRenderHiddenUpdate;
    /// <summary>
    /// Subscribe functions to be called after the CustomNavMesh's render hidden is updated. 
    /// This is reset every time you enter Play mode.
    /// </summary>
    public static event OnRenderHiddenUpdate onRenderHiddenUpdate
    {
        add
        {
            if (Instance != null) Instance.m_onRenderHiddenUpdate += value;
        }
        remove
        {
            if (Instance != null) Instance.m_onRenderHiddenUpdate -= value;
        }
    }

    [SerializeField, HideInInspector] bool renderHidden = true;
    /// <summary>
    /// Whether the hidden nav mesh components are visible or not.
    /// </summary>
    public static bool RenderHidden
    {
        get
        {
            return Instance.renderHidden;
        }
        set
        {
            if (Instance.renderHidden != value)
            {
                Instance.renderHidden = value;
                Instance.m_onRenderHiddenUpdate?.Invoke();
            }
        }
    }

    Material hiddenSurfaceMaterial;
    /// <summary>
    /// The material used by all of the hidden surfaces.
    /// </summary>
    public static Material HiddenSurfaceMaterial
    {
        get
        {
            if (Instance.hiddenSurfaceMaterial == null)
            {
                Instance.hiddenSurfaceMaterial = Resources.Load<Material>("Materials/HiddenSurface");
            }
            return Instance.hiddenSurfaceMaterial;
        }
    }

    Material hiddenAgentMaterial;
    /// <summary>
    /// The material used by all of the hidden agents that are currently moving freely.
    /// </summary>
    public static Material HiddenAgentMaterial
    {
        get
        {
            if (Instance.hiddenAgentMaterial == null)
            {
                Instance.hiddenAgentMaterial = Resources.Load<Material>("Materials/HiddenAgent");
            }
            return Instance.hiddenAgentMaterial;
        }
    }

    Material hiddenBlockingAgentMaterial;
    /// <summary>
    /// The material used by all of the hidden agents that are currently in block mode.
    /// </summary>
    public static Material HiddenBlockingAgentMaterial
    {
        get
        {
            if (Instance.hiddenBlockingAgentMaterial == null)
            {
                Instance.hiddenBlockingAgentMaterial = Resources.Load<Material>("Materials/HiddenBlockingAgent");
            }
            return Instance.hiddenBlockingAgentMaterial;
        }
    }

    Material hiddenObstacleMaterial;
    /// <summary>
    /// The material used by all of the hidden obstacles.
    /// </summary>
    public static Material HiddenObstacleMaterial
    {
        get
        {
            if (Instance.hiddenObstacleMaterial == null)
            {
                Instance.hiddenObstacleMaterial = Resources.Load<Material>("Materials/HiddenObstacle");
            }
            return Instance.hiddenObstacleMaterial;
        }
    }

    /// <summary>
    /// Same as NavMesh.SamplePosition but all the calculations are done in the hidden surface. 
    /// Finds the closest point on NavMesh within specified range.
    /// </summary>
    /// <param name="sourcePosition">The origin of the sample query.</param>
    /// <param name="hit">Holds the properties of the resulting location.</param>
    /// <param name="maxDistance">Sample within this distance from sourcePosition.</param>
    /// <param name="areaMask">A mask specifying which NavMesh areas are allowed when finding 
    /// the nearest point.</param>
    /// <returns>True if a nearest point is found.</returns>
    public static bool SamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask)
    {
        var result = NavMesh.SamplePosition(sourcePosition + HiddenTranslation, out hit, maxDistance, areaMask);
        hit.position -= HiddenTranslation;
        return result;
    }

    /// <summary>
    /// Same as NavMesh.Raycast but all the calculations are done in the hidden surface. 
    /// Trace a line between two points on the NavMesh.
    /// </summary>
    /// <param name="sourcePosition">The origin of the ray.</param>
    /// <param name="targetPosition">The end of the ray.</param>
    /// <param name="hit">Holds the properties of the ray cast resulting location.</param>
    /// <param name="areaMask">A bitfield mask specifying which NavMesh areas can be passed when tracing the ray.</param>
    /// <returns></returns>
    public static bool Raycast(Vector3 sourcePosition, Vector3 targetPosition, out NavMeshHit hit, int areaMask)
    {
        var hiddenSource = sourcePosition + HiddenTranslation;
        var hiddenTarget = targetPosition + HiddenTranslation;
        var result = NavMesh.Raycast(hiddenSource, hiddenTarget, out hit, areaMask);
        hit.position -= HiddenTranslation;
        return result;
    }

    private void OnDestroy()
    {
        if (Time.frameCount == 0) return; // ignore when entering and leaving play mode
        if (!SceneManager.GetActiveScene().isLoaded) return; // ignore when changing scene

        // Disable all custom components (surfaces, obstacles and agents)
        Debug.LogWarning("CustomNavMesh singleton instance is being destroyed. Disabling all dependant components.");
        foreach(CustomNavMeshSurface surface in FindObjectsOfType(typeof(CustomNavMeshSurface)))
        {
            surface.enabled = false;
        }
        foreach (CustomNavMeshObstacle obstacle in FindObjectsOfType(typeof(CustomNavMeshObstacle)))
        {
            obstacle.enabled = false;
        }
        foreach (CustomNavMeshAgent agent in FindObjectsOfType(typeof(CustomNavMeshAgent)))
        {
            agent.enabled = false;
        }
    }
}
