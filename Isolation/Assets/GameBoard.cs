using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EndState
{
    Player1Won = 1,
    Player2Won = 2,
    Draw = 3
}

public class GameBoard : MonoBehaviour
{
    public static Tilemap tileMap;
    private bool isPlayer1Turn;
    private GameGrid gameGrid = new GameGrid();

    public PlayerPawn player1;
    public PlayerPawn player2;

    private void Awake()
    {
        tileMap = GetComponentInChildren<Tilemap>();
    }

    private void Start()
    {
        GameManager.Instance.onGridSizeChanged += ChangeGridSize;
        GameManager.Instance.onGameStarted += StartGame;
        GameManager.Instance.OnInput += CheckInput;
        GameManager.Instance.onTurnPlayed += TurnPlayed;
    }

    private void ChangeGridSize(int width, int height)
    {
        gameGrid.ChangeGridSize(width, height);
    }

    private void StartGame()
    {
        isPlayer1Turn = true;
    }

    private void CheckInput(Vector3 pos)
    {
        Vector3Int tilePos = tileMap.WorldToCell(pos);
        Vector3 cellCenterWorld = tileMap.GetCellCenterWorld(tilePos);
        TileBase tile = tileMap.GetTile(tilePos);
        if (tile != null)
        {
            Debug.Log(string.Format("Tile is: {0}", tile.name));

            if (CheckIsValidMove(tilePos))
            {
                PlayPawn(cellCenterWorld,tilePos);
            }
        }
    }

    public void TurnPlayed(Vector3Int tilePos)
    {
        ExploreTile(tilePos);
        UpdateAvailableMoves();
        if(!GameManager.isGameOver)
            ChangeTurn();
    }

    private void UpdateAvailableMoves()
    {
        player1.UpdateAvaliableMoves(tileMap,gameGrid);
        player2.UpdateAvaliableMoves(tileMap,gameGrid);
        if(player1.availableMoves.Count == 0  && player2.availableMoves.Count == 0)
        {
            //Draw
            GameManager.Instance.BroadcastEndState(EndState.Draw);
        }
        else if(player1.availableMoves.Count == 0)
        {
            //Player2 Win
            GameManager.Instance.BroadcastEndState(EndState.Player2Won);
        }
        else if(player2.availableMoves.Count == 0)
        {
            //Player1 Win
            GameManager.Instance.BroadcastEndState(EndState.Player1Won);
        }
    }

    private void ChangeTurn()
    {
        if (isPlayer1Turn)
        {
            isPlayer1Turn = false;
            GameManager.Instance.BroadcastChangeTurn(player2);
        }
        else
        {
            isPlayer1Turn = true;
            GameManager.Instance.BroadcastChangeTurn(player1);
        }
    }

    private bool CheckIsValidMove(Vector3Int tilePos)
    {

        if (isPlayer1Turn)
        {
            if (player1.availableMoves.Contains(tilePos))
                return true;
        }
        else
        {
            if (player2.availableMoves.Contains(tilePos))
                return true;
        }
        return false;
            
        /*
        Vector3 playerPos;
        if (isPlayer1Turn)
            playerPos = player1.CurrentPos;
        else
            playerPos = player2.CurrentPos;
        Vector3Int playerTilePos = tileMap.WorldToCell(playerPos);
        if (playerPos == tilePos)
            return false;
        int x = Mathf.Abs(playerTilePos.x - tilePos.x);
        int y = Mathf.Abs(playerTilePos.y - tilePos.y);
        Vector3Int offset = Vector3Int.zero;
        if(x > 0 && y > 0)
        {

        }
        else if(x > 0)
        {

        }
        else if(y > 0)
        {

        }
        for (Vector3Int pos = playerTilePos; pos != tilePos; )
        {

        }*/
    }

    private void ExploreTile(Vector3Int tilePos)
    {
        gameGrid.MarkExplored(tilePos);
        tileMap.SetColor(tilePos, Color.blue);
    }

    private void PlayPawn(Vector3 pos, Vector3Int tilePos)
    {
        if (isPlayer1Turn)
        {
            player1.Move(pos, tilePos);
        }
        else
        {
            player2.Move(pos, tilePos);
        }
    }
}

public class GameGrid
{
    public GameSquare[,] Grid { get; set; }

    public GameGrid(int width, int height)
    {
        ChangeGridSize(width, height);
    }

    public GameGrid()
    {

    }

    public void ChangeGridSize(int width, int height)
    {
        Grid = new GameSquare[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Grid[i, j] = new GameSquare();
            }
        }
    } 

    public bool IsValid(Vector3Int pos)
    {
        if (Grid[pos.x, pos.y].isExplored)
            return false;
        else
            return true;
    }

    public void MarkExplored(Vector3Int pos)
    {
        Grid[pos.x, pos.y].isExplored = true;
    }
}

public class GameSquare
{
    public bool isExplored;

    public GameSquare()
    {
        this.isExplored = false;
    }
}
