using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;


public class PlayerNameInputFieldScript : MonoBehaviour
{
    #region Private変数定義
    static string playerNamePrefKey = "PlayerName";
    private InputField inputField;
    private string inputValue;
    #endregion
    #region MonoBehaviourコールバック

    void Start()
    {
        string defaultName = "";
        inputField = GetComponent<InputField>();

        // フォーカス
        inputField.ActivateInputField();

        //前回プレイ開始時に入力した名前をロードして表示
        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
                Debug.Log("前回ロード");
            }
        }
    }
    #endregion

    #region Public Method
    public void InputLogger()
    {

        inputValue = inputField.text;

        Debug.Log("入力したのは" + inputValue);

        //InitInputField();
    }

    public void SetPlayerName()
    {
        PhotonNetwork.NickName = inputValue + " ";     //今回ゲームで利用するプレイヤーの名前を設定

        PlayerPrefs.SetString(playerNamePrefKey, inputValue);    //今回の名前をセーブ

        Debug.Log("ニックネームは" + PhotonNetwork.NickName);   //playerの名前の確認。（動作が確認できればこの行は消してもいい）
    }
    #endregion
}