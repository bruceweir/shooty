using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	// Use this for initialization
	public float startingHealth = 1.0f;
	
	private float currentHealth;
	void Start()
	{
		currentHealth = startingHealth;
	}
	public void TakeDamage(float amount)
	{
		currentHealth -= amount;
	}

	public bool IsDamaged()
	{
		return currentHealth < startingHealth;
	}

	public bool IsDestroyed()
	{
		return currentHealth <= 0;
	}
}
