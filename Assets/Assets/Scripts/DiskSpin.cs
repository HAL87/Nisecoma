using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskSpin : MonoBehaviour
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

    //スローになった後の角速度
    private float omeganext;

    //累計経過時間
    private float elapsedTime;

    //何秒ごとに回転関数を呼び出すか
    [SerializeField] private float clock = 0.01f;
    //FixedUpdate毎(0.01秒)の回転角
    private float rotateAngle;

    //合計回転角度
    float totalAngle;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spin());
    }

    IEnumerator Spin()
    {
        //回転開始からの時間
        elapsedTime = 0;

        //初期位置から目標点までの回転角度
        goalAngle = 360 * N + Random.Range(0, 360);

        //スローになるまでの角速度立式
        omega = 2 * goalAngle / (normalTime + totalTime);

        omeganext = 0;
        while (elapsedTime < totalTime)
        {
            
            if (elapsedTime <normalTime)
            {
                transform.Rotate(new Vector3(0, 0, omega * clock));
            }
            
            else if(elapsedTime < totalTime)
            {
                omeganext = -omega * (elapsedTime - totalTime) / (totalTime - normalTime);
                transform.Rotate(new Vector3(0, 0, omeganext * clock));
            }
            elapsedTime += clock;
            Debug.Log(elapsedTime);
            yield return new WaitForSeconds(clock);
        }
    }
}
