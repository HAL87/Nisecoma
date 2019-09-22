using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GetInfo : MonoBehaviour
{
    private GameObject[] data = new GameObject[BoardController.NUMBER_OF_PLAYERS];
    //使い方: 何らかのトリガーで(ボタンとか)フィギュアの情報を取得
    public void SetData(GameObject _data, int player)
    {
        data[player] = _data;
    }
    public GameObject GetData(int player)
    {
        return data[player];
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
