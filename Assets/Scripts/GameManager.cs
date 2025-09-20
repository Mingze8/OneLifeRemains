using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for global access
    public static GameManager Instance;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Check if there's already an instance of GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Optional: Don't destroy this object when loading new scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate GameManager objects
        }
    }

    // Method to start coroutines from anywhere in the game
    public void StartMyCoroutine(IEnumerator coroutine)
    {
        Debug.Log("Requested Coroutine");
        StartCoroutine(coroutine);
    }
}
