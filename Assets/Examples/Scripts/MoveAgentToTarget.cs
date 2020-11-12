using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgentToTarget : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;

    private void Start()
    {
        target = GameObject.Find("Target").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}
