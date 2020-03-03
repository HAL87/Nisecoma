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
    [SerializeField] private List<GameObject> battleUi;

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

        DisplayRoulette(boardController.GetCurrentFigure());
        DisplayRoulette(boardController.GetOpponentFigure());
    }
    public void NowBattle()
    {
        SpinRoulette(boardController.GetCurrentFigure());
        SpinRoulette(boardController.GetOpponentFigure());
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
        Debug.Log("アナログオブジェクトを消した");
        boardController.SetDoneFlagCustomProperty(true);
    }

    //ルーレット表示
    private void DisplayRoulette(GameObject obj)
    {
        int playerId = obj.GetComponent<FigureParameter>().GetPlayerId();
        int figureId = obj.GetComponent<FigureParameter>().GetFigureIdOnBoard();
        Figure figure = deckManager[playerId].GetFigure(figureId);
        rouletteManager[playerId].MakeRoulette(figure);
    }
    private void SpinRoulette(GameObject obj)
    {
        int playerId = obj.GetComponent<FigureParameter>().GetPlayerId();
        rouletteManager[playerId].SpinRoulette(playerId);

    }
    // ボード用のオブジェクトを表示/非表示
    private void ActivateBoardObjects(bool _flag)
    {
        // ノードを表示/非表示にする
        for (int i = 0; i < CList.NUMBER_OF_WALK_NODES; i++)
        {
            boardController.GetNodes().transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = _flag;
        }
        // エッジを表示/非表示にする
        for (int i = 0; i < boardMaster.transform.childCount; i++)
        {
            boardMaster.transform.GetChild(i).GetComponent<LineRenderer>().enabled = _flag;
        }

        // フィギュアを表示/非表示にする
        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject obj in boardController.GetFigures()[i])
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
}
