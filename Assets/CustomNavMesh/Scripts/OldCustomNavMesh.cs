using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// The NavMesh singleton class replacement for using the nav mesh.
/// </summary>
public class OldCustomNavMesh : ScriptableObject
{
    static readonly Color surfaceColor = new Color(0.5215687f, 0.5215687f, 0.5215687f); // gray
    static readonly Color agentColor = new Color(0.2509804f, 0.3764706f, 0.2509804f); // green
    static readonly Color obstacleColor = new Color(0.6235294f, 0.2509804f, 0.2509804f); // red

    static bool settingsLoaded;

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

    static OldCustomNavMesh instance;
    static OldCustomNavMesh Instance
    {
        get
        {
            if (!settingsLoaded)
            {
                var instanceLoaded = Resources.Load<OldCustomNavMesh>("Settings");

                if (instanceLoaded != null)
                {
                    instance = instanceLoaded;
                    settingsLoaded = true;
                }
                else
                {
                    if (instance == null)
                    {
                        instance = CreateInstance<OldCustomNavMesh>();
                    }

#if UNITY_EDITOR
                    string settingsPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    settingsPath = "Assets" + settingsPath.Substring(Application.dataPath.Length);
                    settingsPath = settingsPath.Remove(settingsPath.LastIndexOf("\\"));
                    settingsPath = settingsPath.Remove(settingsPath.LastIndexOf("\\"));
                    settingsPath += "\\Resources\\Settings.asset";

                    // check if the Settings asset already exists before creating a new one (it might just not be loaded yet)
                    if (!AssetDatabase.GetAllAssetPaths().Any(p => p.Equals(settingsPath.Replace("\\", "/"))))
                    {
                        AssetDatabase.CreateAsset(instance, settingsPath);
                        AssetDatabase.SaveAssets();
                    }
#endif
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
            if(Instance.hiddenTranslation != value)
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
            if(Instance.renderHidden != value)
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
                Instance.hiddenSurfaceMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = surfaceColor
                };

#if UNITY_EDITOR
                if (AssetDatabase.Contains(Instance) && !AssetDatabase.Contains(Instance.hiddenSurfaceMaterial))
                {
                    AssetDatabase.AddObjectToAsset(Instance.hiddenSurfaceMaterial, Instance);
                }
                else
                {
                    EditorUtility.CopySerialized(Instance.hiddenSurfaceMaterial, Instance.hiddenSurfaceMaterial);
                }
                AssetDatabase.SaveAssets();
#endif
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
                Instance.hiddenAgentMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = agentColor
                };

#if UNITY_EDITOR
                if (AssetDatabase.Contains(Instance) && !AssetDatabase.Contains(Instance.hiddenAgentMaterial))
                {
                    AssetDatabase.AddObjectToAsset(Instance.hiddenAgentMaterial, Instance);
                }
                else
                {
                    EditorUtility.CopySerialized(Instance.hiddenAgentMaterial, Instance.hiddenAgentMaterial);
                }
                AssetDatabase.SaveAssets();
#endif

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
                Instance.hiddenObstacleMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = obstacleColor
                };

#if UNITY_EDITOR
                if (AssetDatabase.Contains(Instance) && !AssetDatabase.Contains(Instance.hiddenObstacleMaterial))
                {
                    AssetDatabase.AddObjectToAsset(Instance.hiddenObstacleMaterial, Instance);
                }
                else
                {
                    EditorUtility.CopySerialized(Instance.hiddenObstacleMaterial, Instance.hiddenObstacleMaterial);
                }
                AssetDatabase.SaveAssets();
#endif

            }
            return Instance.hiddenObstacleMaterial;
        }
    }

    // Why use a GameObject to GameObject dictionary instead of CustomNavMeshAgent to HiddenNavMeshAgent? 
    // To avoid them from resetting to null and loosing the correspondence between the two. This is the 
    // only agent dictionary that is serialized, the others are all null after entering play mode.
    GameObjectDictionary customToHiddenAgents;
    Dictionary<GameObject, GameObject> CustomToHiddenAgents
    {
        get
        {
            if (customToHiddenAgents == null)
            {
                customToHiddenAgents = new GameObjectDictionary();
            }
            return customToHiddenAgents;
        }
    }

    Dictionary<GameObject, GameObject> hiddenToCustomAgents;
    Dictionary<GameObject, GameObject> HiddenToCustomAgents
    {
        get
        {
            if (hiddenToCustomAgents == null)
            {
                hiddenToCustomAgents = new Dictionary<GameObject, GameObject>();
                foreach (var kvp in CustomToHiddenAgents)
                {
                    hiddenToCustomAgents.Add(kvp.Value, kvp.Key);
                }
            }
            return hiddenToCustomAgents;
        }
    }

    // Why make this playing versions of the agent correspondence dictionaries? To 
    // manipulate them freely in play mode and discard the changes when leaving it.
    Dictionary<GameObject, GameObject> playingCustomToHiddenAgents;
    Dictionary<GameObject, GameObject> PlayingCustomToHiddenAgents
    {
        get
        {
            if (playingCustomToHiddenAgents == null)
            {
                playingCustomToHiddenAgents = new Dictionary<GameObject, GameObject>();
                foreach (var kvp in CustomToHiddenAgents)
                {
                    playingCustomToHiddenAgents.Add(kvp.Key, kvp.Value);
                }
            }
            return playingCustomToHiddenAgents;
        }
    }

    Dictionary<GameObject, GameObject> playingHiddenToCustomAgents;
    Dictionary<GameObject, GameObject> PlayingHiddenToCustomAgents
    {
        get
        {
            if (playingHiddenToCustomAgents == null)
            {
                playingHiddenToCustomAgents = new Dictionary<GameObject, GameObject>();
                foreach (var kvp in CustomToHiddenAgents)
                {
                    playingHiddenToCustomAgents.Add(kvp.Value, kvp.Key);
                }
            }
            return playingHiddenToCustomAgents;
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
        if(Application.isPlaying)
        {
            Instance.PlayingHiddenToCustomAgents[hiddenAgent.gameObject] = customAgent.gameObject;
            Instance.PlayingCustomToHiddenAgents[customAgent.gameObject] = hiddenAgent.gameObject;
        }
        else
        {
            Instance.CustomToHiddenAgents[customAgent.gameObject] = hiddenAgent.gameObject;

            if(Instance.hiddenToCustomAgents != null)
            {
                Instance.hiddenToCustomAgents[hiddenAgent.gameObject] = customAgent.gameObject;
            }
        }

        hiddenAgent.OnRegister();
    }

    /// <summary>
    /// Unregister the correspondence betweem a custom agent and it's hidden agent.
    /// </summary>
    /// <param name="customAgent">The custom agent</param>
    /// <param name="hiddenAgent">The hidden agent</param>
    public static void UnregisterAgent(CustomNavMeshAgent customAgent, HiddenNavMeshAgent hiddenAgent)
    {
        if(Application.isPlaying)
        {
            Instance.PlayingHiddenToCustomAgents.Remove(hiddenAgent.gameObject);
            Instance.PlayingCustomToHiddenAgents.Remove(customAgent.gameObject);
        }
        else
        {
            Instance.HiddenToCustomAgents.Remove(hiddenAgent.gameObject);
            Instance.CustomToHiddenAgents.Remove(customAgent.gameObject);
        }
    }

    /// <summary>
    /// Tries to retrieve the hidden agent from a custom agent. Returns false if the hidden agent is null.
    /// </summary>
    /// <param name="customAgent">The custom agent</param>
    /// <param name="hiddenAgent">The hidden agent</param>
    /// <returns></returns>
    public static bool TryGetHiddenAgent(CustomNavMeshAgent customAgent, out HiddenNavMeshAgent hiddenAgent)
    {
        GameObject hiddenObject;
        if(Application.isPlaying)
        {
            Instance.PlayingCustomToHiddenAgents.TryGetValue(customAgent.gameObject, out hiddenObject);
        }
        else
        {
            Instance.CustomToHiddenAgents.TryGetValue(customAgent.gameObject, out hiddenObject);
        }
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
        GameObject customObject;
        if (Application.isPlaying)
        {
            Instance.PlayingHiddenToCustomAgents.TryGetValue(hiddenAgent.gameObject, out customObject);
        }
        else
        {
            Instance.HiddenToCustomAgents.TryGetValue(hiddenAgent.gameObject, out customObject);
        }

        customAgent = (customObject != null) ? customObject.GetComponent<CustomNavMeshAgent>() : null;
        return customAgent != null;
    }

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Serialiable game object to game object dictionary.
    /// </summary>
    [Serializable] class GameObjectDictionary : UDictionary<GameObject, GameObject> { }
}
