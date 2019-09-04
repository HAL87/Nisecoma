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

    //あらかじめ場にあるフィギュアを保持しておいて、figureIDOnBoardをインデックスとして呼び出すイメージ？
    [SerializeField] private List<GameObject> figures0;

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
    // Start is called before the first frame update
    void Start()
    {
        //ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();
        EdgeDraw();
    }
    /*
    void Update()
    {
        if (phaseState == PhaseState.Normal)
        {

        }
    }*/
    //ボードのデータ構造を表現
    void CreateBoard()
    {
        //暫定のデータ構造
        //ベンチ、PC、US、除外を連番にするか別にするかは検討
        for (int i = 0; i < 28; i++)
        {
            //nodes[i] = i;
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

    //始点から各ノードへの距離を計算
    int[] CaliculateDistance(int startNode)
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
            prevNode[i] = 99;
        }

        //初期ノードについての処理
        q.Enqueue(startNode);
        reachedFlag[startNode] = true;
        distances[startNode] = 0;
        prevNode[startNode] = -1;
        while (true)
        {
            //キューの先頭のノードを取り出す
            int currentNode = q.Dequeue();
            for(int i = 0; i < edges[currentNode].Count; i++)
            {
                //現在ノードにつながっているノード（=nextNode)を見る
                int nextNode = edges[currentNode][i];
                //nextNodeに到着していなければ
                if(reachedFlag[nextNode] == false)
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
        /*
        for(int i = 0; i < 28; i ++)
        {
            Debug.Log("ノード" + i + ":"+ distances[i]);
        }
        */
        return distances;
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
        /*
        for(int i = 0; i < candidates.Count; i++)
        {
            Debug.Log(candidates[i]);
        }
        */
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
        //上から順番に取り出せば最短経路が復元できる
        /*
        while(route.Count > 0)
        {
            Debug.Log(route.Pop());
        }
        */
        return route;
    }

    //フィギュアがクリックされたときmp以内の候補地を列挙
    public void FigureSelected(int playerID, int figureIDOnBoard)
    {
        //ノードの色を初期化
        for(int i = 0; i < nodes.transform.childCount; i++)
        {
            Image image = nodes.transform.GetChild(i).GetComponent<Image>();
            image.color = Color.white;
        }

        //選択したフィギュアが直前に選択されていたものならフラグをfalseに、そうでなければtrueに
        //選択されていないフィギュアは全てfalseに
        
        for(int i = 0; i < figures0.Count; i++)
        {
            figureParameter = figures0[i].GetComponent<FigureParameter>();
            if (i == figureIDOnBoard) figureParameter.SetBeSelected(!figureParameter.GetBeSelected());
            else figureParameter.SetBeSelected(false);
        }
        figureParameter = figures0[figureIDOnBoard].GetComponent<FigureParameter>();
        //フラグがtrueならそのフィギュアの現在地からmp以内のノードを全列挙して色を変える
        if (figureParameter.GetBeSelected() == true)
        {
            CaliculateDistance(figureParameter.GetPosition());
            FindCandidateofDestinaitonLessThan(figureParameter.GetMp());
            for (int i = 0; i < candidates.Count; i++)
            {
                Image image = nodes.transform.GetChild(candidates[i]).GetComponent<Image>();
                image.color = Color.blue;
            }
            //状態変数更新
            SetPhaseState(PhaseState.FigureSelected);
        }
        else SetPhaseState(PhaseState.Normal);
        Debug.Log(GetPhaseState());
        
        //ここで一旦Waitして、ゲームブック方式でPhase遷移
        //candidatesから候補地を選択→walkingへ
        //敵をタップ→バトルへ
        //ターンエンド→相手のターンへ

    }

    /*
    public void NodeSelected(int nodeID)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if(nodeID == candidates[i])
            {

                DecideRoute(nodeID);
            }
        }
    }
    */
    //ゲームの状態変数のゲッター、セッター
    public void SetPhaseState(PhaseState tempState)
    {
        phaseState = tempState;
    }
    public PhaseState GetPhaseState()
    {
        return phaseState;
    }

    // Update is called once per frame
}
