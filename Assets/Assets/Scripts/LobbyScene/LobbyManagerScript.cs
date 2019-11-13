using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManagerScript : MonoBehaviourPunCallbacks
{

    private const string BOARD_SCENE_NAME = "BoardScene";

    #region Public Variables
    //部屋一覧表示用オブジェクト
    public GameObject RoomParent;//ScrolViewのcontentオブジェクト
    public GameObject RoomElementPrefab;//部屋情報Prefab

    //ルーム接続情報表示用Text
    public Text InfoText;
    #endregion

    #region MonoBehaviour CallBacks
    void Awake()
    {
        //ルーム内のクライアントがMasterClientと同じシーンをロードするように設定
        // PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        //ルーム一覧取得
        // GetRooms();
    }
    #endregion

    #region Public Methods

    //RoomElementを一括削除
    public static void DestroyChildObject(Transform parentTransform)
    {
        for (int i = 0; i < parentTransform.childCount; ++i)
        {
            GameObject.Destroy(parentTransform.GetChild(i).gameObject);
        }
    }
    #endregion

    #region Photon.PunBehaviour CallBacks

    public override void OnRoomListUpdate(List<RoomInfo> roomInfo)
    {
        Debug.Log("ルーム数は" + roomInfo.Count);
        DestroyChildObject(RoomParent.transform);
        //ルームが無ければreturn
        if (roomInfo == null || roomInfo.Count == 0) return;

        //ルームがあればRoomElementでそれぞれのルーム情報を表示
        for (int i = 0; i < roomInfo.Count; i++)
        {
            Debug.Log(roomInfo[i].Name + " : " + roomInfo[i].Name + "–" + roomInfo[i].PlayerCount + " / " + roomInfo[i].MaxPlayers /*+ roomInfo[i].CustomProperties["roomCreator"].ToString()*/);

            //ルーム情報表示用RoomElementを生成
            GameObject RoomElement = GameObject.Instantiate(RoomElementPrefab);

            //RoomElementをcontentの子オブジェクトとしてセット    
            RoomElement.transform.SetParent(RoomParent.transform, false);
            //RoomElementにルーム情報をセット
            RoomElement.GetComponent<RoomElementScript>().SetRoomInfo(roomInfo[i].Name, roomInfo[i].PlayerCount, roomInfo[i].MaxPlayers, roomInfo[i].CustomProperties["RoomCreator"].ToString());
        }
    }

    //ルーム作成失敗した場合
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //テキストを表示
        InfoText.text = "ルームの作成に失敗しました";
    }

    //ルームの入室に失敗した場合
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //テキストを表示
        InfoText.text = "ルームの入室に失敗しました";
    }

    //ルームに入室時の処理
    public override void OnJoinedRoom()
    {
        //プレイヤーローカル変数初期化
        // LocalVariables.VariableReset();
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene(BOARD_SCENE_NAME);
    }
    //ルーム作成時の処理(作成者しか実行されない)
    public override void OnCreatedRoom()
    {
        //battleシーンへ遷移
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel(BOARD_SCENE_NAME);
    }
    #endregion
}
