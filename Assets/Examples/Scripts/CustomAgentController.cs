using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CustomNavMeshAgent))]
public class CustomAgentController : MonoBehaviour
{
    [SerializeField] Vector3 targetPosition;
    [SerializeField] Vector3 moveOffset;
    [SerializeField] Vector3 newPosition;

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

    public void Move(Vector3 offset)
    {
        Agent.Move(offset);
    }

    public void ResetPath()
    {
        Agent.ResetPath();
    }

    public bool Warp(Vector3 newPosition)
    {
        return Agent.Warp(newPosition);
    }
}
