using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FlightState {Stall, Landing, Landed, OK};
public class ControlPlayer : MonoBehaviour
{
    public GameObject playerModel;
    public GeneratedTerrain terrain;
    public float angularVelocity = 1f;
    public float acceleration = .1f;
    public float maxSpeed = 5.0f;
    public float turnRate = 1.0f;
    public float playerSpeed;
    public float landingSpeed = 2.0f;
    public float stallSpeed = 1.0f;
    public float minSpeed = 1.0f;
    public GameObject[] destructionEffects;
    private float currentAttackAngle = 0f;
    private GameObject player;
    private GameObject playerRoll;
    private GameObject playerTurn;
    private GameObject fighterAudio;
    private AudioSource stallAlarm;
    private bool performingRoll = false;
    private float targetRollRotation  = 0;
    private float totalRollRotation = 0;
    private float rollSpeed = 90f;
    private float stallDescentRate = 0f;

    private bool LandedToTheRight = true;
    private Quaternion turnTargetToTheRight;
    private Quaternion turnTargetToTheLeft;
    
    private bool clearedForTakeOff = true;
    public float heightToSitAboveRunway = 1f;
    public float landingTurnSpeed = 3f;
    private Quaternion landingTurnTarget;
    private FlightState flightState;
    private float startHeight;
    
    void Start()
    {
        /*
        player = Instantiate(playerModel, Vector3.zero, Quaternion.identity);

        player = gameObject.transform.GetChild(0).gameObject;
        playerRoll = gameObject.transform.GetChild(0).GetChild(0).gameObject;
        playerTurn = playerRoll.transform.GetChild(0).gameObject;
        */

        StartNewPlayer();
    }

    void StartNewPlayer()
    {  
        if(player != null)
        {
            Destroy(player);
        }

        player = Instantiate(playerModel, Vector3.zero, Quaternion.identity);
        player.name = "Player";
        //PlayerCollisionBehaviour pcb = player.GetComponent<PlayerCollisionBehaviour>();
        //pcb.controlPlayer = gameObject.GetComponent<ControlPlayer>();

        player.transform.parent = gameObject.transform;
        playerRoll = player.transform.GetChild(0).gameObject;
        fighterAudio = player.transform.GetChild(1).gameObject;
        stallAlarm = player.transform.GetChild(2).gameObject.GetComponent<AudioSource>();

        Debug.Log(fighterAudio.name);
        playerTurn = playerRoll.transform.GetChild(0).gameObject;
        GameObject fighterModel = playerTurn.transform.GetChild(0).gameObject;
        PlayerCollisionBehaviour pcb = fighterModel.GetComponent<PlayerCollisionBehaviour>();
        pcb.controlPlayer = gameObject.GetComponent<ControlPlayer>();

        GameObject engine1 = fighterModel.transform.GetChild(0).gameObject;
        GameObject engine2 = fighterModel.transform.GetChild(1).gameObject;

        EngineFlame ef1 = engine1.GetComponent<EngineFlame>();
        EngineFlame ef2 = engine2.GetComponent<EngineFlame>();
        
        ef1.controlPlayer = gameObject.GetComponent<ControlPlayer>();
        ef2.controlPlayer = gameObject.GetComponent<ControlPlayer>();
        

        
        


        playerSpeed = 0;
        flightState = FlightState.Landed;
        currentAttackAngle = 0;
        performingRoll = false;
        stallDescentRate = 0;
        player.transform.localRotation = Quaternion.identity;
        playerRoll.transform.localRotation = Quaternion.identity;
        playerTurn.transform.localRotation = Quaternion.identity;

        Vector3 startPosition = terrain.GetPositionOnTerrainSurface(terrain.GetPlayerStartAngle());
        
        gameObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
       
        startPosition.y = 0;
        player.transform.position = startPosition;

        player.transform.Rotate(Vector3.up, -Mathf.Rad2Deg * terrain.GetPlayerStartAngle()); //orient the player correctly
        
        //player height is determined by height of the pivot arm to which it is attached. 
        startHeight = terrain.GetHeightOfRunway(terrain.GetPlayerStartAngle()) + heightToSitAboveRunway;

        gameObject.transform.position = new Vector3(0, startHeight, 0);   

        SetGroundTurnRotationTargetQuaternions();
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
        if(player == null)
        {
            return;
        }
        
        CheckFlightState();
        
        if(flightState == FlightState.Landed)
        {
            GroundControls();
        }
        else
        {
            AirControls();
        }

        AudioLowPassFilter lpf = fighterAudio.GetComponent<AudioLowPassFilter>();
        lpf.cutoffFrequency = Mathf.Clamp((200 + (Mathf.Abs(angularVelocity) * 50)), 200, 900);
        lpf.lowpassResonanceQ = Mathf.Clamp(1 + ((transform.position.y-startHeight) / 300), 1f, 2.5f);

        AudioEchoFilter Aef = fighterAudio.GetComponent<AudioEchoFilter>();
        Aef.delay = Mathf.Max(10, transform.position.y - terrain.GetHeightOfTerrain(player.transform.position));
    }

