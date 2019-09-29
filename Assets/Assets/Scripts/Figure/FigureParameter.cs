using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FigureParameter : MonoBehaviour
{
    [SerializeField] private int mp;
    [SerializeField] private GameObject data;
    [SerializeField] private int playerId;
    [SerializeField] private int attackRange = 1;
    [SerializeField] private GameObject waitCounter;
    [SerializeField] private Text waitCounterText;
    private bool beSelected = false;
    //ゲーム開始時にboardMasterによりセットされるID
    private int figureIdOnBoard;
    private int benchId;
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
    public void SetFigureIdOnBoard(int _figureIdOnBoard)
    {
        figureIdOnBoard = _figureIdOnBoard;
    }
    public int GetFigureIdOnBoard()
    {
        return figureIdOnBoard;
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
    public int GetPlayerId()
    {
        return playerId;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }
    public GameObject GetData()
    {
        return data;
    }

    public void SetBenchId(int _benchId)
    {
        benchId = _benchId;
    }
    public int GetBenchId()
    {
        return benchId;
    }
    // ウェイトカウントをセットする
    // ウェイト1以上の場合はカウンターを表示する
    public void SetWaitCount(int _waitCount)
    {
        waitCount = _waitCount;
        if(1 <= waitCount)
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
    public void DecreaseWaitCount()
    {
        waitCount--;
        waitCounterText.text = "" + waitCount;
        if (1 <= waitCount)
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
