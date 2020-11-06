using System;
using UnityEngine;

[AddComponentMenu("")] // remove from Add Component list
public class CustomNavMesh : MonoBehaviour
{
    GameObjectDictionary customToHiddenAgents = new GameObjectDictionary();
    GameObjectDictionary hiddenToCustomAgents = new GameObjectDictionary();

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the 
    /// CustomNavMesh's hidden translation is updated.
    /// </summary>
    public delegate void OnHiddenTranslationUpdate();
    /// <summary>
    /// Subscribe functions to be called after the CustomNavMesh's hidden translation is updated.
    /// This is reset every time you enter Play mode.
    /// </summary>
    public static event OnHiddenTranslationUpdate onHiddenTranslationUpdate;

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

    [SerializeField, HideInInspector] Vector3 hiddenTranslation;
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
                onHiddenTranslationUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// A delegate which can be used to register callback methods to be invoked after the 
    /// CustomNavMesh's render hidden is updated.
    /// </summary>
    public delegate void OnRenderHiddenUpdate();
    /// <summary>
    /// Subscribe functions to be called after the CustomNavMesh's render hidden is updated. 
    /// This is reset every time you enter Play mode.
    /// </summary>
    public static event OnRenderHiddenUpdate onRenderHiddenUpdate;

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
                onRenderHiddenUpdate?.Invoke();
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
    /// The material used by all of the hidden agents.
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
    /// Register a custom agent and it's hidden agent to prevent the custom agent 
    /// from spawning more hidden agents then needed.
    /// </summary>
    /// <param name="customAgent">The custom agent</param>
    /// <param name="hiddenAgent">The hidden agent</param>
    public static void RegisterAgent(CustomNavMeshAgent customAgent, HiddenNavMeshAgent hiddenAgent)
    {
        Instance.customToHiddenAgents[customAgent.gameObject] = hiddenAgent.gameObject;
        Instance.hiddenToCustomAgents[hiddenAgent.gameObject] = customAgent.gameObject;

        hiddenAgent.OnRegister();
    }

    /// <summary>
    /// Unregister the correspondence betweem a custom agent and it's hidden agent.
    /// </summary>
    /// <param name="customAgent">The custom agent</param>
    /// <param name="hiddenAgent">The hidden agent</param>
    public static void UnregisterAgent(CustomNavMeshAgent customAgent, HiddenNavMeshAgent hiddenAgent)
    {
        Debug.Log("unregister");
        Instance.hiddenToCustomAgents.Remove(hiddenAgent.gameObject);
        Instance.customToHiddenAgents.Remove(customAgent.gameObject);
    }

    /// <summary>
    /// Tries to retrieve the hidden agent from a custom agent. Returns false if the hidden agent is null.
    /// </summary>
    /// <param name="customAgent">The custom agent</param>
    /// <param name="hiddenAgent">The hidden agent</param>
    /// <returns></returns>
    public static bool TryGetHiddenAgent(CustomNavMeshAgent customAgent, out HiddenNavMeshAgent hiddenAgent)
    {
        Instance.customToHiddenAgents.TryGetValue(customAgent.gameObject, out GameObject hiddenObject);
        hiddenAgent = (hiddenObject != null) ? hiddenObject.GetComponent<HiddenNavMeshAgent>() : null;
        return hiddenAgent != null;
    }

    /// <summary>
    /// Tries to retrieve the custom agent from an hidden agent. Returns false if the custom agent is null.
    /// </summary>
    /// <param name="hiddenAgent">The hidden agent</param>
    /// <param name="customAgent">The custom agent</param>
    /// <returns></returns>
    public static bool TryGetCustomAgent(HiddenNavMeshAgent hiddenAgent, out CustomNavMeshAgent customAgent)
    {
        Instance.hiddenToCustomAgents.TryGetValue(hiddenAgent.gameObject, out GameObject customObject);
        customAgent = (customObject != null) ? customObject.GetComponent<CustomNavMeshAgent>() : null;
        return customAgent != null;
    }

    private void Start()
    {
        Debug.Log(Instance.customToHiddenAgents.Count);
    }

    /// <summary>
    /// Serialiable custom agent to hidden agent dictionary.
    /// </summary>
    [Serializable] class CustomToHiddenAgentDictionary : UDictionary<CustomNavMeshAgent, HiddenNavMeshAgent> { }

    /// <summary>
    /// Serialiable hidden agent to custom agent dictionary.
    /// </summary>
    [Serializable] class HiddenToCustomAgentDictionary : UDictionary<HiddenNavMeshAgent, CustomNavMeshAgent> { }

    /// <summary>
    /// Serialiable game object to game object dictionary.
    /// </summary>
    [Serializable] class GameObjectDictionary : UDictionary<GameObject, GameObject> { }
}