    private void GroundControls()
    {
        float runwayHeight = terrain.GetHeightOfRunway(player.transform.position.x, player.transform.position.z);

        if(runwayHeight < 0) //miss, must be not over runway
        {
            flightState = FlightState.OK;
            return;
        }

        LevelThePlayerAircraft();

        if(Input.GetKey(KeyCode.D))
        {
            if(playerSpeed >= landingSpeed)
            {
                flightState = FlightState.OK;
                return;
            }

            if(playerSpeed < 0.001f)
            {
                
                playerTurn.transform.localRotation = Quaternion.Lerp(playerTurn.transform.localRotation, turnTargetToTheRight, Time.fixedDeltaTime * landingTurnSpeed);

                if(Mathf.Abs(Quaternion.Dot(playerTurn.transform.localRotation, turnTargetToTheRight)) > 0.999)
                {
                   playerTurn.transform.localRotation = turnTargetToTheRight;
                   clearedForTakeOff = true;
                   currentAttackAngle = 0;
                }
                else
                {
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
                playerTurn.transform.localRotation = Quaternion.Lerp(playerTurn.transform.localRotation, turnTargetToTheLeft, Time.fixedDeltaTime * landingTurnSpeed);

                if(Mathf.Abs(Quaternion.Dot(playerTurn.transform.localRotation, turnTargetToTheLeft)) > 0.999)
                {
                   playerTurn.transform.localRotation = turnTargetToTheLeft;
                   clearedForTakeOff = true;
                   currentAttackAngle = 180;
                }
                else
                {
                    clearedForTakeOff = false;   
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

        angularVelocity = 360 / ((terrain.terrainRadius * 2 * Mathf.PI) / -playerVelocity.x);
        
        //rotate the pivot
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        Vector3 position = gameObject.transform.position;

        position.y = runwayHeight + heightToSitAboveRunway;

        //Debug.Log(runwayHeight + " " + position.y);
        
        gameObject.transform.position = position;

    }

    private void AirControls()
    {

        //Debug.Log("distance: " + terrain.DistanceToPlayerRunway(player.transform.position));
        float attackAngleChange = 0;

        if(PlayerUpsideDown() && performingRoll == false)
        {
            performingRoll = true;
            totalRollRotation = 0.0f;
            targetRollRotation = (playerRoll.transform.localEulerAngles.z + 180) % 360.0f;
        }

        if(flightState == FlightState.Stall)
        {
            stallDescentRate += 1f;
            
            attackAngleChange = (1 - Vector3.Dot(player.transform.forward, Vector3.down)) * Time.fixedDeltaTime * 20;

            if(!FlyingToTheRight())
            {
                attackAngleChange *= -1;
            }

            if(!stallAlarm.isPlaying)
            {
                stallAlarm.Play();
            }
            
        }
        else
        {
            stallDescentRate = Mathf.Max(0, stallDescentRate-1f);
            
            if(stallDescentRate < 0.01) 
            {
                stallAlarm.Stop();
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
        playerVelocity.y = -Mathf.Sin(Mathf.Deg2Rad * currentAttackAngle) * playerSpeed - stallDescentRate;
        
        
        float maxVerticalSpeed = GetMaxVerticalSpeed();
        
        //Debug.Log("maxVerticalSpeed: " + maxVerticalSpeed);
        if(playerVelocity.y > maxVerticalSpeed)
        {
            playerVelocity.y = maxVerticalSpeed;
        }

        //angular velocity is horizontal projection of playerVelocity

        angularVelocity = 360/((terrain.terrainRadius * 2 * Mathf.PI) / -playerVelocity.x);

        //rotate the pivot
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
        
        //vertical motion is the vertical component of the playerVelocity 
        //move the pivot vertically        

        Vector3 position = gameObject.transform.position;

        
        position.y += playerVelocity.y * Time.fixedDeltaTime;
        
        gameObject.transform.position = position;
    }

    private void LevelThePlayerAircraft()
    {
        float attackAngleChange;

        if(FlyingToTheRight())
        {
            attackAngleChange = -currentAttackAngle;
        }
        else
        {
            attackAngleChange = 180 - currentAttackAngle;
        }

        currentAttackAngle += attackAngleChange;
        player.transform.Rotate(Vector3.right, attackAngleChange);

    }

    private float GetMaxVerticalSpeed()
    {
        if(playerSpeed > landingSpeed)
        {
            return maxSpeed;
        }
        else
        {
            return ((10/(landingSpeed)) * playerSpeed) + -10;
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

    public FlightState GetFlightState()
    {
        return flightState;
    }

    public void HasTouchedRunway()
    {
        if(flightState == FlightState.Landed) //ignore new collisions with runway when landed
        {
            return;
        }

        if(LandingSafely())
        {
            flightState = FlightState.Landed;
            Debug.Log("currentAttackAngle: " + currentAttackAngle);
            Debug.Log("player turn " + playerTurn.transform.localRotation);
            SetGroundTurnRotationTargetQuaternions();
        }
        else
        {
            Crashed();
        }

        Debug.Log("Landing to the right: " + LandedToTheRight);
    }
    public bool LandingSafely()
    {
        
        if(playerSpeed > landingSpeed)
        {
            return false;
        }

        if(performingRoll)
        {
            return false;
        }

        if(FlyingToTheRight() && (currentAttackAngle >= 350 || currentAttackAngle == 0f))
        {
            LandedToTheRight = true;
            return true;
        }

        if(!FlyingToTheRight() && currentAttackAngle >= 180.0 && currentAttackAngle < 190.0)
        {
            LandedToTheRight = false;
            return true;
        }

        return false;
    }

    private void SetGroundTurnRotationTargetQuaternions()
    {
        if(FlyingToTheRight())
        {
            turnTargetToTheRight = playerTurn.transform.localRotation;
            turnTargetToTheLeft = playerTurn.transform.localRotation;
            if(turnTargetToTheLeft.y > .99f)
            {
                turnTargetToTheLeft = Quaternion.Euler(0, 0, 0);
            }else
            {
                turnTargetToTheLeft = Quaternion.Euler(0, 180, 0);
            }
            
        }
        else
        {
            turnTargetToTheLeft = playerTurn.transform.localRotation;
            turnTargetToTheRight = playerTurn.transform.localRotation;

            if(turnTargetToTheRight.y > 0.99f)
            {
                turnTargetToTheRight = Quaternion.Euler(0, 0, 0);
            }else
            {
                turnTargetToTheRight = Quaternion.Euler(0, 180, 0);
            }
        }

        
    }

    public void Crashed()
    {
        
        GameObject explosion = Instantiate(destructionEffects[0], player.transform.position, player.transform.rotation);
        explosion.transform.localScale = new Vector3(10, 10, 10);
        explosion.name = "PlayerExplosion";

        if(destructionEffects[1] != null)
        {
            GameObject decals = GameObject.Find("Decals");
            if(decals == null)
            {
                decals = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
                decals.name = "Decals";
            }
           
            float groundHeight = terrain.GetHeightOfTerrain(player.transform.position);
           
            GameObject crashDecal = Instantiate(destructionEffects[1], new Vector3(player.transform.position.x, groundHeight+0.05f, player.transform.position.z), Quaternion.Euler(90, 0, 0));
           
            crashDecal.transform.forward = terrain.GetTerrainNormal(player.transform.position) * -1f;
            crashDecal.transform.localScale = new Vector3(20, 20, 20);
            crashDecal.transform.parent = decals.transform;
            crashDecal.name = "PlayerExplosionDecal";

            
        }

        Destroy(player);

        StartCoroutine(RespawnAfterTime(8));
    }

    IEnumerator RespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
    
        StartNewPlayer();
    }

}
