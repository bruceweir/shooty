using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnChecker : MonoBehaviour {

	int currentCollisions = 0;
	void OnTriggerEnter2D(Collider2D other) 
	{
		currentCollisions+=1;
	}
	void OnTriggerExit2D(Collider2D other) 
	{
		currentCollisions-=1;
	}
	
	public bool SpawnClear()
	{
		return currentCollisions == 0;
	}

	
}
