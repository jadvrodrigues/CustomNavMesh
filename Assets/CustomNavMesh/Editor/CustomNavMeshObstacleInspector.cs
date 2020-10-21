using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CanEditMultipleObjects, CustomEditor(typeof(CustomNavMeshObstacle))]
public class CustomNavMeshObstacleInspector : Editor
{
    CustomNavMeshObstacle obstacle;
    NavMeshObstacle navMeshObstacle;

    private void OnEnable()
    {
        obstacle = target as CustomNavMeshObstacle;
        navMeshObstacle = obstacle.GetComponent<NavMeshObstacle>();
        if (navMeshObstacle != null)
        {
            // prevent the user from changing the obstacle through the NavMeshObstacle's inspector
            navMeshObstacle.hideFlags = HideFlags.HideInInspector;
        }
    }

    private void OnDisable()
    {
        if (target == null && navMeshObstacle != null)
        {
            navMeshObstacle.hideFlags = HideFlags.None;
        }
    }

}
