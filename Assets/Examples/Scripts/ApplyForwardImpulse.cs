using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyForwardImpulse : MonoBehaviour
{
    public float intensity = 10.0f;
    public float stopVelocity = 0.1f;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(transform.forward * intensity, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if(rb.velocity.magnitude < stopVelocity)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
