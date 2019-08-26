using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    // Start is called before the first frame update

    public GenerateTerrain terrain;
    public float angularVelocity = 1f;
    public float acceleration = .1f;
    public float maxSpeed = 5.0f;

    public float turnRate = 1.0f;
    private Vector2 playerVelocity;
    private float lastAngle = 0f;

    private GameObject child;

    void Start()
    {
        playerVelocity = new Vector2(0.01f, 0f);
        lastAngle = Mathf.Atan2(playerVelocity.y, playerVelocity.x);

        SetChildPositions();

        child = gameObject.transform.GetChild(0).gameObject;
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
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }
    void FixedUpdate()
    {

        float currentAngle = Mathf.Atan2(playerVelocity.y, playerVelocity.x);
        //Debug.Log(currentAngle);
                
        if(Input.GetKey(KeyCode.D))
        {
            currentAngle += turnRate * Time.fixedDeltaTime;

            playerVelocity.x = Mathf.Cos(currentAngle) * playerVelocity.magnitude;
            playerVelocity.y = Mathf.Sin(currentAngle) * playerVelocity.magnitude;

            child.transform.Rotate(Vector3.right, Mathf.Rad2Deg * turnRate * Time.fixedDeltaTime);
        }
        if(Input.GetKey(KeyCode.A))
        {
            currentAngle -= turnRate * Time.fixedDeltaTime;

            playerVelocity.x = Mathf.Cos(currentAngle) * playerVelocity.magnitude;
            playerVelocity.y = Mathf.Sin(currentAngle) * playerVelocity.magnitude;

            child.transform.Rotate(-Vector3.right, Mathf.Rad2Deg * turnRate * Time.fixedDeltaTime);
        }

    
        lastAngle = currentAngle;

        float currentSpeed = playerVelocity.magnitude;
        
        if(Input.GetKey(KeyCode.W)) //accelerate
        {
            currentSpeed += (acceleration * Time.fixedDeltaTime);
            //Debug.Log("W " + acceleration + " " + Time.fixedDeltaTime + " " + currentSpeed);      
        }
        if(Input.GetKey(KeyCode.S)) //Decelerate
        {
            currentSpeed -= (acceleration * Time.fixedDeltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0.00000f, maxSpeed);
        
        if(currentSpeed > 0.00f)
        {
            if(playerVelocity.normalized.magnitude < 0.0001f)
            {
                playerVelocity.x = Mathf.Cos(lastAngle) * 0.001f;
                playerVelocity.y = Mathf.Sin(lastAngle) * 0.001f;
            }
            else
            {
                
            }

            playerVelocity = playerVelocity.normalized * currentSpeed;  
           
        }
        else
        {
            playerVelocity.x = 0;
            playerVelocity.y = 0;
        }

        //angular velocity is horizontal projection of playerVelocity

        angularVelocity = playerVelocity.x;
        
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        //vertical motion is the vertical component of the playerVelocity 

        float height = terrain.GetHeightOfTerrain(Mathf.Deg2Rad * -gameObject.transform.rotation.eulerAngles.y);
        
        Vector3 position = gameObject.transform.position;
        position.y += playerVelocity.y * Time.fixedDeltaTime * terrain.terrainRadius * Mathf.Deg2Rad;//height;
        
        gameObject.transform.position = position;
        //Debug.Log("" + playerVelocity.y * Time.fixedDeltaTime + " " + gameObject.transform.position.y);
    }
}
