using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Figure
{
    
    public string figureName;
    public string abilityName;
    public List<Attack> pieces = new List<Attack>();

    public Figure(string figureName, List<Attack> pieces)
    {
        this.figureName = figureName;
        this.pieces = pieces;
    }

    //0~96の数字で指定されたところにあるAttackを返す
    public Attack GetAttack(int Number)
    {
        int sizeSum = 0;
        foreach(Attack piece in pieces)
        {
            sizeSum += piece.size;
            if (Number < sizeSum)
            {
                return piece;
            }
        }

        return null;
    }
}
