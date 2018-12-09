using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {


	public float autoLoadNextLevelAfter;

	
	void Start(){
	if(autoLoadNextLevelAfter <= 0){
		Debug.Log ("Level Autoload disabled - use a positive number in seconds to get the next level to load after this time");
	}else {
		Invoke("LoadNextLevel", autoLoadNextLevelAfter);
	}
	}
	
	
	public void LoadLevel(string sceneName){
	Debug.Log("Level Load Requested for: " + name);
		DontDestroyPlayerController ();
		SceneManager.LoadScene(sceneName);
		//SceneManager.LoadScene(string scenePath)
	}
	
	public void QuitRequest(){
	Debug.Log ("Quit Request");
	Application.Quit();
	}
	
	public void LoadNextLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
		DontDestroyPlayerController ();
		//Application.LoadLevel(Application.loadedLevel +1);
	}
	
	private void DontDestroyPlayerController(){
		MyPlayerController[] playerControllers = FindObjectsOfType<MyPlayerController> ();

		foreach (MyPlayerController playerController in playerControllers) {
			DontDestroyOnLoad (playerController);
		}
	}



}