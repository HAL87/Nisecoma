using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyUiScript : MonoBehaviour
{



    // カスタムプロパティ用文字列
    private const string ROOM_CREATOR = "RoomCreator";
    private const string WHICH_TURN = "whichTurn";
    private const string REST_TURN = "restTurn";

    private const string CURRENT_FIGURE_PLAYER_ID = "currentFigurePlayerId";
    private const string CURRENT_FIGURE_ID_ON_BOARD = "currentFigureIdOnBoard";

    private const string OPPONENT_FIGURE_PLAYER_ID = "opponentFigurePlayerId";
    private const string OPPONENT_FIGURE_ID_ON_BOARD = "opponentFigureIdOnBoard";


    private const string GOAL_ANGLE_0 = "goalAngle0";
    private const string GOAL_ANGLE_1 = "goalAngle1";

    private const string IS_WAITING = "isWaiting";

    //部屋作成ウインドウ表示用ボタン
    public Button OpenRoomPanelButton;

    //部屋作成ウインドウ
    public GameObject CreateRoomPanel;  //部屋作成ウインドウ
    public Text RoomNameText;           //作成する部屋名
    public Button CreateRoomButton;     //部屋作成ボタン
   
    // Update is called once per frame
    void Update()
    {

    }

    //部屋作成ウインドウ表示用ボタンを押したときの処理
    public void OnClick_OpenRoomPanelButton()
    {
        //部屋作成ウインドウが表示していれば
        if (CreateRoomPanel.activeSelf)
        {
            //部屋作成ウインドウを非表示に
            CreateRoomPanel.SetActive(false);
        }
        else //そうでなければ
        {
            //部屋作成ウインドウを表示
            CreateRoomPanel.SetActive(true);
        }
    }

    //部屋作成ボタンを押したときの処理
    public void OnClick_CreateRoomButton()
    {
        Debug.Log("部屋を作ります");
        //作成する部屋の設定
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;   //ロビーで見える部屋にする
        roomOptions.IsOpen = true;      //他のプレイヤーの入室を許可する
        roomOptions.MaxPlayers = 2;    //入室可能人数を設定
        //ルームカスタムプロパティで部屋作成者を表示させるため、作成者の名前を格納
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { ROOM_CREATOR, PhotonNetwork.NickName },
            //この段階で先行後攻を決めておく（改修の可能性あり）
            { WHICH_TURN, Random.Range(0, 2)},
            { REST_TURN, 300 },
            { CURRENT_FIGURE_PLAYER_ID, -1 },
            { CURRENT_FIGURE_ID_ON_BOARD, -1 },
            { OPPONENT_FIGURE_PLAYER_ID, -1 },
            { OPPONENT_FIGURE_ID_ON_BOARD, -1 },
            { GOAL_ANGLE_0, -1 },
            { GOAL_ANGLE_1, -1 },
            { IS_WAITING, true }
        };
        //ロビーにカスタムプロパティの情報を表示させる
        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            ROOM_CREATOR,
        };

        //部屋作成
        PhotonNetwork.CreateRoom(RoomNameText.text, roomOptions, null);
    }
}