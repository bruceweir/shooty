using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNight : MonoBehaviour
{
    public float dayCycleDurationMins = 1;
    private Light light; 
    private float durationSeconds;

    void Start()
    {
        light = gameObject.GetComponent<Light>();
        durationSeconds = dayCycleDurationMins * 60;
    }

    // Update is called once per frame
    void Update()
    {
        durationSeconds = dayCycleDurationMins * 60;
        gameObject.transform.Rotate(Vector3.right, Time.deltaTime * 360/durationSeconds);
    }
}
