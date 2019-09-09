using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureController : MonoBehaviour
{
    private Transform nodesTransform;

    //private Stack<int> route = new Stack<int>();
    //private int nextNode;
    //private Vector3 nextPosition;
    private float walkSpeed = 5f;
    private float fastSpeed = 10f;
   // private GameObject walkFigure;

    //スクリプト変数宣言
    private FigureParameter figureParameter;
    private BoardController boardController;

    [SerializeField] private float positionOffset;
    // Use this for initialization
    void Start()
    {
        nodesTransform = GameObject.Find("Nodes").transform;
        //positionに対応するノードの位置を初期位置とする
        //先にBoardMasterのStartで各フィギュアにベンチ番号を割り振っている
        figureParameter = GetComponent<FigureParameter>();
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
        transform.position = new Vector3(nodesTransform.GetChild(figureParameter.GetPosition()).position.x,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.y + positionOffset,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.z);
    }

    private void Update()
    {
        //Walking状態の時,選択したフィギュアのみを動かす
        // && figureParameter.GetBeSelected())
        /*if (boardController.GetPhaseState() == BoardController.PhaseState.Walking && 
            this.gameObject == walkFigure)
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
                    walkFigure = null;
                    //遷移6番
                    boardController.SetPhaseState(BoardController.PhaseState.AfterWalk);
                    //figureParameter.SetBeSelected(false);
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
        }*/
        if(boardController.GetPhaseState() == BoardController.PhaseState.Battle)
        {
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

    }
    public void OnUserAction()
    {
        boardController.FigureClicked(figureParameter.GetPlayerID(), figureParameter.GetFigureIDOnBoard());
    }

    //FigureSelect状態でcandidates内のノードが選択されたとき呼ばれる
   /* public void SetRoute(Stack<int> _route)
    {
        walkFigure = boardController.GetCurrentFigure();
        route = _route;
        //初期位置を捨てる
        route.Pop();
        nextNode = route.Pop();
        nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                   nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                   nodesTransform.GetChild(nextNode).position.z);
        boardController.SetPhaseState(BoardController.PhaseState.Walking);
    }*/

    //routeに沿って1歩ずつ動く
    public IEnumerator Figurewalk(Stack<int> _route)
    {

        boardController.SetPhaseState(BoardController.PhaseState.Walking);
        int nextNode = -1;
        _route.Pop();
        //routeの残り数だけ繰り返す
        while (_route.Count > 0)
        {
            nextNode = _route.Pop();
            Vector3 nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                               nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                               nodesTransform.GetChild(nextNode).position.z);

            while (Vector3.Distance(transform.position, nextPosition) > 0.1f)
            {
                transform.position += (nextPosition - transform.position).normalized * walkSpeed * Time.deltaTime;
                yield return null;
            }
            //1マス移動する度にPositionを更新
            //GetComponent<FigureParameter>().SetPosition(nextNode);
        }
        //目的地に着いたときだけPositionを更新
        GetComponent<FigureParameter>().SetPosition(nextNode);
    }
    //目的地まで一気に動くタイプ
    public IEnumerator FigureOneStepWalk(int _targetNode)
    {
        boardController.SetPhaseState(BoardController.PhaseState.Walking);
        Vector3 targetPosition = new Vector3(nodesTransform.GetChild(_targetNode).position.x,
                                   nodesTransform.GetChild(_targetNode).position.y + positionOffset,
                                   nodesTransform.GetChild(_targetNode).position.z);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position += (targetPosition - transform.position).normalized * fastSpeed * Time.deltaTime;
            yield return null;
        }
        GetComponent<FigureParameter>().SetPosition(_targetNode);
    }

}


