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
    public PlayerPawn opponent;
    private Node bestState;

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
        Node root = MakeTree();
        Minimax(root,GameManager.Instance.minimaxDepth,-Mathf.Infinity, Mathf.Infinity,true);
        if(bestState != null)
        {
            if(bestState.nextMovesBelongsTo == PlayerNum.Player1)
                CurrentTilePos = bestState.opponentPos;
            else
                CurrentTilePos = bestState.activePlayerPos;
            transform.position = GameBoard.tileMap.GetCellCenterWorld(CurrentTilePos);
            hasStartedPlaying = true;
            GameManager.Instance.TurnPlayed(CurrentTilePos);
        }
    }

    private float Minimax(Node node, int depth, float alpha, float beta, bool isMaximizingPlayer)
    {
        if (depth == 0 || node.moves.Count <= 0)
        {
            return node.score;
        }


        if (isMaximizingPlayer)
        {
            float maxEval = -Mathf.Infinity;
            foreach (Node item in node.children)
            {
                float eval = Minimax(item, depth - 1, alpha, beta, false);
                if(eval > maxEval)
                {
                    maxEval = eval;
                    if(depth == GameManager.Instance.minimaxDepth)
                        bestState = item;
                }
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }

            return maxEval;
        }
        else
        {
            float minEval = Mathf.Infinity;
            foreach (Node item in node.children)
            {
                float eval = Minimax(item, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }

            return minEval;
        }

    }

    private Node MakeTree()
    {
        GameGrid realGrid = GameBoard.gameGrid;
        GameGrid virtualGrid = new GameGrid();
        virtualGrid.Grid = GameGrid.CloneGrid(realGrid.Grid);

        Vector3Int opponentPos = opponent.CurrentTilePos;
        PlayerNum nextMovesBelongsTo;

        Queue<Node> list = new Queue<Node>();

        Node root = new Node(virtualGrid, new List<Node>(),HeuristicManager.Heuristic(availableMoves.Count, opponent.availableMoves.Count) , 0, availableMoves,CurrentTilePos, opponentPos, PlayerNum.Player2);
        list.Enqueue(root);

        while (list.Count > 0)
        {
            Node node = list.Dequeue();
            if (node.depth % 2 == 0)
                nextMovesBelongsTo = PlayerNum.Player1;
            else
                nextMovesBelongsTo = PlayerNum.Player2;

            if (node.depth <= GameManager.Instance.treeDepth && node.moves.Count > 0)
            {
                foreach (Vector3Int item in node.moves)
                {
                    GameGrid newGameGrid = new GameGrid();
                    newGameGrid.Grid = GameGrid.CloneGrid(node.gameGrid.Grid);

                    Vector3Int activePlayerPos = item;
                    newGameGrid.Grid[item.x, item.y].isExplored = true;

                    List<Vector3Int> nextMoves = GetAvailableMoves(newGameGrid,node.opponentPos);
                    List<Vector3Int> thisMoves = GetAvailableMoves(newGameGrid, activePlayerPos);
                    Node child = new Node(newGameGrid,new List<Node>(),HeuristicManager.Heuristic(thisMoves.Count,nextMoves.Count),node.depth+1,nextMoves, node.opponentPos,activePlayerPos, nextMovesBelongsTo);
                    node.children.Add(child);
                    list.Enqueue(child);
                }
            }
        }
        return root;
    }


    public List<Vector3Int> GetAvailableMoves(GameGrid gameGrid, Vector3Int currentPos)
    {
        availableMoves = new List<Vector3Int>();
        int rows = gameGrid.Grid.GetLength(0);
        int cols = gameGrid.Grid.GetLength(1);

        int x, y;
        //Up
        Vector3Int pos = currentPos;
        for (y = pos.y + 1; y < rows; y++)
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
        for (x = pos.x + 1; x < cols; x++)
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
        for (x = pos.x - 1; x >= 0; x--)
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
        for (x = pos.x - 1; x >= 0; x--)
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
        return availableMoves;
    }

    public void UpdateAvaliableMoves(GameGrid gameGrid)
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

public class Node
{
    public GameGrid gameGrid;
    public List<Node> children;
    public float baseScore;
    public float score;
    public int depth;
    public List<Vector3Int> moves;
    public Vector3Int activePlayerPos;
    public Vector3Int opponentPos;
    public PlayerNum nextMovesBelongsTo;


    public Node(GameGrid gameGrid, List<Node> children, float score, int depth, List<Vector3Int> moves, Vector3Int activePlayerPos, Vector3Int opponentPos, PlayerNum nextMovesBelongsTo)
    {
        this.gameGrid = gameGrid;
        this.children = children;
        this.score = score;
        this.depth = depth;
        this.moves = moves;
        this.activePlayerPos = activePlayerPos;
        this.opponentPos = opponentPos;
        this.nextMovesBelongsTo = nextMovesBelongsTo;
    }
}