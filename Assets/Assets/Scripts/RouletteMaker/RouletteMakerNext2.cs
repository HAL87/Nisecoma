using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteMakerNext2 : MonoBehaviour
{
    //ルーレットの表示場所
    [SerializeField] private Transform imageParentTransform;
    //白い円の画像
    [SerializeField] private Image rouletteImage;
    [SerializeField] private GameObject data;

    private MoveParameter moveParameter;
    private float convertConstant = 360f / 96f;
    private float totalRange = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < data.transform.childCount; i++) {
            moveParameter = data.transform.GetChild(i).GetComponent<MoveParameter>();
            var obj = Instantiate(rouletteImage, imageParentTransform);
            obj.fillAmount = moveParameter.GetMoveRange() / 96f;

            //ピースの回転
            Vector3 axis = new Vector3(0f, 0f, -1f);
            float angle = convertConstant * totalRange;
            Quaternion q = Quaternion.AngleAxis(angle, axis);
            obj.transform.rotation = q * obj.transform.rotation;

            //ルーレットの色を決定
            obj.color = moveParameter.GetColor();

            //ワザの名前を入力
            Text[] texts = obj.GetComponentsInChildren<Text>();
            texts[0].text = moveParameter.GetMoveName();
            if(moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.White ||
               moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Gold)
            {
                texts[1].text = moveParameter.GetMovePower().ToString();
            }
            else if(moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Purple)
            {
                string star = "";
                for (int j = 0; j < moveParameter.GetMoveNumberOfStar() ; j++)
                {
                    star += "☆";
                }
                texts[1].text = star;
            }
            else if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Blue ||
                     moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Red)
            {
                texts[1].text = "";
            }

            obj.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, -(moveParameter.GetMoveRange() / 2 + totalRange) * convertConstant);
            totalRange += moveParameter.GetMoveRange();

            //Debug.Log(moveParameter.GetMoveName());
        }

    }
  
    // Update is called once per frame
    void Update()
    {

    }
}
