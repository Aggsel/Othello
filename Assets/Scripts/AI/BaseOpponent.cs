using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseOpponent : ScriptableObject
{
    protected bool color = true;

    public void SetColor(bool color){
        this.color = color;
    }

    public virtual bool GetMove(Board board, out Move move){
        move = new Move();
        return false;
    }
}
