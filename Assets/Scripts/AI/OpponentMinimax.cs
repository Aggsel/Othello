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

        int search = Search(board, searchDepth, this.color);
        // Debug.Log(@$"Current best move: {currentBestMove.position}. Best move flips: {currentBestMove.flips.Count}
        //             Best move without search: {legalMoves[maxFlipsIndex].position}. Flips: {maxFlips}");

        move = currentBestMove;
        return true;
    }

    private int Search(Board board, int depth, bool maximizingPlayer){
        List<Move> legalMoves = board.GetLegalMoves(maximizingPlayer);

        if(depth == 0 || legalMoves.Count == 0){
            int white, black;
            board.GetScore(out white, out black);
            int score = maximizingPlayer ? black - white : white - black;
            // Debug.Log($"Getting score {score}."); // Black: {black} White: {white}. Reason: depth = {depth}, legalMoves = {legalMoves.Count}");
            return score;
        }

        Board newBoard = new Board(board.boardSize, board.board);
        
        int bestMove = maximizingPlayer == this.color ? int.MinValue : int.MaxValue;
        Move bestCurrentMove = new Move();
        for (int i = 0; i < legalMoves.Count; i++){
            Move move = legalMoves[i];

            newBoard.TryPlaceDisk(move.position, maximizingPlayer, ref move);
            int moveValue = Search(newBoard, depth-1, !maximizingPlayer);
            newBoard.UndoMove(move);

            if(maximizingPlayer == this.color)
                bestMove = Mathf.Max(moveValue, bestMove);
            else
                bestMove = Mathf.Min(moveValue, bestMove);

            if(moveValue == bestMove)
                bestCurrentMove = legalMoves[i];
        }

        if(depth == searchDepth && maximizingPlayer == this.color)
            this.currentBestMove = bestCurrentMove;

        // if(maximizingPlayer == this.color)
        //     Debug.Log($"Best move depth {depth}, Pos: {bestCurrentMove.position}, Flips {bestCurrentMove.flips.Count}");
        // else
        //     Debug.Log($"Best move depth {depth}, Pos: {bestCurrentMove.position}, Flips -{bestCurrentMove.flips.Count}");
        return bestMove;
    }
}
