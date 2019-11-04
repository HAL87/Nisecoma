using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class ButtonClick : MonoBehaviour
{
    //デモ用だけど後々参考になるかも
    //ここでカラーを設定
    [SerializeField]
    Color btnColor1 = Color.white;
    [SerializeField]
    Color btnColor2 = Color.red;

    //ボタンをキャッシュする変数
    [SerializeField] private List<Button> buttons;

    [SerializeField] private int player;
    private DiskDataList diskDataList;
    private GetInfo getInfo;
    //[SerializeField] private GameObject gameMaster;
    private GameObject gameMaster;
    void Start()
    {
        gameMaster = GameObject.Find("GameMaster");

        diskDataList = gameMaster.GetComponent<DiskDataList>();
        getInfo = gameMaster.GetComponent<GetInfo>();
    }

    public void OnClick(Button button)
    {
        for(int i = 0; i < buttons.Count; i++)
        {
            if(buttons[i] == button)
            {
                //選択されているボタンを赤色に
                buttons[i].image.color = btnColor2;
                //ここをなんかID取得みたいにするのがいいんじゃないか
                GameObject data = diskDataList.Data[i];
                getInfo.SetData(data, player);
                Debug.Log(getInfo.GetData(player));
                //getInfo.SetFigureMoveData0(data);
            }
            else
            {
                buttons[i].image.color = btnColor1;
            }
        }
    }
}
