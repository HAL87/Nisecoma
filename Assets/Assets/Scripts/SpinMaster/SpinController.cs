using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinController : MonoBehaviour
{
    private RouletteMaker[] rouletteMaker = new RouletteMaker[2];
    //private RouletteMaker ruletteMaker1;
    private BoardController boardController;
    private DiskSpin diskSpin0;
    private DiskSpin diskSpin1;
    [SerializeField] private List<GameObject> datadisks;
    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();

        int currentPlayerID = boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID();
        rouletteMaker[currentPlayerID] = datadisks[currentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[currentPlayerID].CreateRulette(boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetData());

        int opponentPlayerID = boardController.GetCurrentFigure().GetComponent<FigureParameter>().GetPlayerID();
        rouletteMaker[opponentPlayerID] = datadisks[opponentPlayerID].GetComponent<RouletteMaker>();
        rouletteMaker[opponentPlayerID].CreateRulette(boardController.GetOpponentFigure().GetComponent<FigureParameter>().GetData());

        /*
        getInfo = GameObject.Find("GameMaster").GetComponent<GetInfo>();
        //Debug.Log(getInfo.GetData(0));
        //ルーレット作成メソッドの呼び出し
        ruletteMaker0.CreateRulette(getInfo.GetData(0));
        diskSpin0 = datadisks[0].GetComponent<DiskSpin>();
        diskSpin1 = datadisks[1].GetComponent<DiskSpin>();
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
