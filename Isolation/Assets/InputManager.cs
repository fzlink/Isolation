using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{

    private bool canInput = true;
    private int fingerID = -1;

    private void Awake()
    {
#if !UNITY_EDITOR
    fingerID = 0; 
#endif
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;
        if (!canInput) return;
#if UNITY_ANDROID
        ProcessTouch();
#elif UNITY_EDITOR || UNITY_STANDALONE
        ProcessClick();
#endif
    }

    private void ProcessClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            CheckTile(pos);
        }
    }

    private void ProcessTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);
                //Ray ray = Camera.main.ScreenPointToRay(touch.position);
                CheckTile(pos);
            }
        }
    }

    private void CheckTile(Vector3 pos)
    {
        Debug.Log(string.Format("Co-ords of mouse is [X: {0} Y: {0}]", pos.x, pos.y));
        GameManager.Instance.BroadcastInput(pos);
    }

    public void SetCanInput(bool canInput)
    {
        this.canInput = canInput;
    }
}
