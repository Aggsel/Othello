using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseOpponent : ScriptableObject
{
    [SerializeField] protected bool color = true;

    public void SetColor(bool color){
        this.color = color;
    }

    /// <summary>
    /// Returns false if no legal move was possible.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    public virtual bool GetMove(Board board, out Move move){
        move = new Move();
        return false;
    }
}
