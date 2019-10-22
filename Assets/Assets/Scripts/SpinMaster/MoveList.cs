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

    public IEnumerator FlyAway(int playerId)
    {
        GameObject currentFigure;
        GameObject opponentFigure;

        //BoardController.PhaseState phaseState;
        if (boardController.GetPhaseState() == BoardController.PhaseState.AfterBattle)
        {
            Debug.Log("飛びます");

            currentFigure = boardController.GetCurrentFigure();
            opponentFigure = boardController.GetOpponentFigure();

            // 着陸候補地
            List<int> landingCandidates = boardController.GetEdges()[opponentFigure.GetComponent<FigureParameter>().GetPosition()];
            foreach(int landing in landingCandidates)
            {
                if (boardController.GetFigureOnBoard(landing) == null)
                {
                    if(playerId == currentFigure.GetComponent<FigureParameter>().GetPlayerId())
                    {
                        yield return currentFigure.GetComponent<FigureController>().FigureOneStepWalk(landing);
                    }
                    else
                    {
                        //photonView.RPC(boardController.FIGURE_ONE_STEP_WALK, RpcTarget.Others, landing);
                        opponentFigure.GetComponent<FigureController>().FigureOneStepWalkRPC(landing);
                        yield return null;
                    }
                    break;
                }
            }
        }
        else
        {
            Debug.Log("飛びません");
        }
        yield break;
    }

    // 追加効果なし
    public IEnumerator NonEffect(int playerId)
    {
        Debug.Log("追加効果なし");
        yield return null;
    }
}
