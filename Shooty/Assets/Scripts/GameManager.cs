using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	public static GameManager Instance { get; private set; }

	private GameObject scoreTextObject;
	private TMP_Text scoreText;
	private Animator scoreAnimator;


	private int score=0;
	void Awake()
	{
		if (Instance == null) 
		{
			Instance = this; 
		}
		else 
		{ 
			Debug.Log("Warning: multiple " + this + " in scene!"); 
		}
	}
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateScoreBy(int value)
	{
		if(scoreTextObject == null) {
			scoreTextObject = GameObject.Find("ScoreText");

			if(scoreTextObject != null) {
				scoreText = scoreTextObject.GetComponent<TMP_Text>();
				scoreAnimator = scoreTextObject.GetComponent<Animator>();
			} else {
				Debug.Log("Could not find ScoreText");
			}
		}

		score += value;

		scoreText.text = "Score " + score.ToString();

		scoreAnimator.SetTrigger("Pop");
	}
}
