using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    [SerializeField] private GameObject boardMaster;
    private BoardController boardController;
    private NodeParameter nodeParameter;
    // Start is called before the first frame update
    void Start()
    {
        //boardController = boardMaster.GetComponent<BoardController>();
        nodeParameter = GetComponent<NodeParameter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    public void OnUserAction()
    {
        boardController.NodeSelected(nodeParameter.GetNodeID());
    }
    */
}
