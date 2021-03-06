﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetGameObject;
    public GeneratedTerrain terrain;
    public float minimumHeightAboveGround;
    public float distanceFromTarget;
    public float minimumDistanceFromTarget = 10;
    private Vector3 predictedTargetPosition;
    private Vector3 positionChange;
    private Vector3 previousPosition;
    public float framesOfPrediction = 1f; 
    public float predictionFilterCoeff = 0.9f;
    public float cameraDollySpeed = 0.2f;
    Camera activeCamera;
    void Start()
    {
        activeCamera = Camera.main;
        positionChange = Vector3.zero;
        
        //previousPosition = targetGameObject.transform.position;
    }

    void Update()
    {
        SetCameraFraming();
        activeCamera.farClipPlane = 1000000;//(terrain.terrainRadius * 2.1f) + distanceFromTarget;
        
    }

    void SetCameraFraming()
    {
        if(targetGameObject == null)
        {
            return;
        }

        Vector3 screenPos = activeCamera.WorldToScreenPoint(targetGameObject.transform.position);
        Vector3 viewPortCoords = activeCamera.ScreenToViewportPoint(screenPos);

        viewPortCoords.z = 0;
        float distanceFromScreenCentre = (viewPortCoords - new Vector3(0.5f, 0.5f, 0)).magnitude;

        if(distanceFromScreenCentre > .5f)
        {
            distanceFromTarget += 0.8f;
            return;
        }

        if(distanceFromScreenCentre > 0.45f)
        {
            distanceFromTarget += cameraDollySpeed;
            return;
        }
    
        if(distanceFromScreenCentre < 0.2f && distanceFromTarget > minimumDistanceFromTarget)
        {
            distanceFromTarget -= cameraDollySpeed;
        }
        

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(targetGameObject == null || targetGameObject.name == "PlayerExplosion")
        {
            targetGameObject = GameObject.Find("Player");

            if(targetGameObject == null)
            {
                targetGameObject = GameObject.Find("PlayerExplosion");
            }
            if(targetGameObject == null)
            {
                return;
            }
        }
        
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
       
        activeCamera.transform.position = Vector3.Lerp(activeCamera.transform.position, desiredCameraPosition, Time.deltaTime*10);

        activeCamera.transform.LookAt(Vector3.zero);

        previousPosition = (predictionFilterCoeff * previousPosition) + ((1 - predictionFilterCoeff) * targetGameObject.transform.position);
    }
}
