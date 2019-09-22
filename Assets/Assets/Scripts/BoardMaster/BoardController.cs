using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BoardController : MonoBehaviour
{
    //あるノードから別のノードへのエッジをListで表現
    //edgesは配列であり、各成分がint型のList
    public const int NUMBER_OF_PLAYERS = 2;
    private List<int>[] edges = new List<int>[44];
    private int[][] entryNodeID = new int[NUMBER_OF_PLAYERS][];
    private int[] goalNodeID = new int[NUMBER_OF_PLAYERS];
    private int[][] benchNodeID = new int[NUMBER_OF_PLAYERS][];
    private int[][] pcNodeID = new int[NUMBER_OF_PLAYERS][];

    //第一要素がプレイヤー番号(0 or 1)、第二要素がfigureIDOnBoard
    //ゲーム開始時に敵味方それぞれのフィギュアを認識してfigureIDOnBoardを振る
    private List<GameObject>[] figures = new List<GameObject>[NUMBER_OF_PLAYERS];

    //ノードの親要素
    [SerializeField] private GameObject nodes;

    //エッジ描画のためのprefab
    [SerializeField] private GameObject drawEdgePrefab;

    //UI
    [SerializeField] private GameObject startText;
    [SerializeField] private List<GameObject> playerTurnText;
    [SerializeField] private GameObject restTurnText;
    [SerializeField] private GameObject gameEndText;
    [SerializeField] private GameObject turnEndButton;

    //そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[44];

    //移動、バトルの主体となるフィギュア（"使用"するフィギュア）
    private GameObject currentFigure = null;
    //歩行範囲と攻撃範囲
    private List<int> walkCandidates = new List<int>();
    private List<int> attackCandidates = new List<int>();
    //バトル相手のフィギュア
    private GameObject opponentFigure = null;
    //効果の対象など
    private GameObject targetFigure = null;

    //どちらのターンかを表す。{0,1}で定められる
    private int turnNumber = 0;
    //残りターン数
    private int restTurn = 300;
    public enum PhaseState
    {
        TurnStart,
        Normal,
        FigureSelected,
        Walking,
        AfterWalk,
        ConfirmFigure,
        Battle,
        AfterBattle,
        TurnEnd,
        GameEnd, 
        Forfeit
    };
    //ゲームの状態変数
    private PhaseState phaseState;
    // Start is called before the first frame update
    void Start()
    {

        //ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();
        EdgeDraw();
        DivideFigures();
        /* UIカット
        yield return FadeInOut(startText, 1);
        Destroy(startText);
        */
        //暫定的にプレイヤー0のターンに固定
        turnNumber = 0;
        StartCoroutine(SetPhaseState(PhaseState.TurnStart));


        //Debug.Log("プレイヤー" + turnNumber + "のターンです");
        //Debug.Log(figures.Length);

        // イベントにイベントハンドラーを追加
        SceneManager.sceneLoaded += SceneLoaded;
    }
    void Update()
    {

    }

    //ボード、フィギュアの初期化処理
    //
    //ボードのデータ構造を表現
    void CreateBoard()
    {
        //暫定のデータ構造
        //ベンチ、PC、US、除外を連番にするか別にするかは検討
        for (int i = 0; i < 44; i++)
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

        edges[28].Add(21); edges[28].Add(27);
        edges[29].Add(21); edges[29].Add(27);
        edges[30].Add(21); edges[30].Add(27);
        edges[31].Add(21); edges[31].Add(27);
        edges[32].Add(21); edges[32].Add(27);
        edges[33].Add(21); edges[33].Add(27);

        edges[34].Add(0); edges[34].Add(6);
        edges[35].Add(0); edges[35].Add(6);
        edges[36].Add(0); edges[36].Add(6);
        edges[37].Add(0); edges[37].Add(6);
        edges[38].Add(0); edges[38].Add(6);
        edges[39].Add(0); edges[39].Add(6);

        //第一成分のインデックスはプレイヤーID、第二成分のインデックス0番が左エントリー, インデックス1番が右エントリー
        for (int i = 0; i < entryNodeID.Length; i++) entryNodeID[i] = new int[NUMBER_OF_PLAYERS];
        entryNodeID[0][0] = 21; entryNodeID[0][1] = 27;
        entryNodeID[1][0] = 6; entryNodeID[1][1] = 0;

        //第一成分のインデックスはプレイヤーID
        goalNodeID[0] = 24; goalNodeID[1] = 3;

        //第一成分のインデックスはプレイヤーID, 各要素figureParameterのbenchIDに対応
        for (int i = 0; i < benchNodeID.Length; i++) benchNodeID[i] = new int[6];
        for (int i = 0; i < 6; i++) benchNodeID[0][i] = 28 + i;
        for (int i = 0; i < 6; i++) benchNodeID[1][i] = 28 + 6 + i;

        //第一成分のインデックスはプレイヤーID , 第二成分のインデックス0番は先に入るPC, インデックス1番は後に入るPC
        for (int i = 0; i < pcNodeID.Length; i++) pcNodeID[i] = new int[NUMBER_OF_PLAYERS];
        pcNodeID[0][0] = 40; pcNodeID[0][1] = 41;
        pcNodeID[1][0] = 42; pcNodeID[1][1] = 43;
    }
    //エッジの描画
    void EdgeDraw()
    {
        for (int i = 0; i < 28; i++)
        {
            for (int j = 0; j < edges[i].Count; j++)
            {
                GameObject obj = Instantiate(drawEdgePrefab, transform.position, Quaternion.identity);
                obj.transform.SetParent(transform);
                LineRenderer line = drawEdgePrefab.GetComponent<LineRenderer>();
                line.sortingOrder = 1;
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;

                //頂点の数を決める
                line.positionCount = 2;

                Vector3 startDrawPosition = nodes.transform.GetChild(i).position;

                Vector3 endDrawPosition = nodes.transform.GetChild(edges[i][j]).position;
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
            FigureParameter figureParameter = obj.GetComponent<FigureParameter>();
            if (figureParameter.GetPlayerID() == 0)
            {
                figureParameter.SetFigureIDOnBoard(figures[0].Count);
                figures[0].Add(obj);
                obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.blue;
                //Debug.Log(figures[0][figureParameter.GetFigureIDOnBoard()]);
            }
            else if (figureParameter.GetPlayerID() == 1)
            {
                figureParameter.SetFigureIDOnBoard(figures[1].Count);
                figures[1].Add(obj);
                obj.transform.Rotate(0, 0, 180f);
                obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
            }
            for (int i = 0; i < figures[0].Count; i++)
            {
                figures[0][i].GetComponent<FigureParameter>().SetBenchID(28 + i);
                figures[0][i].GetComponent<FigureParameter>().SetPosition(28 + i);
            }
            for (int i = 0; i < figures[1].Count; i++)
            {
                figures[1][i].GetComponent<FigureParameter>().SetBenchID(28 + 6 + i);
                figures[1][i].GetComponent<FigureParameter>().SetPosition(28 + 6 + i);
            }
        }
    }
    //
    //


    //
    //
    //マスク配列の作成。
    //とりあえず場にいるすべてのフィギュアを障害物と認識するだけだが、すり抜けとかの実装の際要拡張
    bool[] CreateNormalMask()
    {
        //該当するノードのmaskNodeがtrueの時すり抜け可能、falseなら障害物扱い
        bool[] maskNode = new bool[44];
        for (int i = 0; i < maskNode.Length; i++) maskNode[i] = true;
        for (int i = 0; i < figures.Length; i++)
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
    (int[] distances, int[] prevNode) CalculateDistance(int _startNode, bool[] _maskNode)
    {
        int[] distances = new int[44];
        int[] prevNode = new int[44];
        //幅優先探索を素直に実装
        Queue<int> q = new Queue<int>();

        //各ノードの到着フラグ
        bool[] reachedFlag = new bool[44];

        //distancesと到着フラグの初期化
        for (int i = 0; i < 44; i++)
        {
            distances[i] = 99;
            reachedFlag[i] = false;
            prevNode[i] = -1;
        }

        //初期ノードについての処理
        q.Enqueue(_startNode);
        reachedFlag[_startNode] = true;
        distances[_startNode] = 0;

        while (true)
        {
            //キューの先頭のノードを取り出す
            int currentNode = q.Dequeue();
            for (int i = 0; i < edges[currentNode].Count; i++)
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
        return (distances, prevNode);
    }

    //始点からの距離がmoveNumberと等しいノードを出力
    List<int> FindCandidateofDestinaitonEqual(int _moveNumber, int[] _distances)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < 28; i++)
        {
            //始点から各ノードへの距離がmoveNumberと等しいであるノードを全て格納
            if (_distances[i] == _moveNumber) candidates.Add(i);
        }

        //for(int i = 0; i < candidates.Count; i++){Debug.Log(candidates[i]);
        return candidates;
    }

    //始点からの距離がmoveNumber以下のノードを出力
    List<int> FindCandidateofDestinaitonLessThan(int _moveNumber, int[] _distances)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < 28; i++)
        {
            //始点から各ノードへの距離がmoveNumber以下であるノードを全て格納
            if (_distances[i] <= _moveNumber) candidates.Add(i);
        }
        return candidates;
    }

    //始点から終点までのルートを出力（最短距離を表現するような全ノードを列挙）
    Stack<int> DecideRoute(int _destNode, int[] _prevNode)
    {
        //始点と終点を与えたときそれらのノード間の最短経路を格納
        Stack<int> route = new Stack<int>();

        int currentNode = _destNode;
        while (true)
        {
            //終点から始点に向かってノードを積み上げていく
            route.Push(currentNode);
            currentNode = _prevNode[currentNode];
            if (currentNode == -1) break;
        }
        return route;
    }

    //フィギュアとノードをクリックされたときの処理をまとめる。
    //つまりraycastを用いてクリックしたオブジェクトのタグを取得し、それがフィギュアなら、ノードなら、とする
    //フィギュアの時とノードの時で分けてその中でState別に考えるのが丸いか。
    //どうせノードクリックしたときの処理はFigureSelect→Walkingだけだし

    //フィギュアがクリックされたときの処理
    public void FigureClicked(int _playerID, int _figureIDOnBoard)
    {
        Debug.Log(figures[_playerID][_figureIDOnBoard]);
        //ノーマル状態の時
        if (phaseState == PhaseState.Normal)
        {
            //遷移1番: Normal → FigureSelect
            //遷移条件: 任意のフィギュアをクリック

            //currentFigure更新
            currentFigure = figures[_playerID][_figureIDOnBoard];

            //状態変数更新
            //moveCandidates, walkCandidates, prevNodeの更新
            StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
        }

        else if (phaseState == PhaseState.FigureSelected)
        {
            //遷移2番: FigureSelect → Normal
            //遷移条件: クリックしたフィギュアがcurrentFigureと同じ
            if (figures[_playerID][_figureIDOnBoard] == currentFigure)
            {
                StartCoroutine(SetPhaseState(PhaseState.Normal));
            }
            //遷移3番: FigureSelect→FigureSelect
            else if (figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPlayerID() == turnNumber)
            {
                //ノードの色を初期化
                for (int i = 0; i < 28; i++)
                {
                    nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                }
                //光沢を初期化
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);
                //currentFigure更新
                currentFigure = figures[_playerID][_figureIDOnBoard];
                //状態変数更新
                StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
            }
            //相手のコマをタッチしたとき
            else if (figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPlayerID() != turnNumber)
            {
                bool attackOK = false;
                foreach (int i in attackCandidates)
                {
                    if (i == figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPosition())
                    {
                        attackOK = true;
                    }
                }
                //敵が攻撃範囲内にいなかったとき、遷移3番: FigureSelect→FigureSelect
                if (attackOK == false)
                {
                    //ノードの色を初期化
                    for (int i = 0; i < 28; i++)
                    {
                        nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    //光沢を初期化
                    currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                    currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);

                    //currentFigure更新
                    currentFigure = figures[_playerID][_figureIDOnBoard];
                    //状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                }
                //敵が攻撃範囲にいたとき、遷移5番: FigureSelect→Battle
                else
                {
                    opponentFigure = figures[_playerID][_figureIDOnBoard];
                    StartCoroutine(SetPhaseState(PhaseState.Battle));
                }
            }
        }
        else if (phaseState == PhaseState.Walking)
        {

        }
        else if (phaseState == PhaseState.AfterWalk)
        {
            //遷移10番: AfterWalk→AfterWalk
            if (figures[_playerID][_figureIDOnBoard] == currentFigure)
            {
                //情報表示だけ
            }
            //遷移7番: AfterWalk→ConfirmFigure
            else if (figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPlayerID() == turnNumber)
            {
                StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
            }

            else if (figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPlayerID() != turnNumber)
            {
                bool attackOK = false;
                foreach (int i in attackCandidates)
                {
                    if (i == figures[_playerID][_figureIDOnBoard].GetComponent<FigureParameter>().GetPosition())
                    {
                        attackOK = true;
                    }
                }
                //敵が攻撃範囲にいなかったとき、遷移7番: AfterWalk→ConfirmFigure
                if (attackOK == false)
                {
                    //状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
                }
                //敵が攻撃範囲にいたとき、遷移11番: FigureSelect→Batle
                else
                {
                    opponentFigure = figures[_playerID][_figureIDOnBoard];
                    StartCoroutine(SetPhaseState(PhaseState.Battle));
                }
            }

        }

        else if (phaseState == PhaseState.ConfirmFigure)
        {
            //遷移8番: ConfirmFigure →ConfirmFigure
            if (figures[_playerID][_figureIDOnBoard] != currentFigure)
            {
                //情報表示だけ
            }
            //遷移9番: ConfirmFigure→AfterWalk
            else if (figures[_playerID][_figureIDOnBoard] == currentFigure)
            {
                StartCoroutine(SetPhaseState(PhaseState.AfterWalk));
            }
        }
        else return;
    }

    //遷移4番 FigureSelect → Walking
    //FigureSelect状態でcandidates内のノードがクリックされたら、選択中のフィギュアに経路情報を渡す
    public IEnumerator NodeClicked(int _nodeID)
    {

        // 状態チェック
        if (phaseState != PhaseState.FigureSelected)
        {
            yield break;
        }

        // 選択フィギュアの所有権チェック
        if (currentFigure.GetComponent<FigureParameter>().GetPlayerID() != turnNumber)
        {
            yield break;
        }

        // ウェイトチェック
        if(currentFigure.GetComponent<FigureParameter>().GetWaitCount() >= 1)
        {
            yield break;
        }

        // MP移動先候補とクリックされたノードIDが一致したらWalking状態に遷移して移動を行い、
        // その後AfterWalk状態に遷移する
        for (int i = 0; i < walkCandidates.Count; i++)
        {
            if (_nodeID == walkCandidates[i])
            {
                StartCoroutine(SetPhaseState(PhaseState.Walking));
                //ここの引数のprevNodeはFigureSelectedが呼ばれたときに格納されているよ

                Stack<int> route = DecideRoute(_nodeID, prevNode);
                //コルーチンを使った移動に変えた
                if (currentFigure.GetComponent<FigureParameter>().GetPosition() >= 28)
                {
                    route.Pop();
                    yield return StartCoroutine(currentFigure.GetComponent<FigureController>().FigureOneStepWalk(route.Peek()));
                }
                if (route.Count >= 2)
                {
                    yield return StartCoroutine(currentFigure.GetComponent<FigureController>().Figurewalk(route));
                }
                StartCoroutine(SetPhaseState(PhaseState.AfterWalk));
            }
        }

        yield break;
    }

    //ゲームの状態変数のゲッター、セッター
    public IEnumerator SetPhaseState(PhaseState _tempState)
    {
        phaseState = _tempState;
        Debug.Log(GetPhaseState());

        //ターンの開始時
        if(phaseState == PhaseState.TurnStart)
        {
            /*UIカット
            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
            yield return FadeInOut(playerTurnText[turnNumber], 0.5f);
            */
            StartCoroutine(SetPhaseState(PhaseState.Normal));
        }
        else if (phaseState == PhaseState.Normal)
        {
            //各種変数の初期化
            //光沢を消す
            if (currentFigure != null)
            {
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);
            }
            currentFigure = null;
            for (int i = 0; i < prevNode.Length; i++) prevNode[i] = -1;
            walkCandidates.Clear();
            attackCandidates.Clear();
            //ノードの色を初期化
            for (int i = 0; i < 28; i++)
            {
                nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        else if (phaseState == PhaseState.FigureSelected)
        {
            //選択したフィギュアを目立たせる
            currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
            currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.0f);


            FigureParameter currentFigureParameter = currentFigure.GetComponent<FigureParameter>();
            //他のフィギュアを障害物として認識
            bool[] maskWalkNode = CreateNormalMask();
            //現在地から全ノードへの距離を計算
            var calculateWalkDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskWalkNode);
            //この計算の時にDecideRouteに必要なprevNodeを記録
            prevNode = calculateWalkDistance.prevNode;
            //現在地からmp以内で移動可能な全ノードの色を紫色にする
            if (restTurn == 300)
            {
                walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetMp() - 1, calculateWalkDistance.distances);
            }
            else walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetMp(), calculateWalkDistance.distances);
            for (int i = 0; i < walkCandidates.Count; i++)
            {
                nodes.transform.GetChild(walkCandidates[i]).GetComponent<SpriteRenderer>().color = Color.magenta;
            }

            //攻撃範囲の計算
            bool[] maskAttackNode = new bool[44];
            for (int i = 0; i < maskAttackNode.Length; i++) maskAttackNode[i] = true;
            var calculateAttackDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetAttackRange(), calculateAttackDistance.distances);
        }
        else if (phaseState == PhaseState.AfterWalk)
        {
            //ノードの色を初期化
            for (int i = 0; i < 28; i++)
            {
                nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
            }

            // 包囲処理
            int currentFigureNode = currentFigure.GetComponent<FigureParameter>().GetPosition();
            foreach (int i in edges[currentFigureNode])
            {
                GameObject adjacentFigure = GetFigureOnBoard(i);
                yield return KnockedOutBySurrounding(adjacentFigure);
            }

            //周囲に敵が誰もいなければターンエンド
            //実際はこれに加えて眠り、氷、mpマイナスマーカーの味方がいたとき

            FigureParameter currentFigureParameter = currentFigure.GetComponent<FigureParameter>();
            bool opponentExistInAttackCandidates = false;   // バトル候補がいるかどうか
            int opponentID;
            if (currentFigureParameter.GetPlayerID() == 0) opponentID = 1;
            else opponentID = 0;

            bool[] maskAttackNode = new bool[28];
            for (int i = 0; i < maskAttackNode.Length; i++) maskAttackNode[i] = true;
            var calculateAttackDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetAttackRange(), calculateAttackDistance.distances);
            //Debug.Log(opponentID);

            for (int i = 0; i < figures[opponentID].Count; i++)
            {
                foreach (int j in attackCandidates)
                {
                    if (j == figures[opponentID][i].GetComponent<FigureParameter>().GetPosition())
                    {
                        opponentExistInAttackCandidates = true;
                    }
                    
                }
            }
            //今ゴール処理は通常移動後しか考えてない。ワザの効果で移動するときもな
            if (currentFigure.GetComponent<FigureParameter>().GetPosition() == goalNodeID[opponentID])
            {
                StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }
            else if (opponentExistInAttackCandidates == false) StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
            
            else if (opponentExistInAttackCandidates == true)
            {
                turnEndButton.SetActive(true);
            }

        }
        else if (phaseState == PhaseState.Battle)
        {
            SceneManager.LoadScene("SpinScene");
        }
        else if (phaseState == PhaseState.AfterBattle)
        {
            //バトル後の処理をここに

            //バトル結果の受け取り
            var BattleResult = SpinController.GetBattleResult();
            int result = BattleResult.Item1;
            bool currentMoveAwake = BattleResult.Item2;
            bool opponentMoveAwake = BattleResult.Item3;
            bool currentDeath = BattleResult.Item4;
            bool opponentDeath = BattleResult.Item5;
            Debug.Log("boardで" + BattleResult);
            //if(currentMoveAwake){}
            //if(opponentMoveAwake){}
            if (currentDeath)
            {
                yield return Death(currentFigure);
            }
            if (opponentDeath)
            {
                yield return Death(opponentFigure);
            }
            StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
        }
        else if (phaseState == PhaseState.TurnEnd)
        {
            //相手のターンにして残りのターン数を更新
            if (turnNumber == 0)
            {
                turnNumber = 1;
            }
            else if (turnNumber == 1)
            {
                turnNumber = 0;
            }
            restTurn--;

            // ウェイトが付いているフィギュアのウェイトを更新
            // ウェイト0になったらウェイトカウンターの描画を終了する
            for(int i = 0; i < 2; i++)
            {
                List<GameObject> deck = figures[i];
                foreach(GameObject figure in deck)
                {
                    if(figure.GetComponent<FigureParameter>().GetWaitCount() >= 1)
                    {
                        figure.GetComponent<FigureParameter>().decreaseWaitCount();
                    }
                }
            }

            turnEndButton.SetActive(false);
            //Debug.Log("プレイヤー" + turnNumber + "のターンです");
            StartCoroutine(SetPhaseState(PhaseState.TurnStart));
        }
        else if(phaseState == PhaseState.GameEnd)
        {
            //もう少し真面目にかけ
            //実際はどっかのタイミングで全部のフィギュアの位置調べて相手のゴールにいればなんちゃらみたいな感じかなあ知らんけど
                gameEndText.SetActive(true);
                gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + currentFigure.GetComponent<FigureParameter>().GetPlayerID() + " WIN!";
        }
        else if(phaseState == PhaseState.Forfeit)
        {
//            Debug.Log("CurrentPlayerId = " + currentFigure.GetComponent<FigureParameter>().GetPlayerID());
//            Debug.Log("OpponentPlayerId = " + opponentFigure.GetComponent<FigureParameter>().GetPlayerID());
            gameEndText.SetActive(true);
            gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + (turnNumber + 1) % 2 + " WIN!";
        }
        
        else yield return null;
        //yield return null;
    }

    public PhaseState GetPhaseState()
    {
        return phaseState;
    }

    public GameObject GetCurrentFigure()
    {
        return currentFigure;
    }
    public GameObject GetOpponentFigure()
    {
        return opponentFigure;
    }

    public IEnumerator Death(GameObject _figure)
    {
        GameObject backBenchFigure = null;
        GameObject moveAnotherPCFigure = null;
        int playerID = _figure.GetComponent<FigureParameter>().GetPlayerID();

        for (int i = 0; i < figures[playerID].Count; i++)
        {
            if (figures[playerID][i].GetComponent<FigureParameter>().GetPosition() == pcNodeID[playerID][1])
            {

                backBenchFigure = figures[playerID][i];
            }
            else if (figures[playerID][i].GetComponent<FigureParameter>().GetPosition() == pcNodeID[playerID][0])
            {
                moveAnotherPCFigure = figures[playerID][i];
            }
        }

        //PC1からベンチへ移動
        if (backBenchFigure != null)
        {
            // ウェイト2を付与
            backBenchFigure.GetComponent<FigureParameter>().SetWaitCount(2);
            yield return backBenchFigure.GetComponent<FigureController>().FigureOneStepWalk(backBenchFigure.GetComponent<FigureParameter>().GetBenchID());
        }
        //PC0からPC1へ移動
        if (moveAnotherPCFigure != null)
        {
            yield return moveAnotherPCFigure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeID[playerID][1]);
        }
        //フィールドからPC0へ移動
        yield return _figure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeID[playerID][0]);
    }

    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        if (nextScene.name == "SpinScene")
        {
            //ノードの色を初期化
            for (int i = 0; i < 28; i++)
            {
                nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
            }
            //ノードを非表示
            for (int i = 0; i < 44; i++)
            {
                nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
            }
            //エッジを非表示
            for (int i = 0; i < this.transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
            }

            //フィギュアを非表示にする
            for (int i = 0; i < 2; i++)
            {
                foreach (GameObject obj in figures[i])
                {
                    for (int j = 0; j < obj.transform.childCount; j++)
                    {
                        obj.transform.GetChild(j).GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            //UIの非表示
            turnEndButton.SetActive(false);

            //UIカット
            //restTurnText.GetComponent<TextMeshProUGUI>().enabled = false;

        }
        //SpinSceneからBoardSceneに帰ってきた
        else if (nextScene.name == "BoardScene")
        {
            Destroy(GameObject.Find("BeforeStart"));
            //ノードを再表示
            for (int i = 0; i < 44; i++)
            {
                nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
            }
            //エッジを再表示
            for (int i = 0; i < this.transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
            }
            //フィギュアを再表示
            for (int i = 0; i < 2; i++)
            {
                foreach (GameObject obj in figures[i])
                {
                    for (int j = 0; j < obj.transform.childCount; j++)
                    {
                        obj.transform.GetChild(j).GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
            }
            //UIの表示
            turnEndButton.SetActive(true);
            //UIカット
            //restTurnText.GetComponent<TextMeshProUGUI>().enabled = true;

            
            //バトル後の処理
            StartCoroutine(SetPhaseState(PhaseState.AfterBattle));
        }
    }

    IEnumerator FadeInOut(GameObject _text, float _fadetime)
    {
        float a = 0f;
        while (a < _fadetime)
        {
            _text.GetComponent<TextMeshProUGUI>().color += new Color(0, 0, 0, Time.deltaTime);
            a += Time.deltaTime;
            yield return null;
        }
        while (a > 0)
        {
            _text.GetComponent<TextMeshProUGUI>().color -= new Color(0, 0, 0, Time.deltaTime);
            a -= Time.deltaTime;
            yield return null;
        }

    }

    public void turnEnd()
    {
        StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
    }

    public void forFeit()
    {
        StartCoroutine(SetPhaseState(PhaseState.Forfeit));
    }

    // figureが相手のフィギュアに包囲されているかを判定する
    public bool IsSurrounded(GameObject figure)
    {
        // figureの所有者のID
        int currentId = figure.GetComponent<FigureParameter>().GetPlayerID();
        // figureに隣接するフィギュア
        GameObject adjacentFigure;

        // figureに隣接するフィギュアが全て相手のフィギュアでなければfalseを返す
        // それ以外であればtrueを返す
        foreach(int adjacentNode in edges[figure.GetComponent<FigureParameter>().GetPosition()])
        {
            adjacentFigure = GetFigureOnBoard(adjacentNode);
            if (null == adjacentFigure)
            {
                return false;
            }

            if (currentId == adjacentFigure.GetComponent<FigureParameter>().GetPlayerID())
            {
                return false;
            }
        }

        return true;
    }

    // figureの包囲判定と気絶処理を行う
    public IEnumerator KnockedOutBySurrounding(GameObject figure)
    {
        Debug.Log("KnockedOutBySurrounding is called.");
        if(null == figure)
        {
            yield break;
        }
        bool isSurrounded = IsSurrounded(figure);

        if (isSurrounded)
        {
            Debug.Log("Knocked out by surrounding!!");
            yield return Death(figure);
        }
    }

    // ボード上のノード(nodeId)にあるフィギュアオブジェクトを取得する
    // フィギュアが存在しない場合はnullを返す
    public GameObject GetFigureOnBoard(int nodeId)
    {
        for(int playerId = 0; playerId < NUMBER_OF_PLAYERS; playerId++)
        {
            foreach (GameObject figure in figures[playerId])
            {
                if(nodeId == figure.GetComponent<FigureParameter>().GetPosition())
                {
                    return figure;
                }
            }
        }

        return null;
    }

    public int GetTheOtherPlayerId(int onePlayerId)
    {
        return (onePlayerId + 1) % 2;
    }
}
