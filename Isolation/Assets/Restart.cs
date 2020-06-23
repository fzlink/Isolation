using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public GameObject restartButton;

    void Start()
    {
        restartButton.SetActive(false);
        GameManager.Instance.onEndState += ShowRestartButton;
    }

    private void ShowRestartButton(EndState endState)
    {
        restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
