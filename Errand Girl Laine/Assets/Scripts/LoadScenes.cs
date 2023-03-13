using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour
{
    public bool win;
    public void PlayGame()
    {
        SceneManager.LoadScene("Nancy");
    }

    public void PlayGame_Enemy()
    {
        SceneManager.LoadScene("Ryan");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    
}
