using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public GameObject bulletHitExplosion;

    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Enemy"))
        {
            Instantiate(bulletHitExplosion, gameObject.transform.position, Quaternion.identity);
        }
    }
}
