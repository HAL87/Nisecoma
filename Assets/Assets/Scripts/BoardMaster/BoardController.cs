/*****************************************************************/
/* Class:   BoardController.cs                                   */
/* Assets:  BoardScene\DontDestroyObjects\BoardMaster            */
/*****************************************************************/
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class BoardController : MonoBehaviourPunCallbacks
{
    /****************************************************************/
    /*                          定数宣言                            */
    /****************************************************************/
    // プレイヤー数
    public const int NUMBER_OF_PLAYERS = 2;

    // ノード数
    public const int NUMBER_OF_FIELD_NODES = 28;    // フィールド
    public const int NUMBER_OF_BENCH_NODES = 12;    // ベンチ
    public const int NUMBER_OF_PC_NODES = 4;        // PC
    public const int NUMBER_OF_WALK_NODES = NUMBER_OF_FIELD_NODES + NUMBER_OF_BENCH_NODES + NUMBER_OF_PC_NODES;  // フィールド+ベンチ(MP移動するノード)
    public const int NUMBER_OF_US_NODES = 12;       // ウルトラスペース
    public const int NUMBER_OF_REMOVED_NODES = 12;  // 除外ゾーン

    // 特別なノード番号
    // エントリー
    public const int NODE_ID_ENTRY_PLAYER0_LEFT = 21;
    public const int NODE_ID_ENTRY_PLAYER0_RIGHT = 27;
    public const int NODE_ID_ENTRY_PLAYER1_LEFT = 0;
    public const int NODE_ID_ENTRY_PLAYER1_RIGHT = 6;

    // ゴール
    public const int NODE_ID_GOAL_PLAYER0 = 24;
    public const int NODE_ID_GOAL_PLAYER1 = 3;

    // ベンチ(各プレイヤーについてFigureParameterのbenchIdの最若のものから連番)
    public const int NODE_ID_BENCH_PLAYER0_TOP = 28;
    public const int NODE_ID_BENCH_PLAYER1_TOP = 34;

    // PC
    // _Xは気絶して格納される側を0とした
    public const int NODE_ID_PC_PLAYER0_0 = 40;
    public const int NODE_ID_PC_PLAYER0_1 = 41;
    public const int NODE_ID_PC_PLAYER1_0 = 42;
    public const int NODE_ID_PC_PLAYER1_1 = 43;

    // US

    // 除外ゾーン

    // カスタムプロパティ用変数名
    private const string TURN_NUMBER = "turnNumber";
    private const string REST_TURN = "restTurn";

    private const string CURRENT_FIGURE_PLAYER_ID = "currentFigurePlayerId";
    private const string CURRENT_FIGURE_ID_ON_BOARD = "currentFigureIdOnBoard";

    private const string OPPONENT_FIGURE_PLAYER_ID = "opponentFigurePlayerId";
    private const string OPPONENT_FIGURE_ID_ON_BOARD = "opponentFigureIdOnBoard";


    private const string GOAL_ANGLE_0 = "goalAngle0";
    private const string GOAL_ANGLE_1 = "goalAngle1";

    // RPC用関数名

    private const string ON_BATTLE_START = "OnBattleStart";
    private const string ON_BATTLE_END = "OnBattleEnd";
    private const string DEATH_RPC = "DeathRPC";
    /****************************************************************/
    /*                          メンバ変数宣言                      */
    /****************************************************************/
    // あるノードから別のノードへのエッジをListで表現
    // edgesは配列であり、各成分がint型のList
    // 
    private List<int>[] edges   = new List<int>[NUMBER_OF_WALK_NODES];
    private int[][] entryNodeId = new int[NUMBER_OF_PLAYERS][];
    private int[] goalNodeId    = new int[NUMBER_OF_PLAYERS];
    private int[][] benchNodeId = new int[NUMBER_OF_PLAYERS][];
    private int[][] pcNodeId    = new int[NUMBER_OF_PLAYERS][];

    // 第一要素がプレイヤー番号(0 or 1)、第二要素がfigureIDOnBoard
    // ゲーム開始時に敵味方それぞれのフィギュアを認識してfigureIDOnBoardを振る
    private List<GameObject>[] figures = new List<GameObject>[NUMBER_OF_PLAYERS];

    // ノードの親要素
    [SerializeField] private GameObject nodes;

    // エッジ描画のためのprefab
    [SerializeField] private GameObject drawEdgePrefab;

    // ボードのUI
    [SerializeField] private GameObject startText;
    [SerializeField] private List<GameObject> playerTurnText;
    [SerializeField] private GameObject restTurnText;
    [SerializeField] private GameObject gameEndText;
    [SerializeField] private GameObject turnEndButton;
    [SerializeField] private GameObject forfeitButton;

    // スピンのオブジェクト
    [SerializeField] private GameObject arrow;
    [SerializeField] private List<GameObject> RouletteParentPlayer;

    // スピンのUI
    [SerializeField] private GameObject spinText;

    // そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[NUMBER_OF_WALK_NODES];

    // 移動、バトルの主体となるフィギュア（"使用"するフィギュア）
    private GameObject currentFigure = null;
    // 歩行範囲
    private List<int> walkCandidates = new List<int>();
    // 攻撃範囲
    private List<int> attackCandidates = new List<int>();
    // バトル相手のフィギュア
    private GameObject opponentFigure = null;
    // 効果の対象など
    private GameObject targetFigure = null;

    // どちらのターンかを表す。{0,1}で定められる
    private int turnNumber = 0;

    // 残りターン数
    private int restTurn = 300;

    public enum PhaseState
    {
        TurnStart,
        Normal,
        FigureSelected,
        Walking,
        AfterWalk,
        ConfirmFigure,
        BattleStart,
        BattleEnd,
        AfterBattle,
        TurnEnd,
        GameEnd, 
        Forfeit, 
        Lock
    };
    // ゲームの状態変数
    private PhaseState phaseState;

    // プレイヤーID。最初に入ったら0番、後に入ったら1番
    private int myPlayerId;

    //テスト用
    //デッキに入れたいポケモンの文字列を渡してやる
    [SerializeField] private List<String> deckList0 = new List<String>();
    [SerializeField] private List<String> deckList1 = new List<String>();

    //カメラの位置
    [SerializeField] private Transform cameraTransform;

    [SerializeField] GameObject spinMaster;
    //private float goalAngle;
    private int senderIdFromRouletteParent;
    [SerializeField] private List<GameObject> RouletteParents = new List<GameObject>();
    /***************************************************************/
    /*                      プロトタイプ関数宣言                   */
    /***************************************************************/

    /****************************************************************/
    /*                          関数定義                            */
    /****************************************************************/
    // Start is called before the first frame update
    void Start()
    {
        //同期をオンにする
        PhotonNetwork.IsMessageQueueRunning = true;

        //プレイヤーIDの設定
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Debug.Log("プレイヤーIDは" + myPlayerId);


        //プレイヤーIDが1ならばカメラの位置を動かす
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, -1, 0);
            cameraTransform.Rotate(0, 0, 180f);
            
            for(int i = 0; i < spinText.transform.childCount; i++)
            {
                spinText.transform.GetChild(i).transform.Rotate(0, 0, 180f);
            }
            
            
        }
        // ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();
        EdgeDraw();

        //デッキに登録しているフィギュアをインスタンス化
        InstantiateMyFigure();

        // イベントにイベントハンドラーを追加
        SceneManager.sceneLoaded += SceneLoaded;

        StartCoroutine(GameStart());
    }
    void Update()
    {

    }

    //ゲーム開始
    IEnumerator GameStart()
    {
        //Debug.Log(PhotonNetwork.CountOfPlayersInRooms);
        //2人来るまで待機
        while (PhotonNetwork.PlayerList.Length < NUMBER_OF_PLAYERS)
        {
            yield return null;
        }
        //ここは本当は相手のフィギュアが現れたことを確認したら、に変える
        yield return new WaitForSeconds(1);
        DivideFigures();
        /* UIカット
        yield return FadeInOut(startText, 1);
        Destroy(startText);
        */

        //暫定的にプレイヤー0のターンに固定
        turnNumber = 0;

        if(myPlayerId == 0)
        {
            StartCoroutine(SetPhaseState(PhaseState.TurnStart));
        }
        else if(myPlayerId == 1)
        {
            StartCoroutine(SetPhaseState(PhaseState.Lock));
        }


        Debug.Log("プレイヤー" + turnNumber + "のターンです");
    }

    // ボード、フィギュアの初期化処理
    // ボードのデータ構造を表現
    void CreateBoard()
    {
        // 暫定のデータ構造
        // ベンチ、PC、US、除外を連番にするか別にするかは検討
        for (int i = 0; i < NUMBER_OF_WALK_NODES; i++)
        {
            nodes.transform.GetChild(i).GetComponent<NodeParameter>().SetNodeID(i);
            edges[i] = new List<int>();
        }

        // フィールドノード間のエッジ定義
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

        // ベンチからエントリーへのエッジ(Player0側)
        edges[28].Add(21); edges[28].Add(27);
        edges[29].Add(21); edges[29].Add(27);
        edges[30].Add(21); edges[30].Add(27);
        edges[31].Add(21); edges[31].Add(27);
        edges[32].Add(21); edges[32].Add(27);
        edges[33].Add(21); edges[33].Add(27);

        // ベンチからエントリーへのエッジ(Player1側)
        edges[34].Add(0); edges[34].Add(6);
        edges[35].Add(0); edges[35].Add(6);
        edges[36].Add(0); edges[36].Add(6);
        edges[37].Add(0); edges[37].Add(6);
        edges[38].Add(0); edges[38].Add(6);
        edges[39].Add(0); edges[39].Add(6);

        // エントリー
        // entryNodeId[PlayerId][Left/Right]
        // 左右はPlayer0側視点
        for (int i = 0; i < entryNodeId.Length; i++)
        {
            entryNodeId[i] = new int[NUMBER_OF_PLAYERS];
        }
        entryNodeId[0][0] = NODE_ID_ENTRY_PLAYER0_LEFT; entryNodeId[0][1] = NODE_ID_ENTRY_PLAYER0_RIGHT;
        entryNodeId[1][0] = NODE_ID_ENTRY_PLAYER1_LEFT; entryNodeId[1][1] = NODE_ID_ENTRY_PLAYER1_RIGHT;

        // ゴール
        // goal[PlayerId]
        goalNodeId[0] = NODE_ID_GOAL_PLAYER0;
        goalNodeId[1] = NODE_ID_GOAL_PLAYER1;

        // ベンチ
        // benchNodeId[PlayerId][figureParameter.benchId]
        for (int i = 0; i < benchNodeId.Length; i++)
        {
            benchNodeId[i] = new int[6];
        }
        for (int i = 0; i < 6; i++)
        {
            benchNodeId[0][i] = NODE_ID_BENCH_PLAYER0_TOP + i;
        }
        for (int i = 0; i < 6; i++)
        {
            benchNodeId[1][i] = NODE_ID_BENCH_PLAYER1_TOP + i;
        }

        // PC
        // pcNodeId[PlayerId][X]
        // X = 0: 先に入るPC, X = 1: 後に入るPC
        for (int i = 0; i < pcNodeId.Length; i++)
        {
            pcNodeId[i] = new int[NUMBER_OF_PLAYERS];
        }
        pcNodeId[0][0] = NODE_ID_PC_PLAYER0_0; pcNodeId[0][1] = NODE_ID_PC_PLAYER0_1;
        pcNodeId[1][0] = NODE_ID_PC_PLAYER1_0; pcNodeId[1][1] = NODE_ID_PC_PLAYER1_1;
    }

    // エッジの描画
    void EdgeDraw()
    {
        for (int i = 0; i < NUMBER_OF_FIELD_NODES; i++)
        {
            for (int j = 0; j < edges[i].Count; j++)
            {
                GameObject obj = Instantiate(drawEdgePrefab, transform.position, Quaternion.identity);
                obj.transform.SetParent(transform);
                LineRenderer line = drawEdgePrefab.GetComponent<LineRenderer>();
                line.sortingOrder = 1;
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;

                // 頂点の数を決める
                line.positionCount = 2;

                Vector3 startDrawPosition = nodes.transform.GetChild(i).position;

                Vector3 endDrawPosition = nodes.transform.GetChild(edges[i][j]).position;
                line.SetPosition(0, startDrawPosition);
                line.SetPosition(1, endDrawPosition);

            }
        }
    }

    // 各フィギュアをplayerIDに従って分けてfigureという箱に入れる
    // 配列の第二要素のインデックスに対応する値(figureIdOnBoard)を各フィギュアに割り振る
    void InstantiateMyFigure()
    {
        GameObject obj;
        //本当はデッキを参照する
        //テストだからゆるして
        if (myPlayerId == 0)
        {
            for (int i = 0; i < deckList0.Count; i++)
            {
                obj = PhotonNetwork.Instantiate(deckList0[i], transform.position, Quaternion.identity, 0);
                obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.blue;
            }

        }

        else if (myPlayerId == 1)
        {
            for (int i = 0; i < deckList1.Count; i++)
            {
                obj = PhotonNetwork.Instantiate(deckList1[i], transform.position, Quaternion.identity, 0);
                obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }

    }

    // 各フィギュアをplayerIDに従って分けてfigureという箱に入れる
    // 配列の第二要素のインデックスに対応する値(figureIdOnBoard)を各フィギュアに割り振る
    void DivideFigures()
    {
        figures[0] = new List<GameObject>();
        figures[1] = new List<GameObject>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Figure");
        int count0 = 0;
        int count1 = 0;
        foreach (GameObject obj in objs)
        {
            FigureParameter figureParameter = obj.GetComponent<FigureParameter>();
            //Debug.Log(obj.GetComponent<PhotonView>().OwnerActorNr - 1);
            obj.GetComponent<FigureParameter>().SetPlayerId(obj.GetComponent<PhotonView>().OwnerActorNr - 1);

            if (figureParameter.GetPlayerId() == 0)
            {
                figureParameter.SetFigureIdOnBoard(figures[0].Count);
                figures[0].Add(obj);
                obj.GetComponent<FigureParameter>().SetBenchId(count0 + 28);
                obj.GetComponent<FigureParameter>().SetPosition(count0 + 28);
                obj.transform.position = nodes.transform.GetChild(28 + count0).transform.position;
                if (myPlayerId == 1)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count0++;
                //obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.blue;

                //Debug.Log(figures[0][figureParameter.GetFigureIdOnBoard()]);
            }
            else if (figureParameter.GetPlayerId() == 1)
            {
                figureParameter.SetFigureIdOnBoard(figures[1].Count);
                figures[1].Add(obj);
                obj.transform.Rotate(0, 0, 180f);
                obj.GetComponent<FigureParameter>().SetBenchId(count1 + 34);
                obj.GetComponent<FigureParameter>().SetPosition(count1 + 34);
                obj.transform.position = nodes.transform.GetChild(34 + count1).transform.position;
                if (myPlayerId == 0)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count1++;
                //obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
            }

        }
    }

    // マスク配列の作成。
    // とりあえず場にいるすべてのフィギュアを障害物と認識するだけだが、すり抜けとかの実装の際要拡張
    bool[] CreateNormalMask()
    {
        // 該当するノードのmaskNodeがtrueの時すり抜け可能、falseなら障害物扱い
        bool[] maskNode = new bool[NUMBER_OF_WALK_NODES];
        for (int i = 0; i < maskNode.Length; i++)
        {
            maskNode[i] = true;
        }
        for (int i = 0; i < figures.Length; i++)
        {
            for (int j = 0; j < figures[i].Count; j++)
            {
                FigureParameter figureParameter = figures[i][j].GetComponent<FigureParameter>();
                maskNode[figureParameter.GetPosition()] = false;
                // Debug.Log(figureParameter.GetPosition());
            }
        }

        return maskNode;
    }

    // 始点から各ノードへの距離を計算
    (int[] distances, int[] prevNode) CalculateDistance(int _startNode, bool[] _maskNode)
    {
        int[] distances = new int[NUMBER_OF_WALK_NODES];
        int[] prevNode = new int[NUMBER_OF_WALK_NODES];
        // 幅優先探索を素直に実装
        Queue<int> q = new Queue<int>();

        // 各ノードの到着フラグ
        bool[] reachedFlag = new bool[NUMBER_OF_WALK_NODES];

        // distancesと到着フラグの初期化
        for (int i = 0; i < NUMBER_OF_WALK_NODES; i++)
        {
            distances[i] = 99;
            reachedFlag[i] = false;
            prevNode[i] = -1;
        }

        // 初期ノードについての処理
        q.Enqueue(_startNode);
        reachedFlag[_startNode] = true;
        distances[_startNode] = 0;

        while (true)
        {
            // キューの先頭のノードを取り出す
            int currentNode = q.Dequeue();
            for (int i = 0; i < edges[currentNode].Count; i++)
            {
                // 現在ノードにつながっているノード（=nextNode)を見る
                int nextNode = edges[currentNode][i];
                // nextNodeに到着していなければ
                // maskがfalseのノードは障害物アリなので行けない
                if (reachedFlag[nextNode] == false && _maskNode[nextNode] == true)
                {
                    // キューにnextNodeを入れ、到着フラグをtrueにし、distanceの計算をし、どこから来たのかを記録
                    q.Enqueue(nextNode);
                    reachedFlag[nextNode] = true;
                    distances[nextNode] = distances[currentNode] + 1;
                    prevNode[nextNode] = currentNode;
                }

            }
            // キューの中身が空になればループから抜ける
            if (q.Count == 0)
            {
                break;
            }
        }
        return (distances, prevNode);
    }

    // 始点からの距離がmoveNumber以下のノードを出力
    List<int> FindCandidateofDestinaitonLessThan(int _moveNumber, int[] _distances)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < NUMBER_OF_FIELD_NODES; i++)
        {
            // 始点から各ノードへの距離がmoveNumber以下であるノードを全て格納
            if (_distances[i] <= _moveNumber)
            {
                candidates.Add(i);
            }
        }
        return candidates;
    }

    // 始点から終点までのルートを出力（最短距離を表現するような全ノードを列挙）
    Stack<int> DecideRoute(int _destNode, int[] _prevNode)
    {
        // 始点と終点を与えたときそれらのノード間の最短経路を格納
        Stack<int> route = new Stack<int>();

        int currentNode = _destNode;
        while (true)
        {
            // 終点から始点に向かってノードを積み上げていく
            route.Push(currentNode);
            currentNode = _prevNode[currentNode];
            if (currentNode == -1)
            {
                break;
            }
        }
        return route;
    }

    
    // 手番中のユーザ入力処理(クリックされたのがフィギュアかノードかで処理を分ける)

    // フィギュアがクリックされたときの処理
    public void FigureClicked(int _playerId, int _figureIdOnBoard)
    {
        // fはテスト用
        //GameObject f = figures[_playerId][_figureIdOnBoard];
        //Debug.Log(f + "の位置は" + f.GetComponent<FigureParameter>().GetPosition());

        switch (phaseState)
        {
        // ノーマル状態の時
        case PhaseState.Normal:
            // 遷移1番: Normal → FigureSelect
            // 遷移条件: 任意のフィギュアをクリック

            // currentFigure更新
            currentFigure = figures[_playerId][_figureIdOnBoard];
            SetCurrentFigureCustomProperty();

            // 状態変数更新
            // moveCandidates, walkCandidates, prevNodeの更新
            StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
            break;

        // フィギュア選択状態
        case PhaseState.FigureSelected:
            // 遷移2番: FigureSelect → Normal
            // 遷移条件: クリックしたフィギュアがcurrentFigureと同じ
            if (figures[_playerId][_figureIdOnBoard] == currentFigure)
            {
                StartCoroutine(SetPhaseState(PhaseState.Normal));
            }

            // 遷移3番: FigureSelect→FigureSelect
            else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() == turnNumber)
            {
                ClearNodesColor();
                // 光沢を初期化
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);
                // currentFigure更新
                currentFigure = figures[_playerId][_figureIdOnBoard];
                SetCurrentFigureCustomProperty();
               
                // 状態変数更新
                StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
            }

            // 相手のコマをタッチしたとき
            else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() != turnNumber)
            {
                bool attackOK = false;
                foreach (int i in attackCandidates)
                {
                    if (i == figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPosition())
                    {
                        attackOK = true;
                    }
                }

                // 遷移3番: FigureSelect→FigureSelect
                // 敵が攻撃範囲内にいなかったとき
                if (attackOK == false)
                {
                    ClearNodesColor();
                    // 光沢を初期化
                    currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                    currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);

                    // currentFigure更新
                    currentFigure = figures[_playerId][_figureIdOnBoard];
                    SetCurrentFigureCustomProperty();
                    
                    // 状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                }

                // 遷移5番: FigureSelect→Battle
                // 敵が攻撃範囲にいたとき
                else
                {
                    opponentFigure = figures[_playerId][_figureIdOnBoard];
                    SetOpponentFigureCustomProperty();

                    StartCoroutine(SetPhaseState(PhaseState.BattleStart));
                }
            }
            break;
        
        // フィギュア移動中状態
        case PhaseState.Walking:
            // 何も行わない
            break;

        // フィギュア移動後状態
        case PhaseState.AfterWalk:
            // 遷移10番: AfterWalk→AfterWalk
            if (figures[_playerId][_figureIdOnBoard] == currentFigure)
            {
                // 情報表示だけ
            }

            // 自分のフィギュアをタッチしたとき
            // 遷移7番: AfterWalk→ConfirmFigure
            else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() == turnNumber)
            {
                StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
            }

            // 相手フィギュアをタッチしたとき
            else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() != turnNumber)
            {
                bool attackOK = false;
                foreach (int i in attackCandidates)
                {
                    if (i == figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPosition())
                    {
                        attackOK = true;
                    }
                }

                // 遷移7番: AfterWalk→ConfirmFigure
                // 敵が攻撃範囲にいなかったとき
                if (attackOK == false)
                {
                    // 状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
                }

                // 遷移11番: FigureSelect→Batle
                // 敵が攻撃範囲にいたとき
                else
                {
                    opponentFigure = figures[_playerId][_figureIdOnBoard];
                    SetOpponentFigureCustomProperty();

                    StartCoroutine(SetPhaseState(PhaseState.BattleStart));
                }
            }
            break;

        // フィギュア確認状態
        case PhaseState.ConfirmFigure:
            // 遷移8番: ConfirmFigure →ConfirmFigure
            if (figures[_playerId][_figureIdOnBoard] != currentFigure)
            {
                // 情報表示だけ
            }

            // 遷移9番: ConfirmFigure→AfterWalk
            else if (figures[_playerId][_figureIdOnBoard] == currentFigure)
            {
                StartCoroutine(SetPhaseState(PhaseState.AfterWalk));
            }
            break;

        default:
            break;
        }
    }

    // 遷移4番 FigureSelect → Walking
    // FigureSelect状態でcandidates内のノードがクリックされたら、選択中のフィギュアに経路情報を渡す
    public IEnumerator NodeClicked(int _nodeId)
    {
        // 状態チェック
        if (phaseState != PhaseState.FigureSelected)
        {
            yield break;
        }

        // 選択フィギュアの所有権チェック
        if (currentFigure.GetComponent<FigureParameter>().GetPlayerId() != turnNumber)
        {
            Debug.Log("Current Player ID = " + currentFigure.GetComponent<FigureParameter>().GetPlayerId() + ", turnNumber = " + turnNumber);
            yield break;
        }

        // ウェイトチェック
        if (currentFigure.GetComponent<FigureParameter>().GetWaitCount() >= 1)
        {
            yield break;
        }

        // MP移動先候補とクリックされたノードIDが一致したらWalking状態に遷移して移動を行い、
        // その後AfterWalk状態に遷移する
        for (int i = 0; i < walkCandidates.Count; i++)
        {
            if (_nodeId == walkCandidates[i])
            {
                StartCoroutine(SetPhaseState(PhaseState.Walking));
                // ここの引数のprevNodeはFigureSelectedが呼ばれたときに格納されているよ

                Stack<int> route = DecideRoute(_nodeId, prevNode);
                if (currentFigure.GetComponent<FigureParameter>().GetPosition() >= NUMBER_OF_FIELD_NODES)
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

    // ゲームの状態変数のゲッター、セッター
    public IEnumerator SetPhaseState(PhaseState _tempState)
    {
        phaseState = _tempState;
        Debug.Log(GetPhaseState());

        // ターンの開始時
        if (phaseState == PhaseState.TurnStart)
        {

            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
            /*yield return FadeInOut(playerTurnText[turnNumber], 0.5f);
            */
            StartCoroutine(SetPhaseState(PhaseState.Normal));
        }
        else if (phaseState == PhaseState.Normal)
        {
            // 各種変数の初期化
            // 光沢を消す
            if (currentFigure != null)
            {
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 0.0f);
                currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 1.0f);
            }

            currentFigure = null;
            SetCurrentFigureCustomProperty();

            opponentFigure = null;
            SetOpponentFigureCustomProperty();

            for (int i = 0; i < prevNode.Length; i++)
            {
                prevNode[i] = -1;
            }
            walkCandidates.Clear();
            attackCandidates.Clear();

            ClearNodesColor();
        }
        else if (phaseState == PhaseState.FigureSelected)
        {
            // 選択したフィギュアを目立たせる
            currentFigure.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
            currentFigure.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.0f);


            FigureParameter currentFigureParameter = currentFigure.GetComponent<FigureParameter>();
            // 他のフィギュアを障害物として認識
            bool[] maskWalkNode = CreateNormalMask();
            // 現在地から全ノードへの距離を計算
            var calculateWalkDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskWalkNode);
            // この計算の時にDecideRouteに必要なprevNodeを記録
            prevNode = calculateWalkDistance.prevNode;
            // 現在地からmp以内で移動可能な全ノードの色を紫色にする
            if (restTurn == 300)
            {
                walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetMp() - 1, calculateWalkDistance.distances);
            }
            else
            {
                walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetMp(), calculateWalkDistance.distances);
            }

            for (int i = 0; i < walkCandidates.Count; i++)
            {
                nodes.transform.GetChild(walkCandidates[i]).GetComponent<SpriteRenderer>().color = Color.magenta;
            }

            // 攻撃範囲の計算
            bool[] maskAttackNode = new bool[NUMBER_OF_WALK_NODES];
            for (int i = 0; i < maskAttackNode.Length; i++)
            {
                maskAttackNode[i] = true;
            }

            var calculateAttackDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetAttackRange(), calculateAttackDistance.distances);
        }
        else if (phaseState == PhaseState.AfterWalk)
        {
            ClearNodesColor();

            // 包囲処理
            int currentFigureNode = currentFigure.GetComponent<FigureParameter>().GetPosition();
            foreach (int i in edges[currentFigureNode])
            {
                GameObject adjacentFigure = GetFigureOnBoard(i);
                yield return KnockedOutBySurrounding(adjacentFigure);
            }

            // 周囲に敵が誰もいなければターンエンド
            // 実際はこれに加えて眠り、氷、mpマイナスマーカーの味方がいたとき

            FigureParameter currentFigureParameter = currentFigure.GetComponent<FigureParameter>();
            bool opponentExistInAttackCandidates = false;   // バトル候補がいるかどうか
            int opponentID = GetTheOtherPlayerId(currentFigureParameter.GetPlayerId());

            bool[] maskAttackNode = new bool[NUMBER_OF_FIELD_NODES];
            for (int i = 0; i < maskAttackNode.Length; i++)
            {
                maskAttackNode[i] = true;
            }
            var calculateAttackDistance = CalculateDistance(currentFigureParameter.GetPosition(), maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.GetAttackRange(), calculateAttackDistance.distances);
            // Debug.Log(opponentID);

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
            // 今ゴール処理は通常移動後しか考えてない。ワザの効果で移動するときもな
            if (currentFigure.GetComponent<FigureParameter>().GetPosition() == goalNodeId[opponentID])
            {
                StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }
            else if (opponentExistInAttackCandidates == false)
            {
                StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
            }
            else if (opponentExistInAttackCandidates == true)
            {
                turnEndButton.SetActive(true);
            }

        }
        else if (phaseState == PhaseState.BattleStart)
        {
            //ボードのオブジェクトを消し、スピンのオブジェクトを出してカメラ位置調整
            ClearNodesColor();

            //IEnumerator coroutine = OnBattleStart();
            photonView.RPC(ON_BATTLE_START, RpcTarget.All);
            



            /*
            SpinController spinController = GameObject.Find("SpinMaster").GetComponent<SpinController>();
            yield return StartCoroutine(spinController.SpinStart());
            
            StartCoroutine(SetPhaseState(PhaseState.BattleEnd));
            photonView.RPC("Test", RpcTarget.Others);
            */
        }
        else if(phaseState == PhaseState.BattleEnd)
        {
            //スピンのオブジェクトを消し、ボードのオブジェクトを出してカメラ位置調整
            //photonView.RPC(ON_BATTLE_END, RpcTarget.All);
            // バトル後の処理
            StartCoroutine(SetPhaseState(PhaseState.AfterBattle));
        }
        else if (phaseState == PhaseState.AfterBattle)
        {
            // バトル後の処理をここに

            // バトル結果の受け取り
            var BattleResult = SpinController.GetBattleResult();
            int result = BattleResult.Item1;
            bool currentMoveAwake = BattleResult.Item2;
            bool opponentMoveAwake = BattleResult.Item3;
            bool currentDeath = BattleResult.Item4;
            bool opponentDeath = BattleResult.Item5;
            Debug.Log("boardで" + BattleResult);
            // if(currentMoveAwake){}
            // if(opponentMoveAwake){}
            if (currentDeath)
            {
                yield return Death(currentFigure);
            }
            if (opponentDeath)
            {
                photonView.RPC("DeathRPC", RpcTarget.Others);
            }
            StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
        }
        else if (phaseState == PhaseState.TurnEnd)
        {
            // 相手のターンにして残りのターン数を更新

            turnNumber = (turnNumber + 1) % 2;
            restTurn--;
            SetTurnCustomProperty();

            // ウェイトが付いているフィギュアのウェイトを更新
            // ウェイト0になったらウェイトカウンターの描画を終了する
            for (int i = 0; i < 2; i++)
            {
                List<GameObject> deck = figures[i];
                foreach (GameObject figure in deck)
                {
                    if (figure.GetComponent<FigureParameter>().GetWaitCount() >= 1)
                    {
                        figure.GetComponent<FigureParameter>().DecreaseWaitCount();
                    }
                }
            }

            turnEndButton.SetActive(false);
            // Debug.Log("プレイヤー" + turnNumber + "のターンです");

        }
        else if (phaseState == PhaseState.GameEnd)
        {
            // もう少し真面目にかけ
            // 実際はどっかのタイミングで全部のフィギュアの位置調べて相手のゴールにいればなんちゃらみたいな感じかなあ知らんけど
            gameEndText.SetActive(true);
            gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + currentFigure.GetComponent<FigureParameter>().GetPlayerId() + " WIN!";
        }
        else if (phaseState == PhaseState.Forfeit)
        {
            gameEndText.SetActive(true);
            gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + (turnNumber + 1) % 2 + " WIN!";
        }

        else if (phaseState == PhaseState.Lock)
        {
            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
        }
        else
        {
            yield return null;
        }
        // yield return null;
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
        int playerID = _figure.GetComponent<FigureParameter>().GetPlayerId();

        for (int i = 0; i < figures[playerID].Count; i++)
        {
            if (figures[playerID][i].GetComponent<FigureParameter>().GetPosition() == pcNodeId[playerID][1])
            {

                backBenchFigure = figures[playerID][i];
            }
            else if (figures[playerID][i].GetComponent<FigureParameter>().GetPosition() == pcNodeId[playerID][0])
            {
                moveAnotherPCFigure = figures[playerID][i];
            }
        }

        // PC1からベンチへ移動
        if (backBenchFigure != null)
        {
            // ウェイト2を付与
            backBenchFigure.GetComponent<FigureParameter>().SetWaitCount(2);
            yield return backBenchFigure.GetComponent<FigureController>().FigureOneStepWalk(backBenchFigure.GetComponent<FigureParameter>().GetBenchId());
        }
        // PC0からPC1へ移動
        if (moveAnotherPCFigure != null)
        {
            yield return moveAnotherPCFigure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerID][1]);
        }
        // フィールドからPC0へ移動
        yield return _figure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerID][0]);
    }

    [PunRPC]
    private IEnumerator DeathRPC(GameObject _figure)
    {
        yield return StartCoroutine(Death(_figure));
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
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

    // Assets:  BoardScene\DontDestroyObjects\Canvas\TurnEndButton
    public void OnTurnEnd()
    {
        StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
    }

    // Assets:  BoardScene\DontDestroyObjects\Canvas\ForfeitButton
    public void OnForFeit()
    {
        StartCoroutine(SetPhaseState(PhaseState.Forfeit));
    }

    // figureが相手のフィギュアに包囲されているかを判定する
    public bool IsSurrounded(GameObject figure)
    {
        // figureの所有者のID
        int currentId = figure.GetComponent<FigureParameter>().GetPlayerId();
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

            if (currentId == adjacentFigure.GetComponent<FigureParameter>().GetPlayerId())
            {
                return false;
            }
        }

        return true;
    }

    // figureの包囲判定と気絶処理を行う
    public IEnumerator KnockedOutBySurrounding(GameObject figure)
    {
        if(null == figure)
        {
            yield break;
        }

        bool isSurrounded = IsSurrounded(figure);
        if (isSurrounded)
        {
            yield return Death(figure);
        }
    }

    // ボード上のノード(nodeId)にあるフィギュアオブジェクトを取得する
    // フィギュアが存在しない場合はnullを返す
    public GameObject GetFigureOnBoard(int nodeId)
    {
        for(int playerId = 0; NUMBER_OF_PLAYERS > playerId; playerId++)
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

    public int GetMyPlayerId()
    {
        return myPlayerId;
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public void SetTurnCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();

        // どちらのターンか、と残りターン数を共有
        roomHash.Add(TURN_NUMBER, turnNumber);
        roomHash.Add(REST_TURN, restTurn);

        // ルームにハッシュを送信する

        Debug.Log("ターン情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    public void SetCurrentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // currentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if(currentFigure != null)
        {
            roomHash.Add(CURRENT_FIGURE_PLAYER_ID, currentFigure.GetComponent<FigureParameter>().GetPlayerId());
            roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, currentFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard());
        }
        else
        {
            roomHash.Add(CURRENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("currentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    public void SetOpponentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // opponentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if (opponentFigure != null)
        {
            roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, opponentFigure.GetComponent<FigureParameter>().GetPlayerId());
            roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, opponentFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard());
        }
        else
        {
            roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("opponentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    // ここでいうsenderIdとは2枚のディスクのうちどちらか、という意味
    public void SetGoalAngleCustomProperty(int _senderId, float _goalAngle)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // 回転の終点を共有
        if(_senderId == 0)
        {
            roomHash.Add(GOAL_ANGLE_0, _goalAngle);
        }
        else
        {
            roomHash.Add(GOAL_ANGLE_1, _goalAngle);
        }

        senderIdFromRouletteParent = _senderId;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedRoomHash)
    {
        // 変更されたハッシュを受け取る
        // どちらのターンか
        {
            object value = null;
            if (changedRoomHash.TryGetValue(TURN_NUMBER, out value))
            {
                turnNumber = (int)value;
                Debug.Log("今のターンは" + turnNumber);

            }
        }
        // 残りターン数
        {
            object value = null;
            if (changedRoomHash.TryGetValue(REST_TURN, out value))
            {
                restTurn = (int)value;
                Debug.Log("残りターンは" + restTurn);
                if (myPlayerId == turnNumber)
                {
                    StartCoroutine(SetPhaseState(PhaseState.TurnStart));
                }
                else
                {
                    StartCoroutine(SetPhaseState(PhaseState.Lock));
                }
            }
        }

        // currentFigure
        {
            object value0 = null;
            object value1 = null;
            if(changedRoomHash.TryGetValue(CURRENT_FIGURE_PLAYER_ID, out value0) &&
               changedRoomHash.TryGetValue(CURRENT_FIGURE_ID_ON_BOARD, out value1))
            {
                if((int)value0 == -1 && (int)value1 == -1)
                {
                    currentFigure = null;
                }
                else
                {
                    currentFigure = figures[(int)value0][(int)value1];
                }
                Debug.Log("currentFigureは" + currentFigure);
            }

        }
        // opponentFigure
        {
            object value0 = null;
            object value1 = null;
            if (changedRoomHash.TryGetValue(OPPONENT_FIGURE_PLAYER_ID, out value0) &&
               changedRoomHash.TryGetValue(OPPONENT_FIGURE_ID_ON_BOARD, out value1))
            {
                if ((int)value0 == -1 && (int)value1 == -1)
                {
                    opponentFigure = null;
                }
                else
                {
                    opponentFigure = figures[(int)value0][(int)value1];
                }
                Debug.Log("opponentFigureは" + opponentFigure);
            }

        }
        //goalAngle
        // ここらへん相互参照してるからあまりよくなさそう
        {
            object value = null;
            if (changedRoomHash.TryGetValue(GOAL_ANGLE_0, out value))
            {
                RouletteParents[0].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[0].GetComponent<DiskSpin>().SetReceiveFlag(true);
                //goalAngle = (float)value;
                Debug.Log("送りてのgoalAngleは" + (float)value);
            }

        }
        {
            object value = null;
            if (changedRoomHash.TryGetValue(GOAL_ANGLE_1, out value))
            {
                RouletteParents[1].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[1].GetComponent<DiskSpin>().SetReceiveFlag(true);
                //goalAngle = (float)value;
                Debug.Log("送りてのgoalAngleは" + (float)value);
            }

        }

    }


    private void ActivateBoardObjects(bool _flag)
    {

        // ノードを表示/非表示にする
        for (int i = 0; i < NUMBER_OF_WALK_NODES; i++)
        {
            nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = _flag;
        }
        // エッジを表示/非表示にする
        for (int i = 0; i < this.transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LineRenderer>().enabled = _flag;
        }

        // フィギュアを表示/非表示にする
        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject obj in figures[i])
            {
                for (int j = 0; j < obj.transform.childCount; j++)
                {
                    obj.transform.GetChild(j).GetComponent<SpriteRenderer>().enabled = _flag;
                }
            }
        }

        // UIを表示/非表示にする
        turnEndButton.SetActive(_flag);
        restTurnText.GetComponent<TextMeshProUGUI>().enabled = _flag;
        forfeitButton.SetActive(_flag);


    }

    private void ActivateSpinObjects(bool _flag)
    {
        // スピンの赤い矢
        arrow.SetActive(_flag);

        // データディスクの親要素
        for(int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            RouletteParentPlayer[i].SetActive(_flag);
        }

        // バトル結果の文字
        spinText.SetActive(_flag);
    }

    //ノードの色を初期化
    private void ClearNodesColor()
    {
        for (int i = 0; i < NUMBER_OF_FIELD_NODES; i++)
        {
            nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    [PunRPC]
    private void OnBattleStart()
    {
        //ボードのオブジェクトを消す
        ActivateBoardObjects(false);
        //スピンのオブジェクトを出す
        ActivateSpinObjects(true);

        //プレイヤー1のみカメラ位置調整
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, 1, 0);

        }

        Debug.Log("バトル開始");

        SpinStartRPC();
        /*
        SpinController spinController = GameObject.Find("SpinMaster").GetComponent<SpinController>();
        yield return StartCoroutine(spinController.SpinStart());
        */



    }

    [PunRPC]
    public void OnBattleEnd()
    {
        // ボードのオブジェクトを出す
        ActivateBoardObjects(true);
        turnEndButton.SetActive(false);
        // スピンのオブジェクトを消す
        ActivateSpinObjects(false);

        // プレイヤー1のみカメラの位置調整
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, -1, 0);

        }
        // バトルを仕掛けられたプレイヤーはここでバトル結果待機

        // バトルを仕掛けたプレイヤーはここからバトル後処理へ
        // バトルを仕掛けたプレイヤーがクリックしてターンエンドする→turnNumberが変わる
       // →バトルを仕掛けられたプレイヤーが呼ばれるってなってしまってる
        if(myPlayerId == turnNumber)
        {
            StartCoroutine(SetPhaseState(PhaseState.BattleEnd));
        }

    }

    private void SpinStartRPC()
    {
        StartCoroutine(spinMaster.GetComponent<SpinController>().SpinStart());
    }

}
