using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Worst Move")]
public class OpponentWorst : BaseOpponent
{
    public override bool GetMove(Board board, out Move move){
        List<Move> legalMoves = board.GetLegalMoves(this.color);
        move = new Move();

        //Play only if possible.
        if(legalMoves.Count == 0)
            return false;

        int worstMove = legalMoves[0].flips.Count;
        int worstMoveIndex = 0;
        for (int i = 1; i < legalMoves.Count; i++){
            if(legalMoves[i].flips.Count < worstMove){
                worstMove = legalMoves[i].flips.Count;
                worstMoveIndex = i;
            }
        }

        move = legalMoves[worstMoveIndex];
        return true;
    }
}
