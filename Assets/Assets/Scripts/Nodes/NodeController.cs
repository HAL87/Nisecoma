using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    private BoardController boardController;
    private NodeParameter nodeParameter;
    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
        nodeParameter = GetComponent<NodeParameter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnUserAction()
    {
        StartCoroutine(boardController.NodeClicked(nodeParameter.GetNodeID()));
        //boardController.NodeClicked(nodeParameter.GetNodeID());
    }
    
}
