using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MoveParameter : MonoBehaviour
{
    //moveDataの子要素である各ワザにそれぞれアタッチ
    //色の名前
    public enum MoveOfColorName
    {
        White,
        Purple,
        Blue,
        Gold,
        Red
    };
    [SerializeField] private MoveOfColorName moveOfColorName;
    //ワザの名前
    [SerializeField] private string moveName;
    //ピース幅
    [SerializeField] private int moveRange;
    //ピースの色
    [SerializeField] private Color color;
    //打点（白、金のみ）
    [SerializeField] private int movePower;
    //星の数（紫のみ）
    [SerializeField] private int moveNumberOfStar;
    public UnityEvent OnInitDone;
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
