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
    private float playerSpeed;
    private float currentAngle = 0f;
    private GameObject child;

    void Start()
    {
        playerSpeed = 0;
        
        SetChildPositions();

        child = gameObject.transform.GetChild(0).gameObject;
        Debug.Log("child: " + child.name);
    }

    void SetChildPositions()
    {
        
        Vector3 position = terrain.GetPosition(-gameObject.transform.rotation.eulerAngles.y);
    //    position.y = 0.0f;
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
          
        if(Input.GetKey(KeyCode.D))
        {
            float angleChange = turnRate * Time.fixedDeltaTime;

            currentAngle += angleChange;
            
            child.transform.Rotate(Vector3.right, angleChange);
            
        }
        if(Input.GetKey(KeyCode.A))
        {
            float angleChange = turnRate * Time.fixedDeltaTime;
            
            currentAngle -= angleChange;

            child.transform.Rotate(Vector3.right, -angleChange);            
        }

        currentAngle = currentAngle % 360f;

        
        if(Input.GetKey(KeyCode.W)) //accelerate
        {
            playerSpeed += (acceleration * Time.fixedDeltaTime);
        }
        if(Input.GetKey(KeyCode.S)) //Decelerate
        {
            playerSpeed -= (acceleration * Time.fixedDeltaTime);
        }

        playerSpeed = Mathf.Clamp(playerSpeed, 0.00000f, maxSpeed);
      
        Vector2 playerVelocity;
      
        playerVelocity.x = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * playerSpeed;
        playerVelocity.y = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * playerSpeed;

        //angular velocity is horizontal projection of playerVelocity

        angularVelocity = -playerVelocity.x;
        
        //rotate the pivot
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        //vertical motion is the vertical component of the playerVelocity 
        //move the pivot vertically        

        Vector3 position = gameObject.transform.position;
        position.y -= playerVelocity.y * Time.fixedDeltaTime * terrain.terrainRadius * Mathf.Deg2Rad;//height;
        
        gameObject.transform.position = position;
    }
}
