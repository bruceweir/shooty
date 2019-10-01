using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineFlame : MonoBehaviour
{
    // Start is called before the first frame update
    public float scaleAtMaxSpeed = 2;
    public float scaleAtMinSpeed = 0.2f;
    
    public ControlJet controlJet;

    // Update is called once per frame
    void Update()
    {
        if(controlJet == null)
        {
            return;
        }
        if(controlJet.maxSpeed - controlJet.minSpeed != 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Random.Range(0f, .25f) + Mathf.Lerp(scaleAtMinSpeed, scaleAtMaxSpeed, (controlJet.playerSpeed-controlJet.minSpeed)/(controlJet.maxSpeed-controlJet.minSpeed)));            
        }
    }
}
