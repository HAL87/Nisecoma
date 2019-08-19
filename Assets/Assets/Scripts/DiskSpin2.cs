using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskSpin2 : MonoBehaviour
{
    //目的の角度(360×N + x (0 < x <= 360))
    private float goalAngle;

    //回転数
    [SerializeField] private int N = 10;

    //総回転時間
    [SerializeField] private float totalTime = 1.5f;

    //スローになるまでの時間
    [SerializeField] private float normalTime = 1.2f;

    //スローになるまでの角速度
    private float omega;

    //スローになった後の角速度の減衰率
    private float deltaOmega;

    //スローになった後の角速度
    private float omegaSlow;
    //while文の中身の繰り返し回数
    private float elapsedTime;
    private int loopCount;

    //何秒ごとに回転関数を呼び出すか
    [SerializeField] private float clock = 0.05f;

    //注意: 今のプログラムの仕様上、totalTime, normalTimeをclockで割り切れるようにしなければいけません
    //最初はclockを累加したもの（経過時間）とnormalTime, totalTimeを比較していたのですが、
    //時間を扱うと厳密には0.0001fのズレで条件が満たされなかったり満たされたりみたいになってダルかった
    //ので今のwhile文の中身の繰り返し回数で条件分岐を管理しています

    [SerializeField] private GameObject data;

    private MoveParameter moveParameter;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spin());
    }

    IEnumerator Spin()
    {
        yield return new WaitForSeconds(0.1f);
        //回転開始からの時間
        //elapsedTime = 0;
        loopCount = 0;

        //初期位置から目標点までの回転角度
        goalAngle = 360 * N + Random.Range(0, 360);
        //goalAngle = 360 * N;
        //スローになるまでの角速度立式
        omega = 2 * goalAngle / (totalTime + normalTime);
        deltaOmega = omega * clock / (totalTime - normalTime + clock);
        omegaSlow = omega;
        //Debug.Log(omega);

        while (loopCount < totalTime/clock)
        {
            //スローになるまで
            if (loopCount < normalTime/clock)
            {
                //等角速度運動
                transform.Rotate(new Vector3(0, 0, omega * clock));
            }
            
            else if(loopCount < totalTime/clock)
            {
                //速度をループ回数に対して階段状に小さくする
                omegaSlow = omegaSlow - deltaOmega;
                //Debug.Log(omegaSlow);
                transform.Rotate(new Vector3(0, 0, omegaSlow * clock));
            }
            
            //elapsedTime += clock;
            loopCount++;
            //Debug.Log(loopCount);
            yield return new WaitForSeconds(clock);
        }
        Debug.Log("ゴールは");
        Debug.Log(goalAngle % 360);
        float totalRange = 0;
        for (int i = 0; i < data.transform.childCount; i++)
        {
            //Debug.Log(i);
            moveParameter = data.transform.GetChild(i).GetComponent<MoveParameter>();
            //Debug.Log(moveParameter.GetMoveRange());
            if (totalRange * 3.75f < goalAngle % 360 && 
                goalAngle % 360 <= (totalRange + moveParameter.GetMoveRange()) * 3.75f)
            {
                Debug.Log(moveParameter.GetMoveName());
            }
            totalRange += moveParameter.GetMoveRange();
        }
            
    }
}
