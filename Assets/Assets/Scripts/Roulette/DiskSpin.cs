using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiskSpin : MonoBehaviour
{
    // SpinシーンのPlayerにセット。地震やら根源はまた後日......

    // ここらへんの値は必要とあれば変更（大体大丈夫そう）
    // 目的の角度(360×N + x (0 < x <= 360))
    private float goalAngle;
    private int N = 10;
    // 総回転時間
    private float totalTime = 1.5f;
    // スローになるまでの時間
    private float normalTime = 1.0f;
    // 何秒ごとに回転関数を呼び出すか
    private float clock = 0.05f;

    // スローになるまでの角速度
    private float omega;
    // スローになった後の角速度の減衰率
    private float deltaOmega;
    // スローになった後の角速度
    private float omegaSlow;

    // while文の中身の繰り返し回数
    private int loopCount;

    // 初期位置のズレ（Wチャンス時などを考慮）
    // プレイヤー0は初期0度、プレイヤー1は初期180度
    [SerializeField] public float offsetAngle;

    // 注意: 今のプログラムの仕様上、totalTime, normalTimeをclockで割り切れるようにしなければいけません
    // 最初はclockを累加したもの（経過時間）とnormalTime, totalTimeを比較していたのですが、
    // 時間を扱うと厳密には0.0001fのズレで条件が満たされなかったり満たされたりみたいになってダルかった
    // ので今のwhile文の中身の繰り返し回数で条件分岐を管理しています

    // ワザの情報スクリプト取得
    private MoveParameter moveParameter;

    // Start is called before the first frame update
    void Start()
    {
    }

    // 引数はGameMasterから渡されるワザのデータ）
    public IEnumerator Spin(GameObject data)
    {
        yield return new WaitForSeconds(0.1f);

        loopCount = 0;

        // 初期位置から目標点までの回転角度
        goalAngle = 360 * N + Random.Range(0, 360);

        // スローになるまでの角速度立式
        omega = 2 * goalAngle / (totalTime + normalTime);
        deltaOmega = omega * clock / (totalTime - normalTime + clock);
        omegaSlow = omega;

        while (loopCount < totalTime/clock)
        {
            // スローになるまで
            if (loopCount < normalTime/clock)
            {
                // 等角速度運動
                transform.Rotate(new Vector3(0, 0, omega * clock));
            }
            
            else if(loopCount < totalTime/clock)
            {
                // 速度をループ回数に対して階段状に小さくする
                omegaSlow = omegaSlow - deltaOmega;

                transform.Rotate(new Vector3(0, 0, omegaSlow * clock));
            }
            
            loopCount++;

            // clock秒ごとにwhile文を呼び出す
            yield return new WaitForSeconds(clock);
        }
        // 回転終了
        Debug.Log("ゴールは" + goalAngle % 360);

        // ピース幅を順番に見て、goalAngle %360がどの範囲にあるかを調べる
        float totalRange = 0;
        for (int i = 0; i < data.transform.childCount; i++)
        {
            // 各ワザのパラメータ取得
            moveParameter = data.transform.GetChild(i).GetComponent<MoveParameter>();

            // 不等式の両端
            float leftAngle = totalRange * 3.75f;
            float rightAngle = leftAngle + moveParameter.GetMoveRange() * 3.75f;

            // Debug.Log(leftAngle + "から" + rightAngle);
            if (leftAngle <= (goalAngle + offsetAngle) % 360 && 
                (goalAngle + offsetAngle) % 360 < rightAngle)
            {
                // goalAngleに該当するワザ
                Debug.Log("getMoveName = " + moveParameter.GetMoveName());
                Debug.Log("getMoveColor = " + moveParameter.GetMoveColorName());
                moveParameter.OnInitDone.Invoke();
                yield return moveParameter;
            }
            totalRange += moveParameter.GetMoveRange();
        }
        // 次のスピンのためにオフセットを準備
        offsetAngle += goalAngle % 360;

        // ここでmoveParameterから両者の技を参照して色判定を行う
        // この実装だとfor文でcurrent側のmoveParameterが捨てられているので書き方を変える
        // 勝敗結果と技の効果はSpinControllerに返した上、さらにboardControllerに渡したほうがいいよね～

    }
}
