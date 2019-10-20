using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveParameter : MonoBehaviour
{
    // moveDataの子要素である各ワザにそれぞれアタッチ
    // 色の名前
    public enum MoveOfColorName
    {
        White,
        Purple,
        Blue,
        Gold,
        Red
    };
    [SerializeField] private MoveOfColorName moveOfColorName;
    // ワザの名前
    [SerializeField] private string moveName;
    // ピース幅
    [SerializeField] private int moveRange;
    // ピースの色
    // [SerializeField] private Color color;
    // 打点（白、金のみ）
    [SerializeField] private int movePower;
    // 星の数（紫のみ）
    [SerializeField] private int moveNumberOfStar;
    // 技効果
    // 動的引数付きシリアル関数は抽象クラスなので、独自クラスを作って継承させないといけない
    [System.Serializable] public class MyEvent : UnityEvent<BoardController.PhaseState> { }
    // 技効果関数
    [SerializeField] MyEvent MoveEffect;

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
    /*public Color GetColor()
    {
        return color;
    }*/

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
    
    public void ExecMoveEffect(BoardController.PhaseState phaseState)
    {
        if(MoveEffect != null)
        {
            MoveEffect.Invoke(phaseState);
        }
    }

    public MyEvent GetMoveEffect()
    {
        // 追加効果のない技をnullにするかどうかは要検討
        if(MoveEffect != null)
        {
            return MoveEffect;
        }

        return null;
    }
}
