using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RouletteMaker : MonoBehaviour
{
    // ルーレットの表示場所
    [SerializeField] private Transform imageParentTransform;
    // 白い円の画像
    [SerializeField] private Image rouletteImage;
    private const string ROULETTE_IMAGE = "RouletteImage";
    // [SerializeField] private GameObject data;

    private MoveParameter moveParameter;
    private const float CONVERT_CONSTANT = 360f / 96f;

    // Start is called before the first frame update

    public void CreateRulette(GameObject data)
    {
        float totalRange = 0;
        for (int i = 0; i < data.transform.childCount; i++) {
            moveParameter = data.transform.GetChild(i).GetComponent<MoveParameter>();

            // ネットワークオブジェクトにしてみた
           // GameObject netObj = PhotonNetwork.Instantiate(ROULETTE_IMAGE, transform.position, Quaternion.identity, 0);

            //親子関係とサイズの変更はPrefabのStartでやる
           // netObj.transform.parent = imageParentTransform;
            //netObj.transform.localScale = new Vector3(1, 1, 1);
            //Debug.Log("ネットワークオブジェクトにしてみた");
            
            //Image obj = netObj.GetComponent<Image>();


            var obj = Instantiate(rouletteImage, imageParentTransform);
            obj.fillAmount = moveParameter.GetMoveRange() / 96f;

            // ピースの回転
            Vector3 axis = new Vector3(0f, 0f, -1f);
            float angle = CONVERT_CONSTANT * totalRange;
            Quaternion q = Quaternion.AngleAxis(angle, axis);
            obj.transform.rotation = q * obj.transform.rotation;

            // ルーレットの色を決定
            if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.White) obj.color = Color.white;
            else if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Blue) obj.color = new Color(0,154,255,255);
            else if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Purple) obj.color = Color.magenta;
            else if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Gold) obj.color = Color.yellow;
            else if (moveParameter.GetMoveColorName() == MoveParameter.MoveOfColorName.Red) obj.color = Color.red;


            // ワザの名前を入力
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

            obj.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, -(moveParameter.GetMoveRange() / 2 + totalRange) * CONVERT_CONSTANT);
            totalRange += moveParameter.GetMoveRange();

            // Debug.Log(moveParameter.GetMoveName());
        }

    }
    // Update is called once per frame
    void Update()
    {

    }
}
