using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyUiScript : MonoBehaviour
{
    //部屋作成ウインドウ表示用ボタン
    public Button OpenRoomPanelButton;

    //部屋作成ウインドウ
    public GameObject CreateRoomPanel;  //部屋作成ウインドウ
    public Text RoomNameText;           //作成する部屋名
    public Button CreateRoomButton;     //部屋作成ボタン
   
    // Update is called once per frame
    void Start()
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
        //作成する部屋の設定
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;   //ロビーで見える部屋にする
        roomOptions.IsOpen = true;      //他のプレイヤーの入室を許可する
        roomOptions.MaxPlayers = 2;    //入室可能人数を設定
        //ルームカスタムプロパティで部屋作成者を表示させるため、作成者の名前を格納
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { CList.ROOM_CREATOR, PhotonNetwork.NickName },
            //この段階で先行後攻を決めておく（改修の可能性あり）
            { CList.WHICH_TURN, Random.Range(0, 2)},
            { CList.REST_TURN, 300 },
            { CList.CURRENT_FIGURE_PLAYER_ID, -1 },
            { CList.CURRENT_FIGURE_ID_ON_BOARD, -1 },
            { CList.OPPONENT_FIGURE_PLAYER_ID, -1 },
            { CList.OPPONENT_FIGURE_ID_ON_BOARD, -1 },
            // { CList.GOAL_ANGLE_0, -1 },
            // { CList.GOAL_ANGLE_1, -1 },
            { CList.IS_WAITING, true },
            { CList.DONE_FLAG[0], false },
            { CList.DONE_FLAG[1], false },
            { CList.SPIN_RESULT[0], -1 },
            { CList.SPIN_RESULT[1], -1 }, 
            { CList.DAMMY, true }
           
        };
        //ロビーにカスタムプロパティの情報を表示させる
        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            CList.ROOM_CREATOR,
            CList.DAMMY
        };

        //部屋作成
        PhotonNetwork.CreateRoom(RoomNameText.text, roomOptions, null);
    }

    public void UpdateRoom()
    {
        Debug.Log("シーン読み込み");
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}