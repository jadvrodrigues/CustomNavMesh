using UnityEngine;

/// <summary>
/// Hidden navigation mesh agent.
/// </summary>
[DisallowMultipleComponent, AddComponentMenu("")] // remove from Add Component list
public class HiddenNavMeshAgent : CustomMonoBehaviour
{
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
}
