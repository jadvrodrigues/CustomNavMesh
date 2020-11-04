using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    CustomNavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<CustomNavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(agent != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                agent.SetDestination(GameObject.Find("Target").transform.position);
            }
        }
    }
}
