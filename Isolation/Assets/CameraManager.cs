using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform backGround;

    private void Start()
    {
        GameManager.Instance.onGridSizeChanged += ChangeCameraSize;
    }

    private void ChangeCameraSize(int x, int y)
    {
        transform.position = new Vector3(x / 2f + 0.5f, y / 2f, -10);
        backGround.position = new Vector3(x / 2f, y / 2f, 1);
    }
}
