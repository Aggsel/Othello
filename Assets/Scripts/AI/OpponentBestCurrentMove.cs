using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Best Current Move")]
public class OpponentBestCurrentMove : BaseOpponent
{
    public override bool GetMove(Board board, out Move move){
        List<Move> legalMoves = board.GetLegalMoves(this.color);
        move = new Move();

        //Play only if possible.
        if(legalMoves.Count == 0)
            return false;
        
            int bestCurrentMoveIndex = 0;
            int bestCurrentMove = -1;
            for (int i = 0; i < legalMoves.Count; i++){
                int flips = legalMoves[i].flips.Count;
                if(flips > bestCurrentMove){
                    bestCurrentMove = flips;
                    bestCurrentMoveIndex = i;
                }
            }

            if(bestCurrentMove == -1)
                return false;

        move = legalMoves[bestCurrentMoveIndex];
        return true;
    }
}
