using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    // Start is called before the first frame update

    public GenerateTerrain terrain;
    public float angularVelocity = 0.01f;
    void Start()
    {
        SetChildPositions();
    }

    void SetChildPositions()
    {
        Vector3 position = terrain.GetPosition(-gameObject.transform.rotation.eulerAngles.y);
        position.y = 0.0f;
        foreach(Transform child in transform)
        {
            child.position = position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        gameObject.transform.Rotate(0, angularVelocity, 0, Space.World);
        
        float height = terrain.GetHeightOfTerrain(Mathf.Deg2Rad * -gameObject.transform.rotation.eulerAngles.y);
        
        Vector3 position = gameObject.transform.position;
        position.y = height;
        gameObject.transform.position = position;
    }
}
