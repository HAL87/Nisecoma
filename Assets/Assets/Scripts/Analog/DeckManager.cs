using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    //デッキの大きさ [6,4]なら6匹,各4進化(FC)分セットできる
    //Deckクラスでまとめるなど要検討
    Figure[,] figure = new Figure[2,6];


    // Start is called before the first frame update
    void Start()
    {
        //暫定的なデータを設定
        /*
        List<Attack> pieces = new List<Attack>();
        pieces.Add(new Attack("サイコブースト", "WHITE", 150,0, 52));
        pieces.Add(new Attack("かわす", "BLUE", 0, 0, 8));
        pieces.Add(new Attack("じげんスリップ", "PURPLE", 0, 2, 32));
        pieces.Add(new Attack("ミス", "RED", 0, 0, 4));
        Figure testFigure = new Figure("デオキシスA", pieces);

        List<Attack> pieces2 = new List<Attack>();
        pieces2.Add(new Attack("はがねのつばさ", "WHITE", 50, , 15));
        pieces2.Add(new Attack("サンダークラッシュ", "GOLD", "1100x", 41));
        pieces2.Add(new Attack("ミス", "RED", "", 4));
        pieces2.Add(new Attack("はねやすめ", "BLUE", "", 16));
        pieces2.Add(new Attack("サンダーチャージ", "PURPLE", "★★★★★", 20));
        Figure testFigure2 = new Figure("サンダー", pieces2);
        

        List<Attack> piecesS = new List<Attack>();
        piecesS.Add(new Attack("ミス", "RED", 0, 0, 4));
        piecesS.Add(new Attack("しんそく", "GOLD", 40, 0, 28));
        piecesS.Add(new Attack("かわす", "BLUE", 0, 0, 12));
        piecesS.Add(new Attack("サイコキネシス", "WHITE", 60, 0, 52));
        Figure testFigureS = new Figure("デオキシスS", piecesS);

        List<Attack> piecesD = new List<Attack>();
        piecesD.Add(new Attack("かわす", "BLUE", 0, 0, 8));
        piecesD.Add(new Attack("サイコバリアー", "PURPLE", 0, 2, 60));
        piecesD.Add(new Attack("ミス", "RED", 0, 0, 4));
        piecesD.Add(new Attack("カウンター", "WHITE", 0, 0, 24));
        Figure testFigureD = new Figure("デオキシスD", piecesD);

        List<Attack> piecesLAN = new List<Attack>();
        piecesLAN.Add(new Attack("ミス", "RED", 0, 0, 8));
        piecesLAN.Add(new Attack("ブレインリンク", "WHITE", 70, 0, 36));
        piecesLAN.Add(new Attack("はかいこうせん", "WHITE", 140, 0, 20));
        piecesLAN.Add(new Attack("ミス", "RED", 0, 0, 8));
        piecesLAN.Add(new Attack("あやしいひかり", "PURPLE", 0, 3, 24));
        Figure testFigureLAN = new Figure("ランクルス", piecesLAN);
        
        List<Attack> piecesMew = new List<Attack>();
        piecesMew.Add(new Attack("サイコキネシス", "WHITE", "70", 24));
        piecesMew.Add(new Attack("ハイパーソニック", "GOLD", "50", 52));
        piecesMew.Add(new Attack("シャトルフリップ", "BLUE", "", 20));
        Figure testFigureMew = new Figure("ミュウ", piecesMew);
        
        List<Attack> piecesLuca = new List<Attack>();
        piecesLuca.Add(new Attack("かわす", "BLUE", "", 24));
        piecesLuca.Add(new Attack("はどうだん", "PURPLE", "★", 32));
        piecesLuca.Add(new Attack("ミス", "RED", "", 4));
        piecesLuca.Add(new Attack("メタルクロー", "WHITE", "70", 36));
        Figure testFigureLuca = new Figure("ルカリオ", piecesLuca);

        List<Attack> piecesBuri = new List<Attack>();
        piecesBuri.Add(new Attack("ミス", "RED", "", 4));
        piecesBuri.Add(new Attack("ニードルガード", "PURPLE", "★★", 44));
        piecesBuri.Add(new Attack("ミス", "RED", "", 8));
        piecesBuri.Add(new Attack("アームハンマー", "WHITE", "90", 40));
        Figure testFigureBuri = new Figure("ブリガロン", piecesBuri);

        List<Attack> piecesSaru = new List<Attack>();
        piecesSaru.Add(new Attack("かわす", "BLUE", "", 16));
        piecesSaru.Add(new Attack("ドライブキック", "WHITE", "80", 40));
        piecesSaru.Add(new Attack("ミス", "RED", "", 12));
        piecesSaru.Add(new Attack("マッハパンチ", "GOLD", "40", 28));
        Figure testFigureSaru = new Figure("ゴウカザル", piecesSaru);

        List<Attack> piecesKai = new List<Attack>();
        piecesKai.Add(new Attack("ミス", "RED", "", 4));
        piecesKai.Add(new Attack("からてチョップ", "WHITE", "60", 24));
        piecesKai.Add(new Attack("じごくぐるま", "WHITE", "80", 32));
        piecesKai.Add(new Attack("からてチョップ", "WHITE", "60", 24));
        piecesKai.Add(new Attack("かわす", "BLUE", "", 12));
        Figure testFigureKai = new Figure("カイリキー", piecesKai);

        List<Attack> piecesLugia = new List<Attack>();
        piecesLugia.Add(new Attack("ミス", "RED", "", 4));
        piecesLugia.Add(new Attack("エアロブラスト", "WHITE", "90", 24));
        piecesLugia.Add(new Attack("ツイスター", "PURPLE", "★★★", 28));
        piecesLugia.Add(new Attack("エアロブラスト", "WHITE", "90", 24));
        piecesLugia.Add(new Attack("ミス", "RED", "", 8));
        piecesLugia.Add(new Attack("かわす", "BLUE", "", 8));
        Figure testFigureLugia = new Figure("ルカリオ", piecesLugia);
        

        // this.SetFigure(testFigure, 0, 0);
        // this.SetFigure(testFigure2, 11, 0);

        this.SetFigure(testFigureS, 1, 0);
        this.SetFigure(testFigureD, 2, 0);
        this.SetFigure(testFigureLAN, 3, 0);
        
        this.SetFigure(testFigureMew, 4, 0);
        this.SetFigure(testFigureLuca, 5, 0);
        this.SetFigure(testFigureBuri, 6, 0);
        this.SetFigure(testFigureSaru, 7, 0);
        this.SetFigure(testFigureKai, 8, 0);
        this.SetFigure(testFigureLugia, 9, 0);
        */


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //デッキのfigureNo番目のevolveNo進化目をセットする
    public void SetFigure(Figure figure, int playerId, int figureNo)
    {
        this.figure[playerId, figureNo] = figure;
    }

    //デッキのfigureNo番目のフィギュアのevolveNo進化目を返す
    public Figure GetFigure(int playerId, int figureNo)
    {
        if(figure[playerId,figureNo] != null)
        {
            return figure[playerId, figureNo];
        }
        else
        {
            return null;
        }
    }
}
