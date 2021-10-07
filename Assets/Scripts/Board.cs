using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DiskStruct{
    public bool isPlaced;
    public bool color;
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

    public bool TryPlaceDisk(Vector2Int position, bool color, bool force = false){
        if(!IsWithinBoard(position))
            return false;
        if(this.board[position.x, position.y].isPlaced)
            return false;
        if(!force && !IsLegalMove(position, color))
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

    public List<Vector2Int> GetLegalMoves(bool color){
        List<Vector2Int> legalMoves = new List<Vector2Int>(); 
        for (int y = 0; y < boardSize; y++){
            for (int x = 0; x < boardSize; x++){
                if(board[x,y].isPlaced)  //cell already contains disk.
                    continue;

                Vector2Int currentPos = new Vector2Int(x,y);
                if(IsLegalMove(currentPos, color))
                    legalMoves.Add(currentPos);
            }
        }
        return legalMoves;
    }

    public bool IsLegalMove(Vector2Int position, bool color){
        for (int i = 0; i < directions.Length; i++){
            Vector2Int previousPos = position - directions[i];
            if(EvaluateDirection(position, previousPos, color, 0)){
                return true;
            }
        }
        return false;
    }

    private bool EvaluateDirection(Vector2Int currentPos, Vector2Int previousPos, bool originalColor, int depth){
        Vector2Int nextPos = currentPos + (currentPos - previousPos); //new Vector2Int(0, -1);
        if(!IsWithinBoard(nextPos)){
            // Debug.Log($"{nextPos} is not within board. Current: {currentPos}, Previous: {previousPos}, Next: {nextPos}");
            return false;
        }
        
        if(!board[nextPos.x, nextPos.y].isPlaced){
            // Debug.Log("Reached a dead end.");
            return false;
        }

        if(board[nextPos.x, nextPos.y].color == originalColor && depth >= 1){
            // Debug.Log("Color was same and depth was more than or equal 1!");
            return true;
        }

        if(board[nextPos.x, nextPos.y].color == originalColor && depth == 0){
            // Debug.Log("Color was same and depth was more 0");
            return false;
        }

        // Debug.Log("Going down");
        return EvaluateDirection(nextPos, currentPos, originalColor, depth + 1);
    }

    private bool IsWithinBoard(Vector2Int position){
        if(position.x >= boardSize || position.x < 0 || position.y >= boardSize || position.y < 0)
            return false;
        return true;
    }
}