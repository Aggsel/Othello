using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AI/Minimax")]
public class OpponentMinimax : BaseOpponent
{
    private Move currentBestMove;
    private int searchDepth = 1;

    public override bool GetMove(Board board, out Move move){
        List<Move> legalMoves = board.GetLegalMoves(this.color);
        move = new Move();
        this.currentBestMove = new Move();

        if(legalMoves.Count == 0)
            return false;


        int maxFlips = -100;
        int maxFlipsIndex = -100;
        for (int i = 0; i < legalMoves.Count; i++){
            if(legalMoves[i].flips.Count > maxFlips){
                maxFlips = legalMoves[i].flips.Count;
                maxFlipsIndex = i;
            }
        }

        int search = Search(board, searchDepth, true);
        Debug.Log($"Search finished, a score of {search} was found.");
        Debug.Log($"Current best move: {currentBestMove.position}. Best move flips: {currentBestMove.flips.Count}");
        Debug.Log($"Best move without search: {legalMoves[maxFlipsIndex].position}. Flips: {maxFlips}");

        move = currentBestMove;
        return true;
    }

    private int Search(Board board, int depth, bool maximizingPlayer){
        // Debug.Log($"Searching... Depth: {depth}");
        List<Move> legalMoves = board.GetLegalMoves(maximizingPlayer);

        if(depth == 0 || legalMoves.Count == 0){
            int white, black;
            board.GetScore(out white, out black);
            int score = maximizingPlayer ? black - white : white - black;
            // Debug.Log($"Getting score {score}."); // Black: {black} White: {white}. Reason: depth = {depth}, legalMoves = {legalMoves.Count}");
            return score;
        }

        Board newBoard = new Board(board.boardSize, board.board);
        
        int bestMove = maximizingPlayer ? int.MinValue : int.MaxValue;
        Move bestCurrentMove = new Move();
        for (int i = 0; i < legalMoves.Count; i++){
            Move move = legalMoves[i];

            newBoard.TryPlaceDisk(move.position, maximizingPlayer, ref move);
            // if(!newBoard.TryPlaceDisk(move.position, maximizingPlayer, ref move))
            //     Debug.Log($"Invalid move. {i}/{legalMoves.Count-1}.");
            // else
            //     Debug.Log($"Move was legal. {i}/{legalMoves.Count-1}");
            int moveValue = Search(newBoard, depth-1, !maximizingPlayer);
            newBoard.UndoMove(move);

            if(maximizingPlayer){
                bestMove = Mathf.Max(moveValue, bestMove);
                if(moveValue == bestMove){
                    bestCurrentMove = legalMoves[i];
                    // Debug.Log($"Changing best move in depth {depth}");
                }
            }
            else{
                bestMove = Mathf.Min(moveValue, bestMove);
                if(moveValue == bestMove){
                    bestCurrentMove = legalMoves[i];
                    // Debug.Log($"Changing best move in depth {depth}");
                }
            }
        }

        if(depth == searchDepth)
            this.currentBestMove = bestCurrentMove;

        return bestMove;
    }
}
