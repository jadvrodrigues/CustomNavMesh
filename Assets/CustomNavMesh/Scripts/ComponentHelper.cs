using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class with methods to retrieve immediate children components. Immediate children 
/// are the ones one level down their parent's level in the hierarchy.
/// </summary>
public static class ComponentHelper
{
    /// <summary>
    /// Returns the component of Type type in the GameObject or any of its immmediate children.
    /// A component is returned only if it is found on an active GameObject.
    /// </summary>
    /// <typeparam name="T">The type of Component to retrieve.</typeparam>
    /// <param name="component">The parent component.</param>
    /// <returns>A component of the matching type, if found.</returns>
    public static T GetComponentInImmediateChildren<T>(this Component component) where T : Component
    {
        foreach (Transform child in component.transform)
        {
            T comp = child.GetComponent<T>();
            if (comp != null)
            {
                return comp;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns all components of Type type in any of its immediate children. 
    /// Note: Unlike GetComponentsInChildren, this method doesn't return any 
    /// components in the GameObject.
    /// </summary>
    /// <typeparam name="T">The type of Component to retrieve.</typeparam>
    /// <param name="component">The parent component.</param>
    /// <returns>A list of all found components matching the specified type.</returns>
    public static T[] GetComponentsInImmediateChildren<T>(this Component component) where T : Component
    {
        List<T> comps = new List<T>();
        foreach (Transform child in component.transform)
        {
            T comp = child.GetComponent<T>();
            if (comp != null)
            {
                comps.Add(comp);
            }
        }
        return comps.ToArray();
    }
}
