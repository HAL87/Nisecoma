using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    /* --------------------------------------------------------------- 
    Spinボタンの処理もこっちで管理したほうが良い感じなのでそうするべき 
    -----------------------------------------------------------------*/
    
    
    [SerializeField]
    DeckManager deckManager;
    [SerializeField]
    RouletteManager rouletteManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //フィギュア選択ボタンクリック時
    //引数1つで済ますため10*figureNo+evolveNoが引数となる
    public void OnClick(int figure_evolveNo)
    {
        int figureNo = figure_evolveNo / 10;
        int evolveNo = figure_evolveNo % 10;

        Figure figure = deckManager.GetFigure(figureNo, evolveNo);
        if(figure != null)
        {
            //Debug.Log(figure.figureName);
            rouletteManager.MakeRoulette(figure);
        }
    }

}
