using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MoveList : MonoBehaviourPunCallbacks
{

    BoardController boardController;

    // playerId: 技を出しているフィギュアの所持プレイヤーのID
    public delegate IEnumerator coroutineFincType(int playerId, object arg);

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
    public IEnumerator CallMoveEffect(int moveId, int playerId, object arg)
    {
        //moveEffects[moveId]();
        yield return StartCoroutine(moveEffects[moveId](playerId, arg));
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
    public IEnumerator NonEffect(int playerId, object arg)
    {
        Debug.Log("追加効果なし");
        yield break;
    }

    // moveId = 1
    // とぶ
    public IEnumerator FlyAway(int playerId, object _nodeId)
    {
        GameObject currentFigure = boardController.GetCurrentFigure();  // 手番側(動くフィギュアが手番側なのかどうかで移動の関数が異なる)
        GameObject opponrntFigure = boardController.GetOpponentFigure();
        GameObject affectFigure;                                        // 飛ぶを出した側(実際に動くフィギュア)
        GameObject beAffectedFigure;                                    // 飛ぶを出された側(飛ぶ先の候補地探索に必要)
        List<int> landingCandidates;
        BoardController.PhaseState phaseState = boardController.GetPhaseState();

        

        switch (phaseState)
        {
        case BoardController.PhaseState.AfterBattle:
            // current(バトルを仕掛けた側)ならcurrent側をMoveEffectInputにする
            // opponentならopponent側をMoveEffectInput、currentはwhileループで待ち合わせする
            if (playerId == currentFigure.GetComponent<FigureParameter>().GetPlayerId())
            {
                beAffectedFigure = boardController.GetOpponentFigure();
                // 着陸候補地
                landingCandidates = boardController.GetEdges()[beAffectedFigure.GetComponent<FigureParameter>().GetPosition()];
                bool isAbleToFlyAway = false;
                foreach(int node in landingCandidates)
                {
                    if(boardController.GetFigureOnBoard(node) == null)
                    {
                        // 色付け
                        isAbleToFlyAway = true;
                        boardController.GetNodes().transform.GetChild(node).GetComponent<SpriteRenderer>().color = Color.magenta;
                    }
                }
                // 1個もなかったらyield break;
                if(isAbleToFlyAway == false)
                {
                    yield break;
                }
                boardController.SetPhaseStateSimple(BoardController.PhaseState.MoveEffectInput);
            }
            else
            {
                beAffectedFigure = boardController.GetCurrentFigure();
                // 着陸候補地
                landingCandidates = boardController.GetEdges()[beAffectedFigure.GetComponent<FigureParameter>().GetPosition()];
                bool isAbleToFlyAway = false;
                foreach(int node in landingCandidates)
                {
                    if(boardController.GetFigureOnBoard(node) == null)
                    {
                        // 色付け(相手の端末)
                        photonView.RPC(boardController.ILLUMINATE_NODE_RPC, RpcTarget.Others, node, 1);
                        isAbleToFlyAway = true;
                    }
                }
                // 1個もなかったらyield break;
                if(isAbleToFlyAway == false)
                {
                    yield break;
                }
                boardController.SetPhaseStateSimple(BoardController.PhaseState.Lock);
                photonView.RPC(boardController.SET_PHASE_STATE_SIMPLE_RPC, RpcTarget.Others, (int)BoardController.PhaseState.MoveEffectInput);
                boardController.SetWaitFlag(true);
                while (boardController.GetWaitFlagCustomProperty() == true)
                {
                    yield return null;
                }
                boardController.SetWaitFlag(true);
                StartCoroutine(boardController.SetPhaseState(BoardController.PhaseState.TurnEnd));
            }
            break;

        case BoardController.PhaseState.MoveEffectInput:
            Debug.Log("飛びます");
            // 技出したらphaseStateとflagを戻す
            // 技を出したのが手番側に対してどちらなのかによってそれぞれ初期化
            // 技を出した側の識別にmyPlayerIdを用いているが、先行後攻がランダムになりplayerIdとwhichTurnが一致しなくなったら通用しない
            if (boardController.GetMyPlayerId() == boardController.GetWhichTurn())
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
            landingCandidates = boardController.GetEdges()[beAffectedFigure.GetComponent<FigureParameter>().GetPosition()];
            foreach(int landing in landingCandidates)
            {
                // landing == 着陸可能地
                if (landing == (int)_nodeId)
                {
                    // 全フィギュアの包囲フラグを確認
                    yield return affectFigure.GetComponent<FigureController>().FigureOneStepWalk(landing);
                    for (int i = 0; i < BoardController.NUMBER_OF_PLAYERS; i++)
                    {
                        foreach (GameObject figure in boardController.GetFigures()[i])
                        {
                            boardController.IsSurrounded(figure);
                        }
                    }
                    // 包囲フラグあるフィギュアに対して包囲処理
                    for (int i = 0; i < BoardController.NUMBER_OF_PLAYERS; i++)
                    {
                        foreach (GameObject figure in boardController.GetFigures()[i])
                        {
                            if (figure.GetComponent<FigureParameter>().GetBeSurroundedFlag() == true)
                            {
                                yield return boardController.KnockedOutBySurrounding(figure);
                            }
                        }
                    }

                    boardController.SetWaitFlagCustomProperty(false);

                    if (boardController.GetWhichTurn() != boardController.GetMyPlayerId())
                    {
                        foreach(int node in landingCandidates)
                        {
                            photonView.RPC(boardController.ILLUMINATE_NODE_RPC, RpcTarget.Others, node, 0);
                        }
                        boardController.SetPhaseStateSimpleRPC((int)BoardController.PhaseState.Lock);
                    }
                    else
                    {
                        foreach(int node in landingCandidates)
                        {
                            boardController.GetNodes().transform.GetChild(node).GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        StartCoroutine(boardController.SetPhaseState(BoardController.PhaseState.TurnEnd));
                    }
                    break;
                }
            }
            break;

        case BoardController.PhaseState.MoveEffectFigureSelected:
            // とぶでは何も行わない
            break;

        // 該当状態以外では何も行わない
        default:
            Debug.Log("飛びません");
            break;
        }

        

        yield break;
    }
}
