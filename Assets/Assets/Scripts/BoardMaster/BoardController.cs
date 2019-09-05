using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    //あるノードから別のノードへのエッジをListで表現
    //edgesは配列であり、各成分がint型のList
    private List<int>[] edges = new List<int>[28];

    //第一要素がプレイヤー番号(0 or 1)、第二要素がfigureIDOnBoard
    //ゲーム開始時に敵味方それぞれのフィギュアを認識してfigureIDOnBoardを振る
    private List<GameObject>[] figures = new List<GameObject>[2];
    //ノードの親要素
    [SerializeField] private GameObject nodes;

    //LIneRenderer用のやつ
    [SerializeField] private GameObject drawEdgePrefab;

    //あるノードから他の全てのノードへの距離
    private int[] distances = new int[28];
    //そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[28];
    //始点と移動歩数を与えたとき移動可能な全ノードを格納
    List<int> candidates = new List<int>();

    //選択中のフィギュア
    private GameObject currentFigure;

    public enum PhaseState
    {
        Normal,
        FigureSelected,
        Walking,
        AfterWalk
    };
    //ゲームのの状態変数
    private PhaseState phaseState;

    //スクリプト変数宣言
    private FigureParameter figureParameter;
    private FigureController figureController;
    // Start is called before the first frame update
    void Start()
    {
        //ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();
        EdgeDraw();
        DivideFigures();
        SetPhaseState(PhaseState.Normal);
        //Debug.Log(figures.Length);
    }
    void Update()
    {

    }
    //ボードのデータ構造を表現
    void CreateBoard()
    {
        //暫定のデータ構造
        //ベンチ、PC、US、除外を連番にするか別にするかは検討
        for (int i = 0; i < 28; i++)
        {
            nodes.transform.GetChild(i).GetComponent<NodeParameter>().SetNodeID(i);
            edges[i] = new List<int>();
        }
        edges[0].Add(1); edges[0].Add(7); edges[0].Add(8);
        edges[1].Add(0); edges[1].Add(2);
        edges[2].Add(1); edges[2].Add(3); edges[2].Add(9);
        edges[3].Add(2); edges[3].Add(4);
        edges[4].Add(3); edges[4].Add(5);
        edges[5].Add(4); edges[5].Add(6);
        edges[6].Add(5); edges[6].Add(10); edges[6].Add(11);
        edges[7].Add(0); edges[7].Add(12);
        edges[8].Add(0); edges[8].Add(9); edges[8].Add(13);
        edges[9].Add(2); edges[9].Add(8); edges[9].Add(10);
        edges[10].Add(6); edges[10].Add(9); edges[10].Add(14);
        edges[11].Add(6); edges[11].Add(15);
        edges[12].Add(7); edges[12].Add(16);
        edges[13].Add(8); edges[13].Add(17);
        edges[14].Add(10); edges[14].Add(19);
        edges[15].Add(11); edges[15].Add(20);
        edges[16].Add(12); edges[16].Add(21);
        edges[17].Add(13); edges[17].Add(18); edges[17].Add(21);
        edges[18].Add(17); edges[18].Add(19); edges[18].Add(25);
        edges[19].Add(14); edges[19].Add(18); edges[19].Add(27);
        edges[20].Add(15); edges[20].Add(27);
        edges[21].Add(16); edges[21].Add(17); edges[21].Add(22);
        edges[22].Add(21); edges[22].Add(23);
        edges[23].Add(22); edges[23].Add(24);
        edges[24].Add(23); edges[24].Add(25);
        edges[25].Add(18); edges[25].Add(24); edges[25].Add(26);
        edges[26].Add(25); edges[26].Add(27);
        edges[27].Add(19); edges[27].Add(20); edges[27].Add(26);
    }
    //エッジの描画
    void EdgeDraw()
    {
        for(int i = 0; i < 28; i++)
        {
            for (int j = 0; j < edges[i].Count; j++)
            {
                float offset = 0.01f;
                GameObject obj = Instantiate(drawEdgePrefab, transform.position, Quaternion.identity);
                obj.transform.SetParent(transform);
                LineRenderer line = drawEdgePrefab.GetComponent<LineRenderer>();
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;

                //頂点の数を決める
                line.positionCount = 2;
                
                Vector3 startDrawPosition = new Vector3(nodes.transform.GetChild(i).position.x,
                                                        nodes.transform.GetChild(i).position.y - offset,
                                                        nodes.transform.GetChild(i).position.z);

                Vector3 endDrawPosition = new Vector3(nodes.transform.GetChild(edges[i][j]).position.x,
                                                      nodes.transform.GetChild(edges[i][j]).position.y - offset,
                                                      nodes.transform.GetChild(edges[i][j]).position.z);
                line.SetPosition(0, startDrawPosition);
                line.SetPosition(1, endDrawPosition);
                
                
            }
        }
    }

    //各フィギュアをplayerIDに従って分けてfigureという箱に入れる
    //配列の第二要素のインデックスに対応する値(figureIDOnBoard)を各フィギュアに割り振る
    void DivideFigures()
    {
        figures[0] = new List<GameObject>();
        figures[1] = new List<GameObject>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Figure");
        foreach (GameObject obj in objs)
        {
            figureParameter = obj.GetComponent<FigureParameter>();
            if (figureParameter.GetPlayerID() == 0)
            {
                figureParameter.SetFigureIDOnBoard(figures[0].Count);
                figures[0].Add(obj);
                obj.GetComponent<Renderer>().material.color = Color.blue;
                //Debug.Log(figures[0][figureParameter.GetFigureIDOnBoard()]);
            }
            else if (figureParameter.GetPlayerID() == 1)
            {
                figureParameter.SetFigureIDOnBoard(figures[1].Count);
                figures[1].Add(obj);
                obj.GetComponent<Renderer>().material.color = Color.red;
            }
        }
        /*
        foreach(GameObject i in figures[0])
        {
            Debug.Log(i.GetComponent<FigureParameter>().GetFigureIDOnBoard() + " " + i);
        }
        Debug.Log(" a");
        foreach (GameObject i in figures[1])
        {
            Debug.Log(i.GetComponent<FigureParameter>().GetFigureIDOnBoard() + " " + i);
        }
        */
    }

    //マスク配列の作成。
    //とりあえず場にいるすべてのフィギュアを障害物と認識するだけだが、すり抜けとかの実装の際要拡張
    
    bool[] CreateNormalMask()
    {
        //該当するノードのmaskNodeがtrueの時すり抜け可能、falseなら障害物扱い
        bool[] maskNode = new bool[28];
        for (int i = 0; i < maskNode.Length; i++) maskNode[i] = true;
        for(int i = 0; i < figures.Length; i++)
        {
            for (int j = 0; j < figures[i].Count; j++)
            {
                FigureParameter figureParameter = figures[i][j].GetComponent<FigureParameter>();
                maskNode[figureParameter.GetPosition()] = false;
                //Debug.Log(figureParameter.GetPosition());
            }
        }


        return maskNode;
    }
    
    //始点から各ノードへの距離を計算
    //, bool[] _maskNode
    void CaliculateDistance(int startNode, bool[] _maskNode)
    {

    　　//幅優先探索を素直に実装
    　　Queue<int> q = new Queue<int>();
        
        //各ノードの到着フラグ
        bool[] reachedFlag = new bool[28];

        //distancesと到着フラグの初期化
        for (int i = 0; i < 28; i++)
        {
            distances[i] = 99;
            reachedFlag[i] = false;
            prevNode[i] = -1;
        }

        //初期ノードについての処理
        q.Enqueue(startNode);
        reachedFlag[startNode] = true;
        distances[startNode] = 0;
        //prevNode[startNode] = -1;
        //_maskNode[startNode] = true;

        while (true)
        {
            //キューの先頭のノードを取り出す
            int currentNode = q.Dequeue();
            for(int i = 0; i < edges[currentNode].Count; i++)
            {
                //現在ノードにつながっているノード（=nextNode)を見る
                int nextNode = edges[currentNode][i];
                //nextNodeに到着していなければ
                //maskがfalseのノードは障害物アリなので行けない
                if (reachedFlag[nextNode] == false && _maskNode[nextNode] == true)
                {
                    //キューにnextNodeを入れ、到着フラグをtrueにし、distanceの計算をし、どこから来たのかを記録
                    q.Enqueue(nextNode);
                    reachedFlag[nextNode] = true;
                    distances[nextNode] = distances[currentNode] + 1;
                    prevNode[nextNode] = currentNode;
                }

            }
            //キューの中身が空になればループから抜ける
            if (q.Count == 0) break;
        }
        //Debug.Log("startNodeは" + startNode);
        //for (int i = 0; i < distances.Length; i++) Debug.Log(i + " " + distances[i]);
        return;
    }

    //始点からの距離がmoveNumberと等しいノードを出力
    void FindCandidateofDestinaitonEqual(int moveNumber)
    {
        candidates.Clear();
        for (int i = 0; i < 28; i++)
        {
            //始点から各ノードへの距離がmoveNumberと等しいであるノードを全て格納
            if (distances[i] == moveNumber)
            {
                candidates.Add(i);
            }
        }
        /*
        for(int i = 0; i < candidates.Count; i++)
        {
            Debug.Log(candidates[i]);
        }
        */
        return;
    }

    //始点からの距離がmoveNumber以下のノードを出力
    void FindCandidateofDestinaitonLessThan(int moveNumber)
    {
        candidates.Clear();
        for (int i = 0; i < 28; i++)
        {
            //始点から各ノードへの距離がmoveNumber以下であるノードを全て格納
            if (distances[i] <= moveNumber)
            {
                candidates.Add(i);
            }
        }
        
        return;
    }

    //始点から終点までのルートを出力（最短距離を表現するような全ノードを列挙）
    Stack<int> DecideRoute(int destNode)
    {
        //始点と終点を与えたときそれらのノード間の最短経路を格納
        Stack<int> route = new Stack<int>();

        int currentNode = destNode;
        while (true)
        {
            //終点から始点に向かってノードを積み上げていく
            route.Push(currentNode);
            currentNode = prevNode[currentNode];
            if(currentNode == -1)
            {
                break;
            }
        }
        return route;
    }

    //フィギュアがクリックされたときmp以内の候補地を列挙
    public void FigureSelected(int playerID, int figureIDOnBoard)
    {
        //ノードの色を初期化
        for (int i = 0; i < nodes.transform.childCount; i++)
        {
            Image image = nodes.transform.GetChild(i).GetComponent<Image>();
            image.color = Color.white;
        }
        //選択したフィギュアが直前に選択されていたものならフラグをfalseに、そうでなければtrueに反転
        //選択されていないフィギュアは全てfalseに

        for (int i = 0; i < figures[playerID].Count; i++)
        {
            figureParameter = figures[playerID][i].GetComponent<FigureParameter>();
            if (i == figureIDOnBoard) figureParameter.SetBeSelected(!figureParameter.GetBeSelected());
            else figureParameter.SetBeSelected(false);
        }
        //選択したフィギュアをcurrentFigure変数にセット
        currentFigure = figures[playerID][figureIDOnBoard];
        figureParameter = currentFigure.GetComponent<FigureParameter>();

        //フラグがtrueならそのフィギュアの現在地からmp以内のノードを全列挙して色を変える
        if (figureParameter.GetBeSelected() == true)
        {
            //他のフィギュアを障害物として認識
            bool[] maskNode = CreateNormalMask();
            //現在地から全ノードへの距離を計算
            CaliculateDistance(figureParameter.GetPosition(), maskNode);
            //現在地からmp以内で移動可能な全ノードの色を紫色にする
            FindCandidateofDestinaitonLessThan(figureParameter.GetMp());
            for (int i = 0; i < candidates.Count; i++)
            {
                Image image = nodes.transform.GetChild(candidates[i]).GetComponent<Image>();
                image.color = Color.magenta;
            }
            //状態変数更新
            SetPhaseState(PhaseState.FigureSelected);
        }
        else SetPhaseState(PhaseState.Normal);
    }

    //FigureSelect状態でcandidates内のノードがクリックされたら、選択中のフィギュアに経路情報を渡す
    public void NodeSelected(int nodeID)
    {
        if(phaseState == PhaseState.FigureSelected)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (nodeID == candidates[i])
                {
                    figureController = currentFigure.GetComponent<FigureController>();
                    figureController.SetRoute(DecideRoute(nodeID));                    
                }
            }
        }
    }
    
    //ゲームの状態変数のゲッター、セッター
    public void SetPhaseState(PhaseState tempState)
    {
        phaseState = tempState;
        Debug.Log(GetPhaseState());
        if (phaseState == PhaseState.Normal)
        {
            //ノードの色を初期化
            for (int i = 0; i < nodes.transform.childCount; i++)
            {
                Image image = nodes.transform.GetChild(i).GetComponent<Image>();
                image.color = Color.white;
            }
            //各種変数の初期化
            currentFigure = null;
            for(int i = 0; i < distances.Length; i++)
            {
                distances[i] = 99;
                prevNode[i] = -1;
            }
            candidates.Clear();
        }
    }

    public PhaseState GetPhaseState()
    {
        return phaseState;
    }

    // Update is called once per frame
}
