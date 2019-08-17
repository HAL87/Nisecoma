using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParameter : MonoBehaviour
{
    public enum MoveOfColorName
    {
        White,
        Purple,
        Blue,
        Gold,
        Red
    };
    [SerializeField] private string moveName;
    [SerializeField] private int moveRange;
    [SerializeField] private MoveOfColorName moveOfColorName;
    [SerializeField] private Color color;
    [SerializeField] private int movePower;
    [SerializeField] private int moveNumberOfStar;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public string GetMoveName()
    {
        return moveName;
    }

    public void SetMoveRange(int _moveRange)
    {
        moveRange = _moveRange;
    }
    public int GetMoveRange()
    {
        return moveRange;
    }

    public MoveOfColorName GetMoveColorName()
    {
        return moveOfColorName;
    }
    public Color GetColor()
    {
        return color;
    }


    public void SetMovePower(int _movePower)
    {
        movePower =  _movePower;
    }
    public int GetMovePower()
    {
        return movePower;
    }

    public void SetMoveNumberOfStar(int _moveNumberOfStar)
    {
        moveNumberOfStar = _moveNumberOfStar;
    }
    public int GetMoveNumberOfStar()
    {
        return moveNumberOfStar;
    }
    
}
