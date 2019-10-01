using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	public static GameManager Instance { get; private set; }

	public GameObject terrainGenerationPrefab;
	public GameObject playerVehiclePrefab;
	private GameObject player;
	private GeneratedTerrain terrain = null;
	private GameObject scoreTextObject;
	private TMP_Text scoreText;
	private Animator scoreAnimator;
	public Camera overheadCamera;
	public Camera sideFollowCamera;


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
		CreateTerrain();

		StartCoroutine(SpawnPlayer(0));
		// CreatePlayer();

		// SetCameraToFollow(player);

		// SwitchToFollowCamera();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CreateTerrain()
	{
		if(terrain != null)
		{
			Destroy(terrain);
		}

		GameObject t = Instantiate(terrainGenerationPrefab, Vector3.zero, Quaternion.identity);
		t.name = "Terrain";
		terrain = t.GetComponent<GeneratedTerrain>();
	}

	private void CreatePlayer()
	{
		if(player != null)
		{
			Destroy(player);
		}
		if(terrain == null)
		{
			CreateTerrain();
		}
		

		player = Instantiate(playerVehiclePrefab, Vector3.zero, Quaternion.identity);
		//player.GetComponent<ControlJet>().terrain = terrain;
	}

	private void SetCameraToFollow(GameObject targetGameObject)
	{
		ControlCamera controlCamera = sideFollowCamera.GetComponent<ControlCamera>();
		controlCamera.targetGameObject = targetGameObject;
		controlCamera.terrain = terrain;

	}

	private void SwitchToFollowCamera()
	{
		overheadCamera.enabled = false;
		sideFollowCamera.enabled = true;
	}

	private void SwitchToOverheadCamera()
	{
		overheadCamera.enabled = true;
		sideFollowCamera.enabled = false;
	}

	private IEnumerator SpawnPlayer(float delay)
	{
		yield return new WaitForSeconds(delay);

		CreatePlayer();

		SetCameraToFollow(player);

		SwitchToFollowCamera();

	}

	public void PlayerDestroyed()
	{
		StartCoroutine(SpawnPlayer(8));
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
