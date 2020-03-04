using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class FigureParameter : MonoBehaviourPunCallbacks, IPunObservable
{
    [field: SerializeField] [field: RenameField("Mp")]
    public int Mp { get; private set; }

    [field: SerializeField] [field: RenameField("Data")]
    public GameObject Data { get; private set; }

    [field: SerializeField]  [field: RenameField("AttackRange")]
    public int AttackRange { get; private set; } = 1;

    [field: SerializeField] [field: RenameField("WaitCounter")]
    public GameObject WaitCounter { get; private set; }
    [SerializeField] private Text waitCounterText;
    public bool BeSelected { get; set; } = false;

    // ゲーム開始時にboardMasterによりセットされるID
    public int FigureIdOnBoard { get; set; }
    public int PlayerId { get; set; }
    public int BenchId { get; set; }
    public int Position { get;  set; }
    public int WaitCount { get; private set; } = 0;  // ウェイト デフォルトは0

    //フラグ
    public bool BeSurrounded { get; set; } = false;

    // ウェイトカウントをセットする
    // ウェイト1以上の場合はカウンターを表示する
    [PunRPC]
    public void SetWaitCount(int _waitCount)
    {
        WaitCount = _waitCount;
        if(1 <= WaitCount)
        {
            waitCounterText.text = "" + WaitCount;
            WaitCounter.SetActive(true);
        }
    }

    // ウェイトを1つ減らす
    // 正値の保証は使用者が行う
    public void DecreaseWaitCount()
    {
        WaitCount--;
        waitCounterText.text = "" + WaitCount;
        if (1 <= WaitCount)
        {
            WaitCounter.SetActive(true);
        }
        else
        {
            WaitCounter.SetActive(false);
        }
    }


    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //データの送信
            stream.SendNext(Position);
        }
        else
        {
            Position = (int)stream.ReceiveNext();
            // SetPosition(position);
            //データの受信
        }
    }
}
