using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SystemUIMaster : MonoBehaviourPunCallbacks
{

    // BoardScene\CanvasScreenSpace\SystemUI\BackToLobbyButton
    public void OnClickBackToLobbyButton()
    {
        PhotonNetwork.LeaveRoom();

        Debug.Log("退室");
        SceneManager.LoadScene("LobbyScene");
        Debug.Log("ああ");
    }
}
