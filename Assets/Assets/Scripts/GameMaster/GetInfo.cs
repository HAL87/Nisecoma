using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GetInfo : MonoBehaviour
{
    //public static GameObject[] figureMoveData = new GameObject[2];
    private GameObject[] data = new GameObject[2];
    //使い方: 何らかのトリガーで(ボタンとか)フィギュアの情報を取得
    public void SetData(GameObject _data, int player)
    {
        data[player] = _data;
    }
    public GameObject GetData(int player)
    {
        return data[player];
    }
    /*
    public GameObject GetFigureMoveData0()
    {
        return figureMoveData[0];
    }

    public void SetFigureMoveData0(GameObject _data)
    {
        figureMoveData[0] = _data;
    }
    */
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if(data[0] != null)
       // {
            //Debug.Log(data[0]);
        //}
    }
}
