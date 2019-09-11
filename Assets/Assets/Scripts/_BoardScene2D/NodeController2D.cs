using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController2D : MonoBehaviour
{
    private BoardController2D boardController;
    private NodeParameter nodeParameter;
    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController2D>();
        nodeParameter = GetComponent<NodeParameter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnUserAction()
    {
        Debug.Log(nodeParameter.GetNodeID());
        StartCoroutine(boardController.NodeClicked(nodeParameter.GetNodeID()));
        //boardController.NodeClicked(nodeParameter.GetNodeID());
    }
    
}
