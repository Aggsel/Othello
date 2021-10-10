using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings: ScriptableObject
{
    [SerializeField] private int boardSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float gameSpeed = 1.0f;
    [SerializeField] public bool disableWinnerPrompt = false;

    public int GetBoardSize(){
        return this.boardSize;
    }

    public float GetCellSize(){
        return cellSize;
    }

    public float GetGameSpeed(){
        return gameSpeed;
    }

    public void SetGameSpeed(float newSpeed){
        this.gameSpeed = newSpeed;
    }

    public void StartNewGameWithoutPrompt(bool value){
        disableWinnerPrompt = value;
    }

}
