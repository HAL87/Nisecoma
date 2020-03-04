using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    //デッキの大きさ [6,4]なら6匹,各4進化(FC)分セットできる
    //Deckクラスでまとめるなど要検討
    Figure[] figure = new Figure[6];
    private BoardController boardController;

    // Start is called before the first frame update
    void Start()
    {
        boardController = GameObject.Find("BoardMaster").GetComponent<BoardController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitializeDeck(int playerId)
    {
        List<GameObject>[] figures = boardController.Figures;
        for(int i = 0; i < figures[playerId].Count; i++)
        {
            List<Attack> pieces = new List<Attack>();
            GameObject data = figures[playerId][i].GetComponent<FigureParameter>().Data;
            for (int j = 0; j < data.transform.childCount; j++)
            {
                MoveParameter mP = data.transform.GetChild(j).GetComponent<MoveParameter>();
                pieces.Add(new Attack(mP.GetMoveName(), mP.GetMoveColorName(), mP.GetMovePower(), mP.GetMoveNumberOfStar(), mP.GetMoveRange()));
            }
            Figure tempfigure = new Figure(figures[playerId][i].name, pieces);
            SetFigure(tempfigure,i);
        }


    }
    //デッキのfigureNo番目のevolveNo進化目をセットする
    public void SetFigure(Figure figure, int figureNo)
    {
        this.figure[figureNo] = figure;
    }

    //デッキのfigureNo番目のフィギュアのevolveNo進化目を返す
    public Figure GetFigure(int figureNo)
    {
        if(figure[figureNo] != null)
        {
            return figure[figureNo];
        }
        else
        {
            return null;
        }
    }
}
