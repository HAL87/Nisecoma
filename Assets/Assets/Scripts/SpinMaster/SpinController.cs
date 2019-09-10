using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinController : MonoBehaviour
{
    private BoardController boardController;
    [SerializeField] private List<GameObject> datadisks;
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

        int BattleResult = Judge(mp0, mp1);

        Debug.Log(mp0.GetMoveName() + " vs " + mp1.GetMoveName());
        if (0 == BattleResult)
        {
            Debug.Log(mp0.GetMoveName() + "の勝ち！");
        }
        else if(1 == BattleResult)
        {
            Debug.Log(mp1.GetMoveName() + "の勝ち！");
        }
        else
        {
            Debug.Log("引き分け！");
        }

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
    int Judge(MoveParameter Player0, MoveParameter Player1)
    {
        // 白 vs ○○
        if(MoveParameter.MoveOfColorName.White == Player0.GetMoveColorName())
        {
            // 勝ち
            if (MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return 0;
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                    || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return 1;
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if(Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return 0;
                }
                else if(Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return 1;
                }
                else
                {
                    return 2;
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
                return 0;
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return 1;
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
               || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName())
            {
                if (Player0.GetMovePower() > Player1.GetMovePower())
                {
                    return 0;
                }
                else if (Player0.GetMovePower() < Player1.GetMovePower())
                {
                    return 1;
                }
                else
                {
                    return 2;
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
                return 0;
            }
            // 負け
            else if (MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return 1;
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName())
            {
                if (Player0.GetMoveNumberOfStar() > Player1.GetMoveNumberOfStar())
                {
                    return 0;
                }
                else if (Player0.GetMoveNumberOfStar() < Player1.GetMoveNumberOfStar())
                {
                    return 1;
                }
                else
                {
                    return 2;
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
                return 0;
            }
            // 引き分け
            else if (MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return 2;
            }
        }
        // ミス vs ○○
        else if (MoveParameter.MoveOfColorName.Red == Player0.GetMoveColorName())
        {
            // 負け
            if (MoveParameter.MoveOfColorName.White == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Gold == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Purple == Player1.GetMoveColorName()
                || MoveParameter.MoveOfColorName.Blue == Player1.GetMoveColorName())
            {
                return 1;
            }
            // 引き分け
            else if ( MoveParameter.MoveOfColorName.Red == Player1.GetMoveColorName())
            {
                return 2;
            }
        }

        return -1;
    }
}
