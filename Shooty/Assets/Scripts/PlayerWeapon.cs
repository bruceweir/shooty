using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject weaponPrefab;
    private Weapon weapon;
    void Start()
    {
        GameObject w = Instantiate(weaponPrefab, gameObject.transform.position, gameObject.transform.rotation);
        w.transform.parent = gameObject.transform;
//        w.transform.position = gameObject.transform.position;
//        w.transform.rotation = Quaternion.identity;

        weapon = w.GetComponent<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            weapon.Fire();
        }
    }
}
