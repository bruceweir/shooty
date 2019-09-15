using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltimeterControl : MonoBehaviour
{
    public GameObject ObjectToDisplay;
    private GameObject littleHand;
    private GameObject bigHand;
    private float zeroAngle;


    // Start is called before the first frame update
    void Start()
    {
        littleHand = GameObject.Find("AltimeterLittleHand");
        bigHand = GameObject.Find("AltimeterBigHand");

        zeroAngle = 86.38f;

        SetAltimeter(0);

    }

    // Update is called once per frame
    void Update()
    {
        SetAltimeter(ObjectToDisplay.transform.position.y);
    }

    public void SetAltimeter(float altitude)
    {

        float thousandMetres = altitude / 1000f;
        float metres = altitude - (Mathf.Floor(thousandMetres) * 1000);

        float metresAngle = zeroAngle - ((metres / 1000) * 360);
        bigHand.transform.rotation = Quaternion.Euler(0, 0, metresAngle);

        float thousandMetresAngle = zeroAngle - (thousandMetres/10 * 360);
        littleHand.transform.rotation = Quaternion.Euler(0, 0, thousandMetresAngle);

    }
}
