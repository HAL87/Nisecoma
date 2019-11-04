using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LauncherScript : MonoBehaviourPunCallbacks
{
    #region Public変数定義

    //Public変数の定義はココで

    #endregion

    #region Private変数
    //Private変数の定義はココで
    private PlayerNameInputFieldScript playerNameInputFieldScript;
    #endregion
    [SerializeField] private GameObject inputField;

    private void Start()
    {
        Debug.Log("Lancher起動");
        Screen.SetResolution(540, 960, false, 60);
        DontDestroyOnLoad(this);
        playerNameInputFieldScript = inputField.GetComponent<PlayerNameInputFieldScript>();
    }
    #region Public Methods


    //ログインボタンを押したときに実行される
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {                         //Photonに接続できていなければ
            PhotonNetwork.ConnectUsingSettings();   //Photonに接続する
            Debug.Log("Photonに接続しました。");
            playerNameInputFieldScript.SetPlayerName();
            SceneManager.LoadScene("LobbyScene");    //Lobbyシーンに遷移
        }
    }
    #endregion

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("ロビーに入りました");
    }
}