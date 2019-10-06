using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveList : MonoBehaviour
{

    BoardController boardController;

    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void HyperSonic()
    {
        Debug.Log("はいぱー");

    }
    public void ShatolFlip()
    {
        Debug.Log("しゃとる");
    }
    public void Scychokinesisu()
    {
        Debug.Log("さいきね");
    }

    public void Bubble(BoardController.PhaseState phaseState) {
        Debug.Log("あわわ～");
    }

    public void Dodge(BoardController.PhaseState phaseState)
    {
        Debug.Log("かわした！");
    }

    public void QuickAttack(BoardController.PhaseState phaseState)
    {
        Debug.Log("電光石火！");
    }

    public void WingAttack(BoardController.PhaseState phaseState)
    {
        Debug.Log("つばさでうつ(PP56)");
    }

    public void FlyAway(BoardController.PhaseState phaseState)
    {
        //BoardController.PhaseState phaseState;
        if (phaseState == BoardController.PhaseState.AfterBattle)
        {
            Debug.Log("飛びます");

            GameObject currentObject;

            GameObject opponentObject;

        }
        else
        {
            Debug.Log("飛びません");
        }
    }

    // 追加効果なし
    public void NonEffect(BoardController.PhaseState phaseState)
    {
        Debug.Log("追加効果なし");
    }
}
