using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFall : MonoBehaviour {

	// Use this for initialization

	public float fallDelay;
	private Rigidbody2D rb2d;
	void Start () {
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	void Fall()
	{
		rb2d.isKinematic = false;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			Invoke("Fall", fallDelay);
		}
	}
}
