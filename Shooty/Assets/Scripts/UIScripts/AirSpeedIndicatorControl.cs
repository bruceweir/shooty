using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSpeedIndicatorControl : MonoBehaviour
{
    
    public ControlPlayer player;
    private GameObject airSpeedHand;
    private GameObject warningLight;
    private float angleMax = -311;
    public float stepSizePerSpeed = 10;

    void Start()
    {
        airSpeedHand = GameObject.Find("AirSpeedHand");
        warningLight = GameObject.Find("WarningLight");
    }

    // Update is called once per frame
    void Update()
    {
        SetAirspeedHand(player.playerSpeed);
        SetWarningLightState(player.playerSpeed);
    }

    public void SetAirspeedHand(float speed)
    {
        float angle = -stepSizePerSpeed * speed;

        airSpeedHand.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetWarningLightState(float speed)
    {
        
        CanvasRenderer rend = warningLight.GetComponent<CanvasRenderer>();
        
        if(player.GetFlightState() == FlightState.Stall)
        {
            rend.SetColor(Color.Lerp(Color.black, Color.red,  3 * Mathf.PingPong(Time.time, .3f)));
        }
        if(player.GetFlightState() == FlightState.Landing)
        {
            rend.SetColor(Color.Lerp(Color.black, Color.red,  Mathf.PingPong(Time.time, 2)));
        }
        if(player.GetFlightState() == FlightState.OK)
        {
            rend.SetColor(Color.white);//Color.Lerp(Color.red, Color.black, Time.deltaTime));
        }
        

    }
}
