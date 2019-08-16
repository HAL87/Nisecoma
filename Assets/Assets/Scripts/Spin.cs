using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    //回転している時間(ms)
    private int rotateTime = 1000;
    //回転数
    private int rotateCount = 15;
    //速度遅くなってから回る角度
    float addAngle = 180;
    float addTime = 500;

    //累計回転角度
    private float elapsedAngle;
    //累計経過時間
    private float elapsedTime;

    //FixedUpdate毎(0.01秒)の回転角
    private float rotateAngle;
    //合計回転角度
    float totalAngle;

    // Start is called before the first frame update
    void Start()
    {
        elapsedAngle = 0;
        elapsedTime = 0;
        rotateAngle = 0;
        totalAngle = 0;
        //初期位置から目標点までの角度
        int goalAngle = Random.Range(0, 361);

        totalAngle = 360 * rotateCount + goalAngle;

        rotateAngle = totalAngle / (rotateTime / 10);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(elapsedTime);
        elapsedAngle += rotateAngle;
        if (elapsedAngle < totalAngle)
        {
            transform.Rotate(new Vector3(0, rotateAngle, 0));
        }
        else if (elapsedTime < addTime)
        {
            elapsedTime += Time.deltaTime * 1000;
            //Debug.Log(elapsedTime);
            transform.Rotate(new Vector3(0, rotateAngle - rotateAngle * elapsedTime / addTime, 0));
        }
        if (elapsedTime >= addTime)
        {
            elapsedAngle = 0;
            elapsedTime = 0;
            rotateAngle = 0;
            totalAngle = 0;
            int goalAngle = Random.Range(0, 361);

            totalAngle = 360 * rotateCount + goalAngle;

            rotateAngle = totalAngle / (rotateTime / 10);
            GetComponent<Spin>().enabled = false;
        }
    }
}
