using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteMakerNext : MonoBehaviour
{
    //ルーレットの表示場所
    [SerializeField] private Transform imageParentTransform;
    //ワザの数（ミス含む)
    [SerializeField] private int moveCount;
    //ルーレットのワザ名
    [SerializeField] private List<string> moveNames;
    //ルーレットの打点、星
    [SerializeField] private List<string> movePowers;
    //各ルーレットの色
    [SerializeField] private List<Color> rouletteColors;
    [SerializeField] private List<float> rouletteRange;
    //白い円の画像
    [SerializeField] private Image rouletteImage;

    private float convertConstant = 360f/96f;
    private float totalRange = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            //指定位置に白い円の画像を生成
            var obj = Instantiate(rouletteImage, imageParentTransform);
            obj.fillAmount = rouletteRange[i] / 96;
            //ピースの回転
            Vector3 axis = new Vector3(0f, 0f, -1f);
            float angle = convertConstant * totalRange ;
            Quaternion q = Quaternion.AngleAxis(angle, axis);
            obj.transform.rotation = q * obj.transform.rotation;

            //ルーレットの色を決定
            //obj.color = rouletteColors[(moveNames.Count - 1 - i)];
            obj.color = rouletteColors[i];
            //ワザの名前を入力
            Text[] texts = obj.GetComponentsInChildren<Text>();
            texts[0].text = moveNames[i];
            texts[1].text = movePowers[i];

            obj.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, -(rouletteRange[i] / 2 + totalRange) * convertConstant);
            //obj.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 0);
            totalRange += rouletteRange[i];
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
