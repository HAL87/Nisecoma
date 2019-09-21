using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FigureParameter : MonoBehaviour
{
    [SerializeField] private int mp;
    [SerializeField] private GameObject data;
    [SerializeField] private int playerID;
    [SerializeField] private int attackRange = 1;
    [SerializeField] private GameObject waitCounter;
    [SerializeField] private Text waitCounterText;
    private bool beSelected = false;
    //ゲーム開始時にboardMasterによりセットされるID
    private int figureIDOnBoard;
    private int benchID;
    private int position;
    private int waitCount = 0;  // ウェイト デフォルトは0
    //mp
    public int GetMp()
    {
        return mp;
    }

    //現在地のノードID
    public void SetPosition(int _position)
    {
        position = _position;
    }
    public int GetPosition()
    {
        return position;
    }

    //ゲーム開始時に定められるID
    public void SetFigureIDOnBoard(int _figureIDOnBoard)
    {
        figureIDOnBoard = _figureIDOnBoard;
    }
    public int GetFigureIDOnBoard()
    {
        return figureIDOnBoard;
    }

    //選択中フラグ
    public bool GetBeSelected()
    {
        return beSelected;
    }
    public void SetBeSelected(bool _beSelcted)
    {
        beSelected = _beSelcted;
    }

    //プレイヤーのID(0 or 1)
    public int GetPlayerID()
    {
        return playerID;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }
    public GameObject GetData()
    {
        return data;
    }

    public void SetBenchID(int _benchID)
    {
        benchID = _benchID;
    }
    public int GetBenchID()
    {
        return benchID;
    }
    // ウェイトカウントをセットする
    // ウェイト1以上の場合はカウンターを表示する
    public void SetWaitCount(int _waitCount)
    {
        waitCount = _waitCount;
        if(waitCount >= 1)
        {
            waitCounterText.text = "" + waitCount;
            waitCounter.SetActive(true);
        }
    }

    public int GetWaitCount()
    {
        return waitCount;
    }

    // ウェイトを1つ減らす
    // 正値の保証は使用者が行う
    public void decreaseWaitCount()
    {
        waitCount--;
        waitCounterText.text = "" + waitCount;
        if (waitCount >= 1)
        {
            waitCounter.SetActive(true);
        }
        else
        {
            waitCounter.SetActive(false);
        }
    }

    public GameObject GetWaitCounter()
    {
        return waitCounter;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
