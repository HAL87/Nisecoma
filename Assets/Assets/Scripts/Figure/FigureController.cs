using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureController : MonoBehaviour
{
    private Transform nodesTransform;

    // private Stack<int> route = new Stack<int>();
    // private int nextNode;
    // private Vector3 nextPosition;
    private float walkSpeed = 5f;
    private float fastSpeed = 10f;
   // private GameObject walkFigure;

    // スクリプト変数宣言
    private FigureParameter figureParameter;
    private BoardController boardController;
    // Use this for initialization
    void Start()
    {
        nodesTransform = GameObject.Find("Nodes").transform;
        // positionに対応するノードの位置を初期位置とする
        // 先にBoardMasterのStartで各フィギュアにベンチ番号を割り振っている
        figureParameter = GetComponent<FigureParameter>();
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
        //transform.position = nodesTransform.GetChild(figureParameter.GetPosition()).position;
    }

    private void Update()
    {
        /*
        if(boardController.GetPhaseState() == BoardController2D.PhaseState.Battle)
        {
            
            this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }*/
        

    }
    public void OnUserAction()
    {
        boardController.FigureClicked(figureParameter.GetPlayerId(), figureParameter.GetFigureIdOnBoard());
    }

    // routeに沿って1歩ずつ動く
    public IEnumerator Figurewalk(Stack<int> _route)
    {
        int nextNode = -1;
        _route.Pop();
        // routeの残り数だけ繰り返す
        while (_route.Count > 0)
        {
            nextNode = _route.Pop();
            Vector3 nextPosition = nodesTransform.GetChild(nextNode).position;

            while (Vector3.Distance(transform.position, nextPosition) > 0.1f)
            {
                transform.position += (nextPosition - transform.position).normalized * walkSpeed * Time.deltaTime;
                yield return null;
            }
            // 1マス移動する度にPositionを更新
            // GetComponent<FigureParameter>().SetPosition(nextNode);
        }
        // 目的地に着いたときだけPositionを更新
        GetComponent<FigureParameter>().SetPosition(nextNode);
    }
    // 目的地まで一気に動くタイプ
    public IEnumerator FigureOneStepWalk(int _targetNode)
    {
        Vector3 targetPosition = nodesTransform.GetChild(_targetNode).position;
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position += (targetPosition - transform.position).normalized * fastSpeed * Time.deltaTime;

            yield return null;
        }
        GetComponent<FigureParameter>().SetPosition(_targetNode);
    }  
}


