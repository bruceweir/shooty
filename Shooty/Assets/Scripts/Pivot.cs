using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : MonoBehaviour
{
    // Start is called before the first frame update
    public float angularVelocity;
    void Awake()
    {
        InitialisePivot();
    }

    public void InitialisePivot()
    {
        gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    void FixedUpdate()
    {
        Debug.Log(angularVelocity);
        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);
    }

    public void SetHeight(float height)
    {
        Vector3 position = gameObject.transform.position;
        position.y = height;
        gameObject.transform.position = position;

    }
    public void AdjustHeight(float height)
    {
        Vector3 position = gameObject.transform.position;
        position.y += height;
        gameObject.transform.position = position;

//        Debug.Log(gameObject.transform.position.y);

    }
}
