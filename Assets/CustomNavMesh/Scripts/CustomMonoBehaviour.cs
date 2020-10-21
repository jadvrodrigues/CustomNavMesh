using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

/// <summary>
/// Monobehaviour replacement class used to simplify some UnityMessages and 
/// turn them into uniquely identifiable methods. Hiding the protected 
/// UnityMessages will stop these methods from being called. Also disables 
/// the reset option in the editor. Has ExecuteAlways attribute.
/// </summary>
[ExecuteAlways]
public class CustomMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// Called from editor and play mode when component is added, becomes enabled, is 
    /// instantiated, upon entering Play and prefab mode, on script reload and moving 
    /// game object to the Assets folder to create a prefab. Only called if game 
    /// object is active and in the scene.
    /// </summary>
    protected virtual void OnCustomEnable() { }

    /// <summary>
    /// Called from editor and play mode when component goes from enabled to disabled 
    /// (possible situations include toggling it off, removing it through the inspector, 
    /// destroying it, on script reload, upon leaving Play mode and moving game object 
    /// to the Assets folder to create a prefab. Only  called if game object is active 
    /// and in the scene.
    /// </summary>
    protected virtual void OnCustomDisable() { }

    /// <summary>
    /// Called from editor and play mode when component/owner gameObject is destroyed.
    /// Only called if game object is active and in the scene.
    /// </summary>
    protected virtual void OnCustomDestroy() { }

    /// <summary>
    /// Called when Awake is called in play mode.
    /// </summary>
    protected virtual void OnCustomAwake() { }

    /// <summary>
    /// Called when Start is called in play mode.
    /// </summary>
    protected virtual void OnCustomStart() { }

    /// <summary>
    /// Called when Update is called in play mode.
    /// </summary>
    protected virtual void OnCustomUpdate() { }

    protected void OnEnable()
    {
#if UNITY_EDITOR
        // ignore for the first time when entering Play mode (will only be called once 
        // on entering Play mode);
        if (Time.frameCount == 0 && !Application.isPlaying) return;
#endif
        OnCustomEnable();
    }

    protected void OnDisable()
    {
#if UNITY_EDITOR
        if (Time.frameCount == 0 && !Application.isPlaying) return;
#endif
        OnCustomDisable();
    }

    protected void OnDestroy()
    {
#if UNITY_EDITOR
        if (Time.frameCount == 0) return;
#endif
        OnCustomDestroy();
    }

    protected void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        OnCustomAwake();
    }

    protected void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        OnCustomStart();
    }

    protected void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        OnCustomUpdate();
    }

#if UNITY_EDITOR
    [MenuItem("CONTEXT/CustomMonoBehaviour/Reset")]
    static void ReplacedReset() { } // replace reset option

    [MenuItem("CONTEXT/CustomMonoBehaviour/Reset", true)]
    static bool ValidateReplacedReset() { return false; } // disable reset
#endif

}
