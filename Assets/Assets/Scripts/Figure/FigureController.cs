using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureController : MonoBehaviour
{
    [SerializeField] private Transform nodesTransform;
    [SerializeField] private GameObject boardMaster;
    //スクリプト変数宣言
    private FigureParameter figureParameter;
    private BoardController boardController;

    [SerializeField] private float positionOffset;
    // Use this for initialization
    void Start()
    {
        figureParameter = GetComponent<FigureParameter>();
        boardController = boardMaster.GetComponent<BoardController>();
        transform.position = new Vector3(nodesTransform.GetChild(figureParameter.GetPosition()).position.x,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.y + positionOffset,
                                         nodesTransform.GetChild(figureParameter.GetPosition()).position.z);
    }

    public void OnUserAction()
    {
        Debug.Log("Hello");
        boardController.WalkPrepare(figureParameter.GetFigureIDOnBoard());
    }
}


