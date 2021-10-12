using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Maxmax")]
public class OpponentMaxmax : OpponentMinimax
{
    protected override int Heuristic(Board board, bool player){
        int legalMoves = board.GetLegalMoves(player).Count;
        int score = player == this.color ? legalMoves : -legalMoves;
        return score;
    }
}
