using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DiskStruct{
    public bool isPlaced;
    public bool color;
}

public struct Move{
    public Vector2Int position;
    public List<Vector2Int> flips;

    public void AddFlip(Vector2Int newFlip){
        if(flips == null)
            ClearFlips();
        if(flips.Contains(newFlip)){    //Make sure we dont flip the same disk more than once.
            return;
        }
        flips.Add(newFlip);
    }
    
    public void ClearFlips(){
        flips = new List<Vector2Int>();
    }
}

public class Board{
    private DiskStruct[,] board;
    private int boardSize;

    private readonly Vector2Int[] directions = new Vector2Int[]{
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
    };

    public Board(int size, DiskStruct[,] configuration = null){
        if(configuration != null)
            this.board = configuration;
        else
            this.board = new DiskStruct[size,size];
        this.boardSize = size;
    }

    public void GetScore(out int white, out int black){
        black = 0;
        white = 0;
        for (int y = 0; y < boardSize; y++){
            for (int x = 0; x < boardSize; x++){
                if(board[x,y].isPlaced){
                    if(board[x,y].color)
                        black++;
                    else
                        white++;
                }
            }
        }
    }

    public bool TryPlaceDisk(Vector2Int position, bool color, out Move move, bool force = false){
        move = new Move();
        if(!IsWithinBoard(position))
            return false;
        if(this.board[position.x, position.y].isPlaced)
            return false;

        if(!force && !IsLegalMove(position, color, ref move))
            return false;

        this.board[position.x, position.y].isPlaced = true;
        this.board[position.x, position.y].color = color;
        return true;
    }

    public bool TryFlipDisk(Vector2Int position){
        if(!IsWithinBoard(position))
            return false;
        if(!this.board[position.x, position.y].isPlaced)
            return false;

        this.board[position.x, position.y].color = !this.board[position.x, position.y].color;
        return true;
    }

    public void RemoveDisk(Vector2Int position){
        if(!IsWithinBoard(position))
            return;
        this.board[position.x, position.y].isPlaced = false;
    }

    public List<Move> GetLegalMoves(bool color){
        List<Move> legalMoves = new List<Move>();
        for (int y = 0; y < boardSize; y++){
            for (int x = 0; x < boardSize; x++){
                if(board[x,y].isPlaced)  //cell already contains disk.
                    continue;

                Vector2Int currentPos = new Vector2Int(x,y);
                Move move = new Move();
                if(IsLegalMove(currentPos, color, ref move)){
                    move.position = currentPos;
                    legalMoves.Add(move);
                }
            }
        }
        return legalMoves;
    }

    public void Search(Vector2Int position, bool color, int maxDepth, int currentDepth = 0){
        if(currentDepth > maxDepth)
            return;

        List<Move> legalMoves = this.GetLegalMoves(color);
        Move bestMove = new Move();
        for (int i = 0; i < legalMoves.Count; i++){
            Board newBoard = new Board(this.boardSize, this.board);
            Move move;
            if(newBoard.TryPlaceDisk(legalMoves[i].position, color, out move)){
                if(move.flips.Count > bestMove.flips.Count){
                    bestMove = move;
                    newBoard.Search(move.position, !color, maxDepth, currentDepth + 1);
                }
            }
        }
    }

    public bool IsLegalMove(Vector2Int position, bool color, ref Move move){
        bool isLegal = false;
        Move tempMove = new Move();
        for (int i = 0; i < directions.Length; i++){
            Vector2Int previousPos = position - directions[i];
            if(EvaluateDirection(position, previousPos, color, 0, ref tempMove)){
                for (int j = 0; j < tempMove.flips.Count; j++){
                    move.AddFlip(tempMove.flips[j]);
                }
                isLegal = true;
            }
        }
        return isLegal;
    }

    private bool EvaluateDirection(Vector2Int currentPos, Vector2Int previousPos, bool originalColor, int depth, ref Move move){
        Vector2Int nextPos = currentPos + (currentPos - previousPos); //new Vector2Int(0, -1);
        if(!IsWithinBoard(nextPos)){
            // Debug.Log($"{nextPos} is not within board. Current: {currentPos}, Previous: {previousPos}, Next: {nextPos}");
            move.ClearFlips();
            return false;
        }
        
        if(!board[nextPos.x, nextPos.y].isPlaced){
            // Debug.Log("Reached a dead end.");
            move.ClearFlips();
            return false;
        }

        if(board[nextPos.x, nextPos.y].color == originalColor && depth >= 1){
            // Debug.Log("Color was same and depth was more than or equal 1!");
            return true;
        }

        if(board[nextPos.x, nextPos.y].color == originalColor && depth == 0){
            // Debug.Log("Color was same and depth was more 0");
            move.ClearFlips();
            return false;
        }

        // Debug.Log("Going down");
        move.AddFlip(nextPos);
        return EvaluateDirection(nextPos, currentPos, originalColor, depth + 1, ref move);
    }

    private bool IsWithinBoard(Vector2Int position){
        if(position.x >= boardSize || position.x < 0 || position.y >= boardSize || position.y < 0)
            return false;
        return true;
    }
}