using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPause = false;
    public GameObject pauseManuUI;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Resume()
    {
        pauseManuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPause = false;

    }

    void Pause()
    {
        pauseManuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPause = true;
    }
}
