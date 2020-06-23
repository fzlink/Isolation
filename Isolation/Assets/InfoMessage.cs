using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoMessage : MonoBehaviour
{
    private TMP_Text textComp;

    private void Awake()
    {
        textComp = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.onGameStarted += PrintMessagePlayer1;
        GameManager.Instance.onTurnChanged += onTurnChanged;
        GameManager.Instance.onEndState += onEndState;
    }

    private void onEndState(EndState endState)
    {
        if(endState == EndState.Player1Won)
            textComp.text = "Player 1 Win!";
        else if(endState == EndState.Player2Won)
            textComp.text = "Player 2 Win!";
        else if (endState == EndState.Draw)
            textComp.text = "Draw!";

    }

    private void onTurnChanged(PlayerPawn player)
    {
        if(player.playerNum == PlayerNum.Player1)
            textComp.text = "Player 1's turn";
        else
            textComp.text = "Player 2's turn";
    }

    private void PrintMessagePlayer1()
    {
        textComp.text = "Player 1's turn";
    }
}
