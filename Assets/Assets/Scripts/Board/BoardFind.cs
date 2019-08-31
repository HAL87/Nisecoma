using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardFind : MonoBehaviour
{
    //private int[] nodes = new int[28];
    //あるノードから別のノードへのエッジをListで表現
    //edgesは配列であり、各成分がint型のList
    private List<int>[] edges = new List<int>[28];

    //そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[28];

    //始点。大体フィギュアの移動前の位置
    [SerializeField] private int startNode;

    //移動歩数(意図的にmpと区別している)。mpだったり、特性やワザでの移動歩数
    [SerializeField] private int moveNumber;

    //終点。始点と移動歩数を与えたとき移動可能な全ノードを格納するが、その候補地の中から目的地を選ぶ
    //なので、本来このようにインスペクタから入力するものではない（今はテストのため暫定でこうしている）
    [SerializeField] private int goalNode;
    // Start is called before the first frame update
    void Start()
    {
        //ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();

        //これ以下は本来void Startでよばれるものではないがテストのため暫定的に

        //始点から終点まで実際にどう動くかのルートを決定する
        //DecideRoute(startNode, goalNode);

        //始点から指定移動歩数内で移動できる全ノードを格納

        FindCandidateofDestinaitonEqual(startNode, moveNumber);

    }
    //ボードのエッジを表現
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
        edges[4].Add(3); edges[4].Add(3);
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

    //始点から各ノードへの距離を計算
    int[] CaliculateDistance(int startNode)
    {
        //あるノードから他の全てのノードへの距離
        //移動処理ごとに更新
        int[] distances = new int[28];

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
    List<int> FindCandidateofDestinaitonEqual(int startNode, int moveNumber)
    {
        //始点と移動歩数を与えたとき移動可能な全ノードを格納
        List<int> candidates = new List<int>();
        //始点から各ノードの距離を計算
        int[] distances = CaliculateDistance(startNode);

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
        return candidates;
    }

    //始点からの距離がmoveNumber以下のノードを出力
    List<int> FindCandidateofDestinaitonLessThan(int startNode, int moveNumber)
    {
        //始点と移動歩数を与えたとき移動可能な全ノードを格納
        List<int> candidates = new List<int>();
        //始点から各ノードの距離を計算
        int[] distances = CaliculateDistance(startNode);

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
        return candidates;
    }

    //始点から終点までのルートを出力（最短距離を表現するような全ノードを列挙）
    Stack<int> DecideRoute(int startNode, int goalNode)
    {
        //始点と終点を与えたときそれらのノード間の最短経路を格納
        Stack<int> route = new Stack<int>();
        //distancesとprevNodeを格納
        //DecideRouteメソッドは必ずFindCandidateの後に呼び出されるからCaliculateDistanceを呼ぶ必要ない気がする
        CaliculateDistance(startNode);
        int currentNode = goalNode;
        while (true)
        {
            //終点から始点に向かってノードを積み上げていく
            route.Push(currentNode);
            currentNode = prevNode[currentNode];
            if(currentNode == startNode)
            {
                route.Push(startNode);
                break;
            }
        }
        //上から順番に取り出せば最短経路が復元できる
        while(route.Count > 0)
        {
            Debug.Log(route.Pop());
        }
        return route;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
