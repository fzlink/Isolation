using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GridSizeChanger : MonoBehaviour
{
    public TMP_InputField xSizeInput;
    public TMP_InputField ySizeInput;

    private void Start()
    {
        StartCoroutine(MakeGrid());
    }

    private IEnumerator MakeGrid()
    {
        yield return new WaitForSeconds(0.25f);
        GameManager.Instance.ChangeGridSize(3, 3);
    }

    public void ChangeSize()
    {
        int xSize = Convert.ToInt32(xSizeInput.text);
        //int ySize = Convert.ToInt32(ySizeInput.text);
        GameManager.Instance.ChangeGridSize(xSize, xSize);
    }

}
