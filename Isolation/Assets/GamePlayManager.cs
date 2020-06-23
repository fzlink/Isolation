using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public enum Gameplay
{
    AI = 0,
    Random = 1,
    Player = 2
}

public class GamePlayManager : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    void Start()
    {
        GameManager.Instance.ChangeGamePlay((Gameplay)dropdown.value);
        dropdown.onValueChanged.AddListener(x => ChangeGameplay(x));
    }

    private void ChangeGameplay(int x)
    {
        GameManager.Instance.ChangeGamePlay((Gameplay)x);
    }
}
