using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DiskStruct{
    public bool isPlaced;
    public bool color;
}

public class Board
{
    private DiskStruct[,] board;

    public Board(int size){
        this.board = new DiskStruct[size,size];
    }

    public void PlaceDisk(Vector2Int position, bool color){
        this.board[position.x, position.y].isPlaced = true;
        this.board[position.x, position.y].color = color;
    }

    public bool FlipDisk(Vector2Int position){
        if(!this.board[position.x, position.y].isPlaced)
            return false;

        this.board[position.x, position.y].color = !this.board[position.x, position.y].color;
        return true;
    }
}