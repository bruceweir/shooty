using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    
    public GenerateTerrain terrain;
    public float angularVelocity = 1f;
    public float acceleration = .1f;
    public float maxSpeed = 5.0f;
    public float turnRate = 1.0f;
    private float playerSpeed;
    private float currentAngle = 0f;
    private GameObject player;
    private GameObject playerRoll;
    float inversionCountdownValue;
    private bool performingRoll = false;
    private float targetRollRotation  = 0;
    private float totalRollRotation = 0;
    private float rollSpeed = 90f;
    
    void Start()
    {

        playerSpeed = 5;
        
        player = gameObject.transform.GetChild(0).gameObject;
        playerRoll = gameObject.transform.GetChild(0).GetChild(0).gameObject;

        Debug.Log(playerRoll.name);

        SetPlayerStartPositions();

      //  StartCoroutine(InversionCountdown());

    }

    void SetPlayerStartPositions()
    {
        
        Vector3 position = terrain.GetPosition(-gameObject.transform.rotation.eulerAngles.y);
        position.y = 0;
        player.transform.position = position;

        //player height is determined by height of the pivot arm to which it is apparently attached. 
        float startHeight = terrain.heightScale + 3;

        gameObject.transform.position = new Vector3(0, startHeight, 0);
        
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
        
        if(PlayerUpsideDown() && performingRoll == false)
        {
            performingRoll = true;
            totalRollRotation = 0.0f;
            targetRollRotation = (playerRoll.transform.localEulerAngles.z + 180) % 360.0f;
        }
        
        if(Input.GetKey(KeyCode.D))
        {
            float angleChange = turnRate * Time.fixedDeltaTime;

            currentAngle += angleChange;
            
            player.transform.Rotate(Vector3.right, angleChange);

        }
        if(Input.GetKey(KeyCode.A))
        {
            float angleChange = turnRate * Time.fixedDeltaTime;
            
            currentAngle -= angleChange;

            player.transform.Rotate(Vector3.right, -angleChange);     
    
        }

        currentAngle = currentAngle % 360f;

        if(performingRoll)
        {
            float rollAmount = rollSpeed * Time.fixedDeltaTime;

            playerRoll.transform.Rotate(Vector3.forward, rollSpeed * Time.fixedDeltaTime, Space.Self);

            totalRollRotation += rollAmount;
            
            if(totalRollRotation > 180.0f)
            {
                playerRoll.transform.Rotate(Vector3.forward, 180.0f - totalRollRotation, Space.Self);
                performingRoll = false;
            }
        }

        
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

    private bool PlayerUpsideDown()
    {      
        if(playerRoll.transform.up.y > 0)
        {
            return false;
        }
        return true;
    }

}
