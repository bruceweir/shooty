using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPivot : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotationSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime);
    }
}
