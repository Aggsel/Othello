using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Random Move")]
public class OpponentRandom : BaseOpponent
{
    public override bool GetMove(Board board, out Move move){
        List<Move> legalMoves = board.GetLegalMoves(this.color);
        move = new Move();

        //Play only if possible.
        if(legalMoves.Count == 0)
            return false;

        move = legalMoves[Random.Range(0, legalMoves.Count-1)];
        return true;
    }
}
