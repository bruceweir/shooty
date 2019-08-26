using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetGameObject;
    public GenerateTerrain terrain;
    public float minimumHeightAboveGround;
    public float distanceFromTarget;
    Camera camera;
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetDirection = targetGameObject.transform.position.normalized;

        float requiredDistance = terrain.terrainRadius + distanceFromTarget;

        Vector3 desiredCameraPosition = targetDirection * requiredDistance;

        float height = terrain.GetHeightOfTerrain(desiredCameraPosition);

        if(height != -1f)
        {
            if((desiredCameraPosition.y - height) < minimumHeightAboveGround)
            {
                desiredCameraPosition.y = minimumHeightAboveGround + height;
            }
        }

        camera.transform.position = desiredCameraPosition;

        camera.transform.LookAt(targetGameObject.transform);
    }
}
