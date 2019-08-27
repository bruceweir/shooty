﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetGameObject;
    public GenerateTerrain terrain;
    public float minimumHeightAboveGround;
    public float distanceFromTarget;
    private Vector3 predictedTargetPosition;
    private Vector3 positionChange;
    private Vector3 previousPosition;
    public float framesOfPrediction = 1f; 
    public float predictionFilterCoeff = 0.9f;
    public float minFov = 45f;
    Camera activeCamera;
    void Start()
    {
        activeCamera = Camera.main;
        activeCamera.farClipPlane = (terrain.terrainRadius * 2.1f) + distanceFromTarget;
        positionChange = Vector3.zero;
        previousPosition = targetGameObject.transform.position;
    }

    void Update()
    {
        SetCameraFoV();
    }

    void SetCameraFoV()
    {
        Vector3 screenPos = activeCamera.WorldToScreenPoint(targetGameObject.transform.position);
        Vector3 viewPortCoords = activeCamera.ScreenToViewportPoint(screenPos);

        viewPortCoords.z = 0;
        float distanceFromScreenCentre = (viewPortCoords - new Vector3(0.5f, 0.5f, 0)).magnitude;

        if(distanceFromScreenCentre > 0.45f)
           {
               activeCamera.fieldOfView += 0.4f;
               return;
           }
        
        if(distanceFromScreenCentre < 0.2f && activeCamera.fieldOfView > minFov)
           {
               activeCamera.fieldOfView -= 0.4f;
           }

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        positionChange = targetGameObject.transform.position - previousPosition;

        predictedTargetPosition = targetGameObject.transform.position + (positionChange * framesOfPrediction);

        Vector3 targetDirection = predictedTargetPosition.normalized;

        float requiredDistance = targetGameObject.transform.position.magnitude + distanceFromTarget;

        Vector3 desiredCameraPosition = targetDirection * requiredDistance;

        float cameraAngleAroundTerrain = terrain.GetAngleRoundTerrain(desiredCameraPosition);

        float height = terrain.GetHeightOfTerrain(cameraAngleAroundTerrain);

        if(height != -1f)
        {
            if((desiredCameraPosition.y - height) < minimumHeightAboveGround)
            {
                desiredCameraPosition.y = minimumHeightAboveGround + height;
            }
        }
       
        activeCamera.transform.position = desiredCameraPosition;

        activeCamera.transform.LookAt(Vector3.zero);

        previousPosition = (predictionFilterCoeff * previousPosition) + ((1 - predictionFilterCoeff) * targetGameObject.transform.position);
    }
}
