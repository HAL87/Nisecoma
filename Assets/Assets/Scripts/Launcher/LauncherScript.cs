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
    // string _gameVersion = "Chapter12";   //ゲームのバージョン。仕様が異なるバージョンとなったときはバージョンを変更しないとエラーが発生する。
    #endregion

    #region Public Methods
    //ログインボタンを押したときに実行される
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {                         //Photonに接続できていなければ
            PhotonNetwork.ConnectUsingSettings();   //Photonに接続する
            Debug.Log("Photonに接続しました。");
            Debug.Log(PhotonNetwork.NickName);
            SceneManager.LoadScene("LobbyScene2");    //Lobbyシーンに遷移
        }
    }
    #endregion
}