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

    private bool clearedForTakeOff = true;
    public float heightToSitAboveRunway = 1f;
    private float landingTurnSpeed = 30f;
    private FlightState flightState;
    
    void Start()
    {
       // playerSpeed = 5;
       // flightState = FlightState.OK;
        
        player = gameObject.transform.GetChild(0).gameObject;
        playerRoll = gameObject.transform.GetChild(0).GetChild(0).gameObject;

        SetPlayerStartPositions();
    }

    void SetPlayerStartPositions()
    {  
        playerSpeed = 0;
        flightState = FlightState.Landed;
       
        Vector3 position = terrain.GetPosition(terrain.GetPlayerStartAngle());
        position.y = 0;
        player.transform.position = position;
        player.transform.Rotate(Vector3.up, -Mathf.Rad2Deg * terrain.GetPlayerStartAngle()); //orient the player correctly

        //player height is determined by height of the pivot arm to which it is attached. 
        float startHeight = terrain.GetHeightOfRunway(terrain.GetPlayerStartAngle()) + heightToSitAboveRunway;

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
        //Debug.Log(currentAttackAngle);

        CheckFlightState();
        
        if(flightState == FlightState.Landed)
        {
            GroundControls();
        }
        else
        {
            AirControls();
        }

    }

    private void GroundControls()
    {
        float runwayHeight = terrain.GetHeightOfRunway(player.transform.position.x, player.transform.position.z);

        if(runwayHeight < 0) //miss, must be not over runway
        {
            flightState = FlightState.OK;
            return;
        }

        if(Input.GetKey(KeyCode.D))
        {
            if(playerSpeed >= landingSpeed)
            {
                flightState = FlightState.OK;
                return;
            }

            
            if(playerSpeed < 0.001f)
            {
                float turnAmount = landingTurnSpeed * Time.fixedDeltaTime;

                float turnAngle = playerRoll.transform.localRotation.eulerAngles.y;

                if(turnAngle - turnAmount < 0 )
                {
                    
                    playerRoll.transform.Rotate(Vector3.up, -turnAngle, Space.Self);
                    currentAttackAngle = 0;
                    clearedForTakeOff = true;
                    
                }
                else
                {
                    playerRoll.transform.Rotate(Vector3.up, -turnAmount, Space.Self);
                    clearedForTakeOff = false;
                    
                }

            }
        }
        
        if(Input.GetKey(KeyCode.A))
        {
            if(playerSpeed >= landingSpeed)
            {
                flightState = FlightState.OK;
                return;
            }
            
            if(playerSpeed < 0.001f)
            {
                float turnAmount = landingTurnSpeed * Time.fixedDeltaTime;

                float turnAngle = playerRoll.transform.localRotation.eulerAngles.y;

                if(turnAngle < 180)
                {
                    clearedForTakeOff = false;
                
                    playerRoll.transform.Rotate(Vector3.up, turnAmount, Space.Self);
                }

                turnAngle = playerRoll.transform.localRotation.eulerAngles.y;

                if(turnAngle > 180)
                {
                    playerRoll.transform.Rotate(Vector3.up, -(turnAngle - 180), Space.Self);

                    currentAttackAngle = 180;
                    clearedForTakeOff = true;
                }
            }
            
        }
        
        if(Input.GetKey(KeyCode.W)) //accelerate
        {
            if(clearedForTakeOff)
            {
                playerSpeed += (acceleration * Time.fixedDeltaTime);
            }

        }
        
        if(Input.GetKey(KeyCode.S)) //Decelerate
        {
            
            playerSpeed -= (acceleration * Time.fixedDeltaTime);
        }

        
        playerSpeed = Mathf.Clamp(playerSpeed, 0, maxSpeed);
      
        Vector2 playerVelocity;

        playerVelocity.x = Mathf.Cos(Mathf.Deg2Rad * currentAttackAngle) * playerSpeed;
        playerVelocity.y = 0;

        
        //angular velocity is horizontal projection of playerVelocity

        angularVelocity = -playerVelocity.x;
        
        //rotate the pivot
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        Vector3 position = gameObject.transform.position;

        position.y = runwayHeight + heightToSitAboveRunway;
        
        gameObject.transform.position = position;

    }

    private void AirControls()
    {

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

    public void CheckFlightState()
    {

        if(flightState == FlightState.Landed)
        {
            if(playerSpeed >= landingSpeed) //check for takeoff
            {
                flightState = FlightState.OK;
            }
            return;
        }

        if(playerSpeed < stallSpeed)
        {
            flightState = FlightState.Stall;
            return;
        }
        if(playerSpeed < landingSpeed)
        {
            flightState =  FlightState.Landing;
            return;
        }

        flightState = FlightState.OK;
        return;
    }

    public void HasTouchedRunway()
    {
        if(LandingSafely())
        {
            flightState = FlightState.Landed;
        }
        else
        {
            Debug.Log("crashed on runway");
        }
    }
    public bool LandingSafely()
    {
        if(playerSpeed > landingSpeed)
        {
            return false;
        }

        if(FlyingToTheRight() && (currentAttackAngle >= 350 || currentAttackAngle == 0f))
        {
            return true;
        }

        if(!FlyingToTheRight() && currentAttackAngle >= 180.0 && currentAttackAngle < 190.0)
        {
            return true;
        }

        return false;
    }

}
