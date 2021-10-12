using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Minimax")]
public class OpponentMinimax : BaseOpponent
{
    private Move currentBestMove;
    [SerializeField] private int searchDepth = 5;

    public override bool GetMove(Board board, out Move move){
        List<Move> legalMoves = board.GetLegalMoves(this.color);
        move = new Move();
        this.currentBestMove = new Move();

        if(legalMoves.Count == 0)
            return false;

        int maxFlips = legalMoves[0].flips.Count;
        int maxFlipsIndex = 0;
        for (int i = 1; i < legalMoves.Count; i++){
            if(legalMoves[i].flips.Count > maxFlips){
                maxFlips = legalMoves[i].flips.Count;
                maxFlipsIndex = i;
            }
        }

        int search = Search(board, searchDepth, int.MinValue, int.MaxValue, this.color);

        move = currentBestMove;
        return true;
    }

    protected virtual int Heuristic(Board board, bool player){
        int white, black;
        board.GetScore(out white, out black);
        int score = player ? black - white : white - black;
        return score;
    }

    private int Search(Board board, int depth, int alpha, int beta, bool maximizingPlayer){
        List<Move> legalMoves = board.GetLegalMoves(maximizingPlayer);

        if(depth == 0 || legalMoves.Count == 0)
            return Heuristic(board, maximizingPlayer);

        Board newBoard = new Board(board.boardSize, board.board);
        
        int bestMove = maximizingPlayer == this.color ? int.MinValue : int.MaxValue;
        Move bestCurrentMove = new Move();
        for (int i = 0; i < legalMoves.Count; i++){
            Move move = legalMoves[i];

            newBoard.TryPlaceDisk(move.position, maximizingPlayer, ref move);
            int moveValue = Search(newBoard, depth-1, alpha, beta, !maximizingPlayer);
            newBoard.UndoMove(move);

            if(maximizingPlayer == this.color){
                bestMove = Mathf.Max(moveValue, bestMove);
                if(bestMove >= beta)
                    break;
                alpha = Mathf.Max(alpha, bestMove);
            }
            else{
                bestMove = Mathf.Min(moveValue, bestMove);
                if(bestMove <= alpha)
                    break;
                beta = Mathf.Min(beta, bestMove);
            }

            if(moveValue == bestMove)
                bestCurrentMove = legalMoves[i];
        }

        if(depth == searchDepth && maximizingPlayer == this.color)
            this.currentBestMove = bestCurrentMove;
        return bestMove;
    }
}
