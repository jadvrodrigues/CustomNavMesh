using UnityEngine;

[RequireComponent(typeof(CustomNavMeshAgent))]
public class MoveCustomAgentToTarget : MonoBehaviour
{
    CustomNavMeshAgent agent;
    Transform target;

    private void Start()
    {
        target = GameObject.Find("Target").transform;
        agent = GetComponent<CustomNavMeshAgent>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}
