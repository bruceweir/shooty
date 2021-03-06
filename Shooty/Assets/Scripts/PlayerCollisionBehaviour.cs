﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionBehaviour : MonoBehaviour
{
    // Start is called before the first frame update

    public ControlJet controlJet;

    void Start()
    {
        Debug.Log("PlayerCollisionBehaviour");
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
            controlJet.HasTouchedRunway();
        }
        if(collider.CompareTag("Terrain"))
        {
 //           Debug.Log("crash");
            controlJet.Crashed();
        }
    }
}
