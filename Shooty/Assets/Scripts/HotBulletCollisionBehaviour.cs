using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotBulletCollisionBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bulletStrikePrefab;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollision(Collision collision)
    {
        Debug.Log("player collision");
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("hotbullet trigger: " + collider.gameObject.name + " " + collider.gameObject.tag);

        
        if(collider.CompareTag("Player") || collider.CompareTag("Cloud"))
        {
            return;
        }

        Instantiate(bulletStrikePrefab, gameObject.transform.position, Quaternion.identity);
        //destroy the parent pivot
        Destroy(gameObject.transform.parent.gameObject);

    }
}
