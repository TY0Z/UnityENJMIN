using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Controller : MonoBehaviour
{
    public float speed = 1f;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(rb.velocity.magnitude < speed)
        {
            float direction = Input.GetAxis("Vertical");
            if(direction != 0)
            {
                rb.AddForce(0, 0, direction * Time.fixedDeltaTime * 1000f);
            }
            direction = Input.GetAxis("Horizontal");
            if(direction != 0)
            {
                rb.AddForce(direction * Time.fixedDeltaTime * 1000f, 0f, 0f);
            }
        }
    }
}
