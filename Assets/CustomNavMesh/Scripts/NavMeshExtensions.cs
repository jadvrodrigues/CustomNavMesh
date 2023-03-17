using UnityEngine.AI;

/// <summary>
/// Static class with utility methods for the NavMesh.
/// </summary>
public static class NavMeshExtensions
{
    /// <summary>
    /// Gets the agent type radius from the NavMesh settings.
    /// </summary>
    /// <param name="agentTypeID">The agent type</param>
    /// <returns>The radius</returns>
    public static float GetAgentTypeRadius(int agentTypeID)
    {
        return NavMesh.GetSettingsByID(agentTypeID).agentRadius;
    }
}
