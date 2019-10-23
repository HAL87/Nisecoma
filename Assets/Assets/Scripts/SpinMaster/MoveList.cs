using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MoveList : MonoBehaviourPunCallbacks
{

    BoardController boardController;

    // playerId: 技を出しているフィギュアの所持プレイヤーのID
    public delegate IEnumerator coroutineFincType(int playerId);

    public struct MoveEffect
    {
        public coroutineFincType moveEffect;
    }

    public List<coroutineFincType> moveEffects;

    // Start is called before the first frame update
    void Start()
    {
        boardController = GetComponent<BoardController>();
        moveEffects = new List<coroutineFincType>();
        SetMoveEffect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // moveEffectsに関数をセットする
    public void SetMoveEffect()
    {
        moveEffects.Add(NonEffect);
        moveEffects.Add(FlyAway);
    }

    // moveIdの関数を呼び出す
    public IEnumerator CallMoveEffect(int moveId, int playerId)
    {
        //moveEffects[moveId]();
        yield return StartCoroutine(moveEffects[moveId](playerId));
    }
    [PunRPC]
    public void FigureOneStepWalkRPC(int _targetNode, int _playerId, int _figureIdOnBoard)
    {
        GameObject opponentFigure = boardController.GetFigureFromFigures(_playerId, _figureIdOnBoard);
        StartCoroutine(opponentFigure.GetComponent<FigureController>().FigureOneStepWalk(_targetNode));
    }

    public void HyperSonic()
    {
        Debug.Log("はいぱー");

    }
    public void ShatolFlip()
    {
        Debug.Log("しゃとる");
    }
    public void Scychokinesisu()
    {
        Debug.Log("さいきね");
    }

    public void Bubble(int ret) {
        Debug.Log("あわわ～");
    }

    public void Dodge(int ret)
    {
        Debug.Log("かわした！");
    }

    public void QuickAttack(int ret)
    {
        Debug.Log("電光石火！");
    }

    public void WingAttack()
    {
        Debug.Log("つばさでうつ(PP56)");
    }

    // moveId = 0
    // 追加効果なし
    public IEnumerator NonEffect(int playerId)
    {
        Debug.Log("追加効果なし");
        yield break;
    }

    // moveId = 1
    // とぶ
    public IEnumerator FlyAway(int playerId)
    {
        GameObject currentFigure = boardController.GetCurrentFigure();  // 手番側(動くフィギュアが手番側なのかどうかで移動の関数が異なる)
        GameObject affectFigure;                                        // 飛ぶを出した側(実際に動くフィギュア)
        GameObject beAffectedFigure;                                    // 飛ぶを出された側(飛ぶ先の候補地探索に必要)

        // AfterBattle状態以外では何も行わない
        if (boardController.GetPhaseState() != BoardController.PhaseState.AfterBattle)
        {
            Debug.Log("飛びません");
            yield break;
        }

        Debug.Log("飛びます");
        // 技を出したのが手番側に対してどちらなのかによってそれぞれ初期化
        if (playerId == currentFigure.GetComponent<FigureParameter>().GetPlayerId())
        {
            affectFigure = boardController.GetCurrentFigure();
            beAffectedFigure = boardController.GetOpponentFigure();
        }
        else
        {
            affectFigure = boardController.GetOpponentFigure();
            beAffectedFigure = boardController.GetCurrentFigure();
        }

        // 着陸候補地
        List<int> landingCandidates = boardController.GetEdges()[beAffectedFigure.GetComponent<FigureParameter>().GetPosition()];
        foreach(int landing in landingCandidates)
        {
            // landing == 着陸可能地
            if (boardController.GetFigureOnBoard(landing) == null)
            {
                // 手番側の移動
                if(playerId == currentFigure.GetComponent<FigureParameter>().GetPlayerId())
                {
                    yield return affectFigure.GetComponent<FigureController>().FigureOneStepWalk(landing);
                }
                // 非手番側の移動
                else
                {
                    int figureIdOnBoard = affectFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard();

                    photonView.RPC(boardController.FIGURE_ONE_STEP_WALK_RPC, RpcTarget.Others, landing, playerId, figureIdOnBoard);

                    boardController.SetWaitFlag(true);
                    while (boardController.GetWaitFlagCustomProperty() == true)
                    {
                        yield return null;
                    }
                    boardController.SetWaitFlag(true);
                    Debug.Log("飛んだ");
                }
                break;
            }
        }

        yield break;
    }
}
