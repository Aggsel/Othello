using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings: ScriptableObject
{
    [SerializeField] private int boardSize;
    [SerializeField] private float cellSize;

    public int GetBoardSize(){
        return this.boardSize;
    }

    public float GetCellSize(){
        return cellSize;
    }
}
