using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    // Start is called before the first frame update

    public GenerateTerrain terrain;
    private float angle = 0f;
    void Start()
    {
        gameObject.transform.position = terrain.GetPosition(angle);
        Debug.Log("go: " + gameObject.transform.position + " " + terrain.GetPosition(angle));
    }

    // Update is called once per frame
    void Update()
    {
        angle += 0.001f;
        gameObject.transform.position = terrain.GetPosition(angle);
    }
}
