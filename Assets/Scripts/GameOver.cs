using UnityEngine;
using System.Collections;

/// <summary>
/// Return to main menu
/// </summary>
public class GameOver : MonoBehaviour {

	public void EndGame()
    {
        Application.LoadLevel("MainMenu");
    }
}
