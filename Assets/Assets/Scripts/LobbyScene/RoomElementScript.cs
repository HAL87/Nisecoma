using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomElementScript : MonoBehaviour
{
    //Room情報UI表示用
    public Text RoomName;   //部屋名
    public Text PlayerNumber;   //人数
    public Text RoomCreator;    //部屋作成者名

    //入室ボタンroomname格納用
    private string roomname;

    //GetRoomListからRoom情報をRoomElementにセットしていくための関数
    public void SetRoomInfo(string _roomName, int _playerNumber, int _maxPlayer, string _roomCreator)
    {
        Debug.Log("ルーム情報がセットされたよ");
        //入室ボタン用roomname取得
        roomname = _roomName;
        RoomName.text = "部屋名：" + _roomName;
        PlayerNumber.text = "人　数：" + _playerNumber + "/" + _maxPlayer;
        RoomCreator.text = "作成者：" + _roomCreator;
    }

    //入室ボタン処理
    public void OnJoinRoomButton()
    {
        //roomnameの部屋に入室
        PhotonNetwork.JoinRoom(roomname);
    }
}