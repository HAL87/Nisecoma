using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureController : MonoBehaviour
{
    [SerializeField] private Transform nodesTransform;

    private Stack<int> route = new Stack<int>();
    private int nextNode;
    private Vector3 nextPosition;
    private Vector3 direction;
    private float walkSpeed = 3f;

    //スクリプト変数宣言
    private FigureParameter figureParameter;
    private BoardController boardController;

    [SerializeField] private float positionOffset;
    // Use this for initialization
    void Start()
    {
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

            transform.position += direction * walkSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, nextPosition) < 0.05f)
            {
                if (route.Count == 0)
                {
                    figureParameter.SetPosition(nextNode);
                    boardController.SetPhaseState(BoardController.PhaseState.Normal);
                    figureParameter.SetBeSelected(false);
                }

                else
                {
                    nextNode = route.Pop();
                    nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                               nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                               nodesTransform.GetChild(nextNode).position.z);
                    direction = (nextPosition - transform.position).normalized;
                    //figureParameter.SetPosition(nextNode);
                    //Debug.Log(figureParameter.GetPosition());
                }
            }
        }

    }
    public void OnUserAction()
    {
        boardController.FigureSelected(figureParameter.GetPlayerID(), figureParameter.GetFigureIDOnBoard());
    }

    public void SetRoute(Stack<int> _route)
    {
        Debug.Log("SetRoute");
        route = _route;
        //Stack<int> routecp = _route;
        //for (int i = 0; i < routecp.Count; i++) Debug.Log(routecp.Pop());
        route.Pop();
        nextNode = route.Pop();
        nextPosition = new Vector3(nodesTransform.GetChild(nextNode).position.x,
                                   nodesTransform.GetChild(nextNode).position.y + positionOffset,
                                   nodesTransform.GetChild(nextNode).position.z);
        direction = (nextPosition - transform.position).normalized;
        //Debug.Log(direction);
        boardController.SetPhaseState(BoardController.PhaseState.Walking);
    }
    
}


