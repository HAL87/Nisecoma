using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureParameter : MonoBehaviour
{
    [SerializeField] private int mp;
    [SerializeField] private int position;
    [SerializeField] private GameObject data;
    [SerializeField] private int playerID;
    [SerializeField] private int attackRange = 1;
    private bool beSelected = false;
    //ゲーム開始時にboardMasterによりセットされるID
    private int figureIDOnBoard;

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
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
