using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinController : MonoBehaviour
{
    private BoardController boardController;
    [SerializeField] private List<GameObject> datadisks;
    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();

        RouletteMaker[] rouletteMaker = new RouletteMaker[2];

        int currentPlayerID = boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID();
        Debug.Log("currentPlayerIDは" + currentPlayerID);
        rouletteMaker[currentPlayerID] = datadisks[currentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[currentPlayerID].CreateRulette(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData());

        int opponentPlayerID = boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetPlayerID();
        Debug.Log("opponentPlayerIDは" + opponentPlayerID);
        rouletteMaker[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[opponentPlayerID].CreateRulette(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData());

        DiskSpin[] diskSpin = new DiskSpin[2];
        diskSpin[currentPlayerID] = datadisks[currentPlayerID].GetComponent<DiskSpin>();
        diskSpin[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<DiskSpin>();

        StartCoroutine(diskSpin[currentPlayerID].Spin(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData()));
        StartCoroutine(diskSpin[opponentPlayerID].Spin(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
