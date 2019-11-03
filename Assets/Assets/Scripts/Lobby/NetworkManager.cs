using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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

    // カスタムプロパティ用文字列
    private const string WHICH_TURN = "whichTurn";
    private const string REST_TURN = "restTurn";

    private const string CURRENT_FIGURE_PLAYER_ID = "currentFigurePlayerId";
    private const string CURRENT_FIGURE_ID_ON_BOARD = "currentFigureIdOnBoard";

    private const string OPPONENT_FIGURE_PLAYER_ID = "opponentFigurePlayerId";
    private const string OPPONENT_FIGURE_ID_ON_BOARD = "opponentFigureIdOnBoard";


    private const string GOAL_ANGLE_0 = "goalAngle0";
    private const string GOAL_ANGLE_1 = "goalAngle1";

    private const string IS_WAITING = "isWaiting";
    // 
    [SerializeField] private GameObject CreateAndJoinButton;

    /////////////////////////////////////////////////////////////////////////////////////
    // Awake & Start ////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Screen.SetResolution(540,960, false, 60);

        // シーンの読み込みコールバックを登録.
        SceneManager.sceneLoaded += OnLoadedScene;

        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.LocalPlayer.NickName = "Player" + UnityEngine.Random.Range(1000, 9999);
        PhotonNetwork.ConnectUsingSettings();

    }
    /////////////////////////////////////////////////////////////////////////////////////
    // Connect //////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    // CreateAndJoinButtonが押されたときの処理
    // Assets: LobbyScene\Canvas\CreateAndJoinButton
    
    public void CreateAndJoinRoom()
    {
        /* ルームオプションの設定 */
        
        // RoomOptionsのインスタンスを生成
        RoomOptions roomOptions = new RoomOptions
        {

            // ルームに入室できる最大人数。0を代入すると上限なし。
            MaxPlayers = 2,

            // ルームへの入室を許可するか否か
            IsOpen = true,

            // ロビーのルーム一覧にこのルームが表示されるか否か
            IsVisible = true
        };

        Hashtable roomHash = new Hashtable();
        roomHash.Add(WHICH_TURN, 0);
        roomHash.Add(REST_TURN, 300);

        roomHash.Add(CURRENT_FIGURE_PLAYER_ID, -1);
        roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, -1);

        roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, -1);
        roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, -1);
        
        roomHash.Add(GOAL_ANGLE_0, -1);
        roomHash.Add(GOAL_ANGLE_1, -1);

        roomHash.Add(IS_WAITING, true);
        roomOptions.CustomRoomProperties = roomHash;
        
        PhotonNetwork.JoinOrCreateRoom("room", roomOptions, TypedLobby.Default);

    }
    /*
    public void CreateRoom(string roomName, string password)
    {
        string roomNameStr = roomName;
        if (!string.IsNullOrEmpty(password)) roomNameStr += "_" + password;

        // ルームオプションの設定               
        RoomOptions roomOptions = new RoomOptions
        {
            // ルームに入室できる最大人数。
            MaxPlayers = 2,

            // ルームへの入室を許可するか否か
            IsOpen = true,

            // ロビーのルーム一覧にこのルームが表示されるか否か
            IsVisible = true
        };

        // カスタムプロパティの設定
        Hashtable roomHash = new Hashtable();

        roomHash.Add(WHICH_TURN, 0);
        roomHash.Add(REST_TURN, 300);

        roomHash.Add(CURRENT_FIGURE_PLAYER_ID, -1);
        roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, -1);

        roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, -1);
        roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, -1);

        roomHash.Add(GOAL_ANGLE_0, -1);
        roomHash.Add(GOAL_ANGLE_1, -1);

        roomHash.Add(IS_WAITING, true);
        roomOptions.CustomRoomProperties = roomHash;

        // ルームを作成
        PhotonNetwork.CreateRoom(roomNameStr,roomOptions, null);
                    
    }
    */
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
        CreateAndJoinButton.SetActive(true);
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

    public void OnPhotonCreateRoomFailed()
    {
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    // ルームリストに更新があった時
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
    }

}

