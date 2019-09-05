using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureController : MonoBehaviour
{
    [SerializeField] private Transform nodesTransform;

    private Stack<int> route = new Stack<int>();
    private int nextNode;
    private Vector3 nextPosition;
    private float walkSpeed = 3f;

    //スクリプト変数宣言
    private FigureParameter figureParameter;
    private BoardController boardController;

    [SerializeField] private float positionOffset;
    // Use this for initialization
    void Start()
    {
        //positionに対応するノードの位置を初期位置とする（実際はベンチ）
        figureParameter = GetComponent<FigureParameter>();
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
        transform.position = new Vector3(nodesTransform.GetChild(figureParameter.GetPosition()).position.x,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.y + positionOffset,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.z);
    }

    private void Update()
    {
        //Walking状態の時,選択したフィギュアのみを動かす
        if (boardController.GetPhaseState() == BoardController.PhaseState.Walking && figureParameter.GetBeSelected())
        {

            transform.position += (nextPosition - transform.position).normalized * walkSpeed * Time.deltaTime;
            //次に行きたいノードの場所に来た時
            if (Vector3.Distance(transform.position, nextPosition) < 0.05f)
            {
                //もうroute内にノードがない = destNodeに到着したとき
                if (route.Count == 0)
                {
                    //フィギュアの現在地を更新
                    figureParameter.SetPosition(nextNode);
                    //ノーマル状態に戻る（実際はAfterWalkだが）
                    boardController.SetPhaseState(BoardController.PhaseState.Normal);
                    figureParameter.SetBeSelected(false);
                }

                else
                {
                    //次に行きたいノードの更新
                    nextNode = route.Pop();
                    nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                               nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                               nodesTransform.GetChild(nextNode).position.z);
                }
            }
        }

    }
    public void OnUserAction()
    {
        boardController.FigureSelected(figureParameter.GetPlayerID(), figureParameter.GetFigureIDOnBoard());
    }

    //FigureSelect状態でcandidates内のノードが選択されたとき呼ばれる
    public void SetRoute(Stack<int> _route)
    {
        route = _route;
        //初期位置を捨てる
        route.Pop();
        nextNode = route.Pop();
        nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                   nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                   nodesTransform.GetChild(nextNode).position.z);
        boardController.SetPhaseState(BoardController.PhaseState.Walking);
    }
    
}


