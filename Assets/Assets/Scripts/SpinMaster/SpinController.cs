using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinController : MonoBehaviour
{
    private BoardController boardController;
    [SerializeField] private List<GameObject> datadisks;
    //変更点
    public static (int result, bool currentMoveAwake, bool opponentMoveAwake, bool currentDeath, bool oppnentDeath) BattleResult;

    [SerializeField] private Text[] moveText = new Text[2];
    [SerializeField] private Text[] battleResultText = new Text[2];
 //   [SerializeField] private Text buttleResultText0;
 //   [SerializeField] private Text buttleResultText1;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();

        RouletteMaker[] rouletteMaker = new RouletteMaker[2];

        int currentPlayerID = boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID();
        Debug.Log("currentPlayerIDは" + currentPlayerID);
        rouletteMaker[currentPlayerID] = datadisks[currentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[currentPlayerID].CreateRulette(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData());

        int opponentPlayerID = boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID();
        Debug.Log("opponentPlayerIDは" + opponentPlayerID);
        rouletteMaker[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[opponentPlayerID].CreateRulette(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData());

        DiskSpin[] diskSpin = new DiskSpin[2];
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


        //moveText0.GetComponent<Text>();
        //moveText1.GetComponent<Text>();

        moveText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = mp0.GetMoveName();
        moveText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = mp1.GetMoveName();

        Debug.Log(mp0.GetMoveName() + " vs " + mp1.GetMoveName());
        if (0 == BattleResult.result)
        {
            Debug.Log(mp0.GetMoveName() + "の勝ち！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Win!!";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Lose...";
        }
        else if(1 == BattleResult.result)
        {
            Debug.Log(mp1.GetMoveName() + "の勝ち！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Lose...";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Win!!";
        }
        else
        {
            Debug.Log("引き分け！");
            battleResultText[boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Draw";
            battleResultText[boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID()].text = "Draw";
        }
        //yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        SceneManager.LoadScene("BoardScene");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // バトルの勝敗判定を行う
    // 第一引数: Player0    ※currentPlayerではない
    // 第二引数: Player1    ※opponentPlayerではない
    // Player0が勝った場合return 0
    // Player1が負けた場合return 1
    // 引き分けの場合return 2
    public (int result, bool currentMoveAwake, bool opponentMoveAwake, bool currentDeath, bool oppnentDeath) Judge(MoveParameter Player0, MoveParameter Player1)
    {
        // 白 vs ○○
        if(MoveParameter.MoveOfColorName.White == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (0, true, false, false, true);
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                    || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false);
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if(Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return (0, true, true, false, true);
                }
                else if(Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return (1, true, true, true, false);
                }
                else
                {
                    return (2, true, true, false, false);
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
                return (0, true, false, false, true);
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false);
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
               || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if (Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return (0, true, true, false, true);
                }
                else if (Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return (1, true, true, true, false);
                }
                else
                {
                    return (2, true, true, false, false);
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
                return (0, true, false, false, false);
            }
            // 負け(気絶)
            else if (MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                return (1, false, true, true, false);
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false);
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName())
            {
                if (Player0.GetMoveNumberOfStar() > Player1.GetMoveNumberOfStar())
                {
                    return (0, true, false, false, false);
                }
                else if (Player0.GetMoveNumberOfStar() < Player1.GetMoveNumberOfStar())
                {
                    return (1, false, true, false, false);
                }
                else
                {
                    return (2, false, false, false, false);
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
                return (0, true, false, false, false);
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (2, false, false, false, false);
            }
        }
        // ミス vs ○○
        else if (MoveParameter.MoveOfColorName.Red == Player0.GetMoveColorName())
        {
            // 負け(気絶)
            if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                return (1, false, true, true, false);
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return (1, false, true, false, false);
            }
            // 引き分け
            else if ( MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return (2, false, false, false, false);
            }
        }

        // default(いらない)
        return (2, false, false, false, false);
    }

    public static (int, bool, bool, bool, bool) GetBattleResult()
    {
        return BattleResult;
    }
}
