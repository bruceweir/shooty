using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollision(Collision collision)
    {
        Debug.Log("player collision");
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("player trigger: " + collider.gameObject.name + " " + collider.gameObject.tag);

        if(collider.CompareTag("Runway"))
        {
            Debug.Log("Landing?");
        }
        if(collider.CompareTag("Terrain"))
        {
            Debug.Log("crash");
        }
    }
}
