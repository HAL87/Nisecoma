using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
public class LocalController : MonoBehaviour
{
    // ボード用のUI
    [SerializeField] private GameObject playerTurnText;
    [SerializeField] private GameObject restTurnText;
    [SerializeField] private GameObject gameEndText;
    [SerializeField] private GameObject turnEndButton;
    [SerializeField] private GameObject forfeitButton;
    [SerializeField] private GameObject playerNameText;
    [SerializeField] private GameObject opponentNameText;
    [SerializeField] private GameObject informationText;

    // システムUI
    [SerializeField] private GameObject backToLobbyButton;
    // [SerializeField] private GameObject exitGameButton;

    // アナログ用のオブジェクト
    [SerializeField] private List<DeckManager> deckManager;
    [SerializeField] private List<RouletteManager> rouletteManager;
    [SerializeField] private GameObject diskParent;
    //カメラの位置
    [SerializeField] private Transform cameraTransform;
        
    [SerializeField] private GameObject boardMaster;
    private BoardController boardController;

    private void Start()
    {
        boardController = boardMaster.GetComponent<BoardController>();

    }

    public void PrepareBattle()
    {
        // ボードのオブジェクトを消す
        ActivateBoardObjects(false);

        // ルーレットを表示
        DisplayRoulette(boardController.CurrentFigure);
        DisplayRoulette(boardController.OpponentFigure);
    }
    public void NowBattle()
    {
        // 
        SpinRoulette(boardController.CurrentFigure);
        SpinRoulette(boardController.OpponentFigure);
    }

    public void EndBattle()
    {
        // アナログを消す
        for (int i = 0; i < CList.NUMBER_OF_PLAYERS; i++)
        {
            Destroy(rouletteManager[i].GetBatleUIObj());
        }
        // ボードのオブジェクトを戻す
        ActivateBoardObjects(true);

        //ロック解除
        boardController.SetDoneFlagCustomProperty(boardController.GetMyPlayerId(), true);
    }

    //ルーレット表示
    private void DisplayRoulette(GameObject obj)
    {
        int playerId = obj.GetComponent<FigureParameter>().PlayerId;
        int figureId = obj.GetComponent<FigureParameter>().FigureIdOnBoard;
        Figure figure = deckManager[playerId].GetFigure(figureId);
        rouletteManager[playerId].MakeRoulette(figure);
    }
    private void SpinRoulette(GameObject obj)
    {
        int playerId = obj.GetComponent<FigureParameter>().PlayerId;
        rouletteManager[playerId].SpinRoulette(playerId);

    }
    // ボード用のオブジェクトを表示/非表示
    private void ActivateBoardObjects(bool _flag)
    {
        // ノードを表示/非表示にする
        for (int i = 0; i < CList.NUMBER_OF_WALK_NODES; i++)
        {
            boardController.Nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = _flag;
        }
        // エッジを表示/非表示にする
        for (int i = 0; i < boardMaster.transform.childCount; i++)
        {
            boardMaster.transform.GetChild(i).GetComponent<LineRenderer>().enabled = _flag;
        }

        // フィギュアを表示/非表示にする
        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject obj in boardController.Figures[i])
            {
                for (int j = 0; j < obj.transform.childCount; j++)
                {
                    obj.transform.GetChild(j).GetComponent<SpriteRenderer>().enabled = _flag;
                }
            }
        }

        // UIを表示/非表示にする
        turnEndButton.SetActive(_flag);
        restTurnText.GetComponent<TextMeshProUGUI>().enabled = _flag;
        forfeitButton.SetActive(_flag);
        playerNameText.SetActive(_flag);
        opponentNameText.SetActive(_flag);


    }

    
    public void RotateUi()
    {
        Debug.Log("回転した");
        diskParent.transform.Rotate(0, 0, 180f);

    }
}
