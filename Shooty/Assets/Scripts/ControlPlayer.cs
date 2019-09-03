using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FlightState {Stall, Landing, Landed, OK};
public class ControlPlayer : MonoBehaviour
{
    public GenerateTerrain terrain;
    public float angularVelocity = 1f;
    public float acceleration = .1f;
    public float maxSpeed = 5.0f;
    public float turnRate = 1.0f;
    private float playerSpeed;
    public float landingSpeed = 2.0f;
    public float stallSpeed = 1.0f;
    public float minSpeed = 1.0f;
    private float currentAttackAngle = 0f;
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

        SetPlayerStartPositions();
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
        FlightState flightState = GetFlightState();
        
        float attackAngleChange = 0;

        if(PlayerUpsideDown() && performingRoll == false)
        {
            performingRoll = true;
            totalRollRotation = 0.0f;
            targetRollRotation = (playerRoll.transform.localEulerAngles.z + 180) % 360.0f;
        }

        if(flightState == FlightState.Stall)
        {
            attackAngleChange = (1 - Vector3.Dot(player.transform.forward, Vector3.down)) * Time.fixedDeltaTime * 20;

            if(!FlyingToTheRight())
            {
                attackAngleChange *= -1;
            }
        }
        
        if(Input.GetKey(KeyCode.D))
        {
            if(flightState != FlightState.Stall)
            {
                attackAngleChange = turnRate * Time.fixedDeltaTime;
            }
        }

        if(Input.GetKey(KeyCode.A))
        {   
            if(flightState != FlightState.Stall)
            {
                attackAngleChange = turnRate * Time.fixedDeltaTime;
                attackAngleChange *= -1;
            }   
        }

        currentAttackAngle += attackAngleChange;

        player.transform.Rotate(Vector3.right, attackAngleChange);     


        currentAttackAngle = currentAttackAngle % 360f;

        if(currentAttackAngle < 0 )
        {
            currentAttackAngle += 360;
        }

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

        playerSpeed = Mathf.Clamp(playerSpeed, minSpeed, maxSpeed);
      
        Vector2 playerVelocity;

        playerVelocity.x = Mathf.Cos(Mathf.Deg2Rad * currentAttackAngle) * playerSpeed;
        playerVelocity.y = -Mathf.Sin(Mathf.Deg2Rad * currentAttackAngle) * playerSpeed;
        
        float maxVerticalSpeed = GetMaxVerticalSpeed();
        
        if(playerVelocity.y > maxVerticalSpeed)
        {
            playerVelocity.y = maxVerticalSpeed;
        }

        Debug.Log(playerVelocity.y + " " + maxVerticalSpeed);
        //angular velocity is horizontal projection of playerVelocity

        angularVelocity = -playerVelocity.x;
        
        //rotate the pivot
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        //vertical motion is the vertical component of the playerVelocity 
        //move the pivot vertically        

        Vector3 position = gameObject.transform.position;

        
        position.y += playerVelocity.y * Time.fixedDeltaTime * terrain.terrainRadius * Mathf.Deg2Rad;//height;
        
        gameObject.transform.position = position;
    }

    private float GetMaxVerticalSpeed()
    {
        if(playerSpeed > landingSpeed)
        {
            return maxSpeed;
        }
        else
        {
            return (10/landingSpeed) * playerSpeed + -10;
        }
    }

    private bool PlayerUpsideDown()
    {      
        if(playerRoll.transform.up.y > 0)
        {
            return false;
        }
        return true;
    }

    private bool FlyingToTheRight()
    {
        return currentAttackAngle < 90 || currentAttackAngle > 270;
    }

    public FlightState GetFlightState()
    {

        if(playerSpeed < stallSpeed)
        {
            return FlightState.Stall;
        }
        if(playerSpeed < landingSpeed)
        {
            return FlightState.Landing;
        }

        return FlightState.OK;
    }

}
