using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class SpinController : MonoBehaviour
{
    private BoardController boardController;
    [SerializeField] private List<GameObject> datadisks;
    // 変更点
    public static (int result, bool currentMoveAwake, bool opponentMoveAwake, bool currentDeath, bool oppnentDeath, int currentMoveId, int opponentMoveId) BattleResult;

    [SerializeField] private Text[] moveText = new Text[BoardController.NUMBER_OF_PLAYERS];
    [SerializeField] private Text[] battleResultText = new Text[BoardController.NUMBER_OF_PLAYERS];

    bool receiveFlag = false;

    // Start is called before the first frame update
    private void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator SpinStart()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();

        RouletteMaker[] rouletteMaker = new RouletteMaker[BoardController.NUMBER_OF_PLAYERS];

        int currentPlayerID = boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerId();
        Debug.Log("currentPlayerIDは" + currentPlayerID);
        rouletteMaker[currentPlayerID] = datadisks[currentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[currentPlayerID].CreateRulette(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData());

        int opponentPlayerID = boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerId();
        Debug.Log("opponentPlayerIDは" + opponentPlayerID);
        rouletteMaker[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[opponentPlayerID].CreateRulette(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData());

        DiskSpin[] diskSpin = new DiskSpin[BoardController.NUMBER_OF_PLAYERS];
        diskSpin[currentPlayerID] = datadisks[currentPlayerID].GetComponent<DiskSpin>();
        diskSpin[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<DiskSpin>();

        var ie0 = diskSpin[currentPlayerID].Spin(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData());
        var ie1 = diskSpin[opponentPlayerID].Spin(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData());
        var coroutine0 = StartCoroutine(ie0);
        var coroutine1 = StartCoroutine(ie1);
        yield return coroutine0;
        yield return coroutine1;
        MoveParameter mp0 = (MoveParameter)ie0.Current;
        MoveParameter mp1 = (MoveParameter)ie1.Current;

        BattleResult = Judge(mp0, mp1);


        // moveText0.GetComponent<Text>();
        // moveText1.GetComponent<Text>();

        moveText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = mp0.GetMoveName();
        moveText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = mp1.GetMoveName();

        Debug.Log(mp0.GetMoveName() + " vs " + mp1.GetMoveName());
        if (0 == BattleResult.result)
        {
            Debug.Log(mp0.GetMoveName() + "の勝ち！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Win!!";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Lose...";
        }
        else if (1 == BattleResult.result)
        {
            Debug.Log(mp1.GetMoveName() + "の勝ち！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Lose...";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Win!!";
        }
        else
        {
            Debug.Log("引き分け！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Draw";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerId()].text = "Draw";
        }
        // yield return new WaitForSeconds(0.5f);
        int myPlayerId = boardController.GetMyPlayerId();
        int whichTurn = boardController.GetWhichTurn();

        if (myPlayerId == whichTurn)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            // スピンの情報を初期化する
            ClearSpin();
            boardController.OnBattleEnd();
            // BattleEndでバトルされる側に「バトル終わったよ」と送信
            StartCoroutine(boardController.SetPhaseState(BoardController.PhaseState.BattleEnd));
        }
        else
        {
            while (receiveFlag == false)
            {
                yield return null;
            }
            receiveFlag = false;
            ClearSpin();
            boardController.OnBattleEnd();
        }
    }

    // バトルの勝敗判定を行う
    // 第一引数: Player0    ※currentPlayerではない
    // 第二引数: Player1    ※opponentPlayerではない
    // Player0が勝った場合return 0
    // Player1が負けた場合return 1
    // 引き分けの場合return 2
    public (int result, bool currentMoveAwake, bool opponentMoveAwake, bool currentDeath, bool oppnentDeath, int currentMoveId, int opponentMoveId) Judge(MoveParameter Player0, MoveParameter Player1)
    {
        // 白 vs ○○
        if (MoveParameter.MoveOfColorName.White == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (0, true, false, false, true, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                    || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if (Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return (0, true, true, false, true, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else if (Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return (1, true, true, true, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else
                {
                    return (2, true, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
            }
        }
        // 金 vs ○○
        else if (MoveParameter.MoveOfColorName.Gold == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                    || MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (0, true, false, false, true, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
               || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if (Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return (0, true, true, false, true, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else if (Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return (1, true, true, true, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else
                {
                    return (2, true, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
            }
        }
        // 紫 vs ○○
        else if (MoveParameter.MoveOfColorName.Purple == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (0, true, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 負け(気絶)
            else if (MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                return (1, false, true, true, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName())
            {
                if (Player0.GetMoveNumberOfStar() > Player1.GetMoveNumberOfStar())
                {
                    return (0, true, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else if (Player0.GetMoveNumberOfStar() < Player1.GetMoveNumberOfStar())
                {
                    return (1, false, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
                else
                {
                    return (2, false, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
                }
            }
        }
        // 青 vs ○○
        else if (MoveParameter.MoveOfColorName.Blue == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (0, true, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (2, false, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
        }
        // ミス vs ○○
        else if (MoveParameter.MoveOfColorName.Red == Player0.GetMoveColorName())
        {
            // 負け(気絶)
            if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                return (1, false, true, true, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (2, false, false, false, false, Player0.GetMoveId(), Player1.GetMoveId());
            }
        }

        // default(いらない)
        return (2, false, false, false, false, 0, 0);
    }

    public static (int, bool, bool, bool, bool, int currentMoveId, int opponentMoveId) GetBattleResult()
    {
        return BattleResult;
    }

    // バトルが終わりボード画面に戻る時(これで不都合あれば次のバトル開始前に変える)に
    // ピースを削除しspinTextを初期化する
    private void ClearSpin()
    {
        //ピースを削除
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("RouletteImage");
        foreach (GameObject piece in pieces)
        {
            Destroy(piece);
        }
        for (int i = 0; i < 2; i++)
        {
            // ディスクの回転位置初期化
            datadisks[i].transform.rotation = Quaternion.Euler(0, 0, 0);

            // 回転のオフセットを初期化

            datadisks[i].GetComponent<DiskSpin>().offsetAngle = 45 + 180 * i;

            // UIの初期化
            moveText[i].text = "";
            battleResultText[i].text = "";
        }
    }

    public void SetReceiveFlag(bool _flag)
    {
        receiveFlag = _flag;
    }
}
