using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;


public class RoomNameInputFieldScript : MonoBehaviour
{
    #region Private変数定義
    static string roomNamePrefKey = "RoomName";
    #endregion

    #region MonoBehaviourコールバック
    void Start()
    {
        string defaultName = "";
        InputField _inputField = this.GetComponent<InputField>();

        //前回プレイ開始時に入力した名前をロードして表示
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(roomNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(roomNamePrefKey);
                _inputField.text = defaultName;
            }
        }
    }
    #endregion

    #region Public Method

    public void SetRoomName(string value)
    {
        PlayerPrefs.SetString(roomNamePrefKey, value);    //今回の名前をセーブ

        Debug.Log(value);   //playerの名前の確認。（動作が確認できればこの行は消してもいい）
    }
    #endregion
}