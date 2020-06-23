using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

public enum PlayerNum
{
    Player1 = 1,
    Player2 = 2
}

public class PlayerPawn : MonoBehaviour
{
    private Gameplay currentGameplay;
    private bool hasStartedPlaying;
    public PlayerNum playerNum;
    public Vector3Int CurrentTilePos { get; set; } = Vector3Int.zero;
    public List<Vector3Int> availableMoves;

    private void Start()
    {
        GameManager.Instance.onGridSizeChanged += onGridSizeChanged;
        GameManager.Instance.onTurnChanged += onTurnChanged;
        if (playerNum == PlayerNum.Player2)
            GameManager.Instance.onGameplayChanged += ChangeGameplay;
        else if (playerNum == PlayerNum.Player1)
            ChangeGameplay(Gameplay.Player);
        
        onGridSizeChanged(3, 3);
    }

    private void onTurnChanged(PlayerPawn obj)
    {
        if(obj == this)
        {
            if (GameManager.isGameOver) return;
            StartCoroutine(TryMove());
        }
    }

    private void ChangeGameplay(Gameplay gameplay)
    {
        currentGameplay = gameplay;
    }

    private void onGridSizeChanged(int x, int y)
    {
        hasStartedPlaying = false;
        transform.position = new Vector3(-1000, 0, 0);
        availableMoves = new List<Vector3Int>();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                availableMoves.Add(new Vector3Int(i, j, 0));
            }
        }
    }

    public void Move(Vector3 pos,Vector3Int tilePos)
    {
        if (GameManager.isGameOver) return;
        if(currentGameplay == Gameplay.Player)
        {
            CurrentTilePos = tilePos;
            transform.position = pos;
            hasStartedPlaying = true;
            GameManager.Instance.TurnPlayed(tilePos);
        }
    }

    public IEnumerator TryMove()
    {
        yield return new WaitForSeconds(1f);
        if (currentGameplay == Gameplay.AI)
        {
            AIMove();
        }
        else if (currentGameplay == Gameplay.Random)
        {
            RandomMove();
        }
    }

    private void RandomMove()
    {
        if(availableMoves.Count> 0)
        {
            int ind = UnityEngine.Random.Range(0, availableMoves.Count);
            Vector3Int randomPos = availableMoves[ind];
            CurrentTilePos = randomPos;
            transform.position = GameBoard.tileMap.GetCellCenterWorld(randomPos);
            hasStartedPlaying = true;
            GameManager.Instance.TurnPlayed(randomPos);
        }

    }

    private void AIMove()
    {

    }

    public void UpdateAvaliableMoves(Tilemap tileMap, GameGrid gameGrid)
    {
        availableMoves = new List<Vector3Int>();
        int rows = gameGrid.Grid.GetLength(0);
        int cols = gameGrid.Grid.GetLength(1);
        if (!hasStartedPlaying)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (!gameGrid.Grid[i, j].isExplored)
                        availableMoves.Add(new Vector3Int(i, j, 0));
                }
            }
            return;
        }

        int x, y;
        //Up
        Vector3Int pos = CurrentTilePos;
        for (y = pos.y+1; y < rows; y++)
        {
            Vector3Int posc = new Vector3Int(pos.x, y, 0);
            if (!gameGrid.Grid[pos.x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Down
        for (y = pos.y - 1; y >= 0; y--)
        {
            Vector3Int posc = new Vector3Int(pos.x, y, 0);
            if (!gameGrid.Grid[pos.x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Right
        for (x = pos.x+1; x < cols; x++)
        {
            Vector3Int posc = new Vector3Int(x, pos.y, 0);
            if (!gameGrid.Grid[x, pos.y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Left
        for (x = pos.x-1; x >= 0; x--)
        {
            Vector3Int posc = new Vector3Int(x, pos.y, 0);
            if (!gameGrid.Grid[x, pos.y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Up-Left
        y = pos.y;
        for(x = pos.x-1; x >= 0; x--)
        {
            y++;
            if (y >= rows)
                break;
            Vector3Int posc = new Vector3Int(x, y, 0);
            if (!gameGrid.Grid[x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Up-Right
        y = pos.y;
        for (x = pos.x + 1; x < cols; x++)
        {
            y++;
            if (y >= rows)
                break;
            Vector3Int posc = new Vector3Int(x, y, 0);
            if (!gameGrid.Grid[x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Down-Left
        y = pos.y;
        for (x = pos.x - 1; x >= 0; x--)
        {
            y--;
            if (y < 0)
                break;
            Vector3Int posc = new Vector3Int(x, y, 0);
            if (!gameGrid.Grid[x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
        //Down-Right
        y = pos.y;
        for (x = pos.x + 1; x < cols; x++)
        {
            y--;
            if (y < 0)
                break;
            Vector3Int posc = new Vector3Int(x, y, 0);
            if (!gameGrid.Grid[x, y].isExplored)
            {
                if (!availableMoves.Contains(posc))
                    availableMoves.Add(posc);
            }
            else
                break;
        }
    }

}
