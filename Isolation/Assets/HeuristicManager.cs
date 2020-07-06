using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum HeuristicType
{
    Simple = 0,
    Defensive = 1,
    Offensive = 2
}

public class HeuristicManager
{

    public static float Heuristic(int playerMoves, int opponentMoves)
    {
        HeuristicType hT = GameManager.Instance.heuristicType;
        if (hT == HeuristicType.Simple)
        {
            return Simple(playerMoves);
        }
        else if (hT == HeuristicType.Defensive)
        {
            return Defensive(playerMoves, opponentMoves);
        }
        else
            return Offensive(playerMoves, opponentMoves);
    }

    public static float Simple(int playerMoves)
    {
        return playerMoves;
    }

    public static float Defensive(int playerMoves, int opponentMoves)
    {
        return (playerMoves * 2) - opponentMoves;
    }

    public static float Offensive(int playerMoves, int opponentMoves)
    {
        return playerMoves - (opponentMoves * 2);
    }
}

