using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// 
/// Unity 2019.1.11f1
/// 
/// Pun: 2.4
/// 
/// Photon lib: 4.1.2.4
/// 
/// </summary>

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string BOARD_SCENE_NAME = "BoardScene";

    /////////////////////////////////////////////////////////////////////////////////////
    // Awake & Start ////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // シーンの読み込みコールバックを登録.
        SceneManager.sceneLoaded += OnLoadedScene;

        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.LocalPlayer.NickName = "Player" + UnityEngine.Random.Range(1000, 9999);
        PhotonNetwork.ConnectUsingSettings();

    }
    /////////////////////////////////////////////////////////////////////////////////////
    // Connect //////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    public void CreateAndJoinRoom()
    {
        /* ルームオプションの設定 */

        // RoomOptionsのインスタンスを生成
        RoomOptions roomOptions = new RoomOptions();

        // ルームに入室できる最大人数。0を代入すると上限なし。
        roomOptions.MaxPlayers = 2;

        // ルームへの入室を許可するか否か
        roomOptions.IsOpen = true;

        // ロビーのルーム一覧にこのルームが表示されるか否か
        roomOptions.IsVisible = true;
        PhotonNetwork.JoinOrCreateRoom("room", roomOptions, TypedLobby.Default);

    }

    public void OnLoadedScene(Scene _scene, LoadSceneMode _mode)
    {
        if(_scene.name == BOARD_SCENE_NAME)
        {
            //登録していたデッキ内のフィギュアを生成、プレイヤーIDを渡す
            //配置はBoardControllerに任せる
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // Pun Callbacks ////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////


    // マスターサーバーに接続した時
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");

    }

    // 部屋を作成した時
    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    // 部屋に入室した時
    public override void OnJoinedRoom()
    {
        // ルームに入ると同時にシーンを遷移させる.
        Debug.Log("OnJoinedRoom");
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene(BOARD_SCENE_NAME);
    }

    private void Update()
    {

    }
}

