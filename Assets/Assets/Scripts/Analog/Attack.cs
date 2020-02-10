using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{

    public string name;
    public string color;
    public int damage;
    public int numberOfStar;
    public int size;

    public Attack(string name, string color, int damage, int numberOfStar, int size)
    {
        this.name = name;
        this.color = color;
        this.damage = damage;
        this.numberOfStar = numberOfStar;
        this.size = size;
    }
}
