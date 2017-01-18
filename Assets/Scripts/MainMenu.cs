using UnityEngine;
using System.Collections;

/// <summary>
/// Behaviors for the main menu (Start the game)
/// </summary>
public class MainMenu : MonoBehaviour {

    /// <summary>
    /// Start the game
    /// </summary>
    public void StartGame()
    {
        Application.LoadLevel("AsteroidScene");
    }
}
