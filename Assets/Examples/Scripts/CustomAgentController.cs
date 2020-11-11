using UnityEngine;

[RequireComponent(typeof(CustomNavMeshAgent))]
public class CustomAgentController : MonoBehaviour
{
    [SerializeField] Vector3 targetPosition;

    CustomNavMeshAgent agent;
    CustomNavMeshAgent Agent
    {
        get
        {
            if(agent == null)
            {
                agent = GetComponent<CustomNavMeshAgent>();
            }
            return agent;
        }
    }

    public bool SetDestination(Vector3 target)
    {
        return Agent.SetDestination(target);
    }
}
