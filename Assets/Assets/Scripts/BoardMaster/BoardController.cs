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
    private int test;
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
    private const string WHICH_TURN = "whichTurn";
    private const string REST_TURN = "restTurn";

    private const string CURRENT_FIGURE_PLAYER_ID = "currentFigurePlayerId";
    private const string CURRENT_FIGURE_ID_ON_BOARD = "currentFigureIdOnBoard";

    private const string OPPONENT_FIGURE_PLAYER_ID = "opponentFigurePlayerId";
    private const string OPPONENT_FIGURE_ID_ON_BOARD = "opponentFigureIdOnBoard";


    private const string GOAL_ANGLE_0 = "goalAngle0";
    private const string GOAL_ANGLE_1 = "goalAngle1";

    private const string IS_WAITING = "isWaiting";

    // RPC用関数名

    private const string ON_BATTLE_START = "OnBattleStart";
    private const string ON_BATTLE_END = "OnBattleEnd";
    private const string DEATH_RPC = "DeathRPC";
    private const string SEND_FLAG_TO_SPIN_CONTROLLER = "SendFlagToSpinController";
    public readonly string FIGURE_ONE_STEP_WALK = "FigureOneStepWalk";
    private const string GAME_END_RPC = "GameEndRPC";
    private const string FORFEIT_RPC = "ForfeitRPC";
    private const string SET_WAIT_COUNTER_RPC = "SetWaitCounterRPC";

    /****************************************************************/
    /*                          メンバ変数宣言                      */
    /****************************************************************/

    // インスペクタから設定するオブジェクト

    // ノードの親要素
    [SerializeField] private GameObject nodes;

    // エッジ描画のためのprefab
    [SerializeField] private GameObject drawEdgePrefab;

    // ボード用のUI
    [SerializeField] private GameObject startText;
    [SerializeField] private GameObject playerTurnText;
    [SerializeField] private GameObject restTurnText;
    [SerializeField] private GameObject gameEndText;
    [SerializeField] private GameObject turnEndButton;
    [SerializeField] private GameObject forfeitButton;
    [SerializeField] private GameObject playerNameText;
    [SerializeField] private GameObject opponentNameText;
    [SerializeField] private GameObject informationText;

    // システムUI
    [SerializeField] private GameObject backToLobbyButton;
    // [SerializeField] private GameObject exitGameButton;


    // スピン用のオブジェクト
    [SerializeField] private GameObject arrow;
    [SerializeField] private List<GameObject> RouletteParentPlayer;

    // スピン用のUI
    [SerializeField] private GameObject spinText;

    // spinMaster
    [SerializeField] GameObject spinMaster;

    // ルーレットの親要素
    [SerializeField] private List<GameObject> RouletteParents = new List<GameObject>();

    //テスト用
    //デッキに入れたいポケモンの文字列を渡してやる
    [SerializeField] private List<String> deckList0 = new List<String>();
    [SerializeField] private List<String> deckList1 = new List<String>();

    //カメラの位置
    [SerializeField] private Transform cameraTransform;


    // ゲームの進行に必要な変数

    // あるノードから別のノードへのエッジをListで表現
    // edgesは配列であり、各成分がint型のList
    private List<int>[] edges = new List<int>[NUMBER_OF_WALK_NODES];

    //特別なノードの意味づけ
    private int[][] entryNodeId = new int[NUMBER_OF_PLAYERS][];
    private int[] goalNodeId = new int[NUMBER_OF_PLAYERS];
    private int[][] benchNodeId = new int[NUMBER_OF_PLAYERS][];
    private int[][] pcNodeId = new int[NUMBER_OF_PLAYERS][];

    // 第一要素がプレイヤー番号(0 or 1)、第二要素がfigureIDOnBoard
    // ゲーム開始時に敵味方それぞれのフィギュアを認識してfigureIDOnBoardを振る
    private List<GameObject>[] figures = new List<GameObject>[NUMBER_OF_PLAYERS];

    // 移動、バトルの主体となるフィギュア（"使用"するフィギュア）
    private GameObject currentFigure = null;
    // バトル相手のフィギュア
    private GameObject opponentFigure = null;
    // 効果の対象などのフィギュア
    private GameObject targetFigure = null;

    // そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[NUMBER_OF_WALK_NODES];
    // 歩行範囲
    private List<int> walkCandidates = new List<int>();
    // 攻撃範囲
    private List<int> attackCandidates = new List<int>();

    // どちらのターンかを表す。{0,1}で定められる
    private int whichTurn = 0;
    // 残りターン数
    private int restTurn = 300;

    // プレイヤーID。最初に入ったら0番、後に入ったら1番
    private int myPlayerId;

    // スピン情報同期に使う
    private int senderIdFromRouletteParent;

    // 相手の行動を待っている状態 = true。相手の行動が終わったらfalseにセットしてもらってすぐtrueに戻す
    private bool isWaiting = true;
    // ゲームの状態変数
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
    private PhaseState phaseState;

    MoveList moveList;


    /***************************************************************/
    /*                      プロトタイプ関数宣言                   */
    /***************************************************************/

    /****************************************************************/
    /*                          関数定義                            */
    /****************************************************************/
    // Start is called before the first frame update
    void Start()
    {
        moveList = GetComponent<MoveList>();
        // ゲーム開始前の初期化処理
        InitializeGame();

        // ボードのデータ構造作成（ゲームの最初に1回だけ呼ばれる
        CreateBoard();
        EdgeDraw();

        // デッキに登録しているフィギュアをインスタンス化
        InstantiateMyFigure();

        // 2人揃ったらゲーム開始
        StartCoroutine(GameStart());
    }
    void Update()
    {

    }

    /****************************************************************/
    /*             　 ゲーム開始時に呼ばれる関数 　　               */
    /****************************************************************/
    // ゲーム開始前の初期化処理
    private void InitializeGame()
    {
        //同期をオンにする
        PhotonNetwork.IsMessageQueueRunning = true;

        //プレイヤーIDの設定
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Debug.Log("プレイヤーIDは" + myPlayerId);


        // プレイヤーIDが1ならばUIとカメラの位置を動かす
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, -1, 0);
            cameraTransform.Rotate(0, 0, 180f);

            for (int i = 0; i < spinText.transform.childCount; i++)
            {
                spinText.transform.GetChild(i).transform.Rotate(0, 0, 180f);
            }


        }

        // イベントにイベントハンドラーを追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ゲーム開始
    IEnumerator GameStart()
    {
        
        // 2人来るまで待機
        while (PhotonNetwork.PlayerList.Length < NUMBER_OF_PLAYERS)
        {
            yield return null;
        }

        // 部屋を隠す
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        playerNameText.GetComponent<Text>().text = PhotonNetwork.LocalPlayer.NickName;
        playerNameText.SetActive(true);
        opponentNameText.GetComponent<Text>().text = PhotonNetwork.PlayerListOthers[0].NickName;
        opponentNameText.SetActive(true);

        // ここは本当は相手のフィギュアが現れたことを確認したら、に変える
        yield return new WaitForSeconds(1);

        // 自分と相手のフィギュア情報をキャッシュし、規定場所に配置する
        DivideFigures();

        // どちらのターンかランダムに決められる
        whichTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties[WHICH_TURN];
        //whichTurn = (int);

        if (myPlayerId == whichTurn)
        {
            // 自分のターンならTurnStart状態へ
            StartCoroutine(SetPhaseState(PhaseState.TurnStart));
        }
        else
        {
            // 相手のターンなら何も動かせないようにする
            StartCoroutine(SetPhaseState(PhaseState.Lock));
        }


        Debug.Log("プレイヤー" + whichTurn + "のターンです");
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


    // 各プレイヤーがデッキにセットしていたフィギュアをネットワークオブジェクトとしてインスタンス化
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
                foreach (Transform child in obj.transform) {
                    child.gameObject.SetActive(false);
                }
            }

        }

        else if (myPlayerId == 1)
        {
            for (int i = 0; i < deckList1.Count; i++)
            {
                obj = PhotonNetwork.Instantiate(deckList1[i], transform.position, Quaternion.identity, 0);
                obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.blue;
                foreach (Transform child in obj.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }

        }

    }

    // 各フィギュアをplayerIDに従って分けてfigure[][]という箱に入れる(キャッシュ)
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
            obj.GetComponent<FigureParameter>().SetPlayerId(obj.GetComponent<PhotonView>().OwnerActorNr - 1);
            if (figureParameter.GetPlayerId() == 0)
            {
                // キャッシュ用のId(figureIdOnBoard)を配番する
                figureParameter.SetFigureIdOnBoard(figures[0].Count);
                // フィギュア情報をキャッシュ
                figures[0].Add(obj);
                // benchIdをセット
                obj.GetComponent<FigureParameter>().SetBenchId(count0 + 28);
                // 現在地情報更新
                obj.GetComponent<FigureParameter>().SetPosition(count0 + 28);
                // 現在地へ移動
                obj.transform.position = nodes.transform.GetChild(28 + count0).transform.position;
                // 相手のフィギュアなら輪郭を赤に
                if (myPlayerId == 1)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count0++;
            }
            else if (figureParameter.GetPlayerId() == 1)
            {
                // キャッシュ用のId(figureIdOnBoard)を配番する
                figureParameter.SetFigureIdOnBoard(figures[1].Count);
                // フィギュア情報をキャッシュ
                figures[1].Add(obj);
                // プレイヤー1のフィギュアは向きを反転
                obj.transform.Rotate(0, 0, 180f);
                // benchIdをセット
                obj.GetComponent<FigureParameter>().SetBenchId(count1 + 34);
                // 現在地情報更新
                obj.GetComponent<FigureParameter>().SetPosition(count1 + 34);
                // 現在地へ移動
                obj.transform.position = nodes.transform.GetChild(34 + count1).transform.position;
                // 相手のフィギュアなら輪郭を赤に
                if (myPlayerId == 0)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count1++;
            }

        }
        foreach (GameObject obj in objs)
        {
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(true);
            }
            obj.GetComponent<FigureParameter>().GetWaitCounter().SetActive(false);
        }

    }

    /****************************************************************/
    /*            フィギュア移動、気絶に使われる関数 　　           */
    /****************************************************************/

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

    // フィギュアがクリックされたときの処理
    public void FigureClicked(int _playerId, int _figureIdOnBoard)
    {

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
                else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() == whichTurn)
                {
                    ClearNodesColor();
                    // 光沢を初期化
                    currentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

                    // currentFigure更新
                    currentFigure = figures[_playerId][_figureIdOnBoard];
                    SetCurrentFigureCustomProperty();

                    // 状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                }

                // 相手のコマをタッチしたとき
                else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() != whichTurn)
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
                        currentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

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
                        if (currentFigure.GetComponent<FigureParameter>().GetPosition() >= 28)
                        {
                            return;
                        }
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
                else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() == whichTurn)
                {
                    StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
                }

                // 相手フィギュアをタッチしたとき
                else if (figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().GetPlayerId() != whichTurn)
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
                        if (currentFigure.GetComponent<FigureParameter>().GetPosition() >= 28)
                        {
                            return;
                        }
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

    // ノードがクリックされたときの処理
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
        if (currentFigure.GetComponent<FigureParameter>().GetPlayerId() != whichTurn)
        {

            Debug.Log("Current Player ID = " + currentFigure.GetComponent<FigureParameter>().GetPlayerId() + ", turnNumber = " + whichTurn);

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

    // 気絶の処理
    public IEnumerator Death(GameObject _figure)
    {
        Debug.Log("ルルーシュビブリタニアが命じる");
        GameObject backBenchFigure = null;
        GameObject moveAnotherPCFigure = null;
        int playerId = _figure.GetComponent<FigureParameter>().GetPlayerId();

        for (int i = 0; i < figures[playerId].Count; i++)
        {
            if (figures[playerId][i].GetComponent<FigureParameter>().GetPosition() == pcNodeId[playerId][1])
            {
                backBenchFigure = figures[playerId][i];
            }
            else if (figures[playerId][i].GetComponent<FigureParameter>().GetPosition() == pcNodeId[playerId][0])
            {
                moveAnotherPCFigure = figures[playerId][i];
            }
        }

        // PC1からベンチへ移動
        if (backBenchFigure != null)
        {
            int backBenchFigurePlayerId = backBenchFigure.GetComponent<FigureParameter>().GetPlayerId();
            int backBenchFigureIdOnBoard = backBenchFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard();
            // ウェイト2を付与
            photonView.RPC(SET_WAIT_COUNTER_RPC, RpcTarget.All, backBenchFigurePlayerId, backBenchFigureIdOnBoard, 2);
            yield return backBenchFigure.GetComponent<FigureController>().FigureOneStepWalk(backBenchFigure.GetComponent<FigureParameter>().GetBenchId());
        }
        // PC0からPC1へ移動
        if (moveAnotherPCFigure != null)
        {
            yield return moveAnotherPCFigure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerId][1]);
        }
        // フィールドからPC0へ移動
        yield return _figure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerId][0]);
        Debug.Log("やっと死んだ");
        SetWaitFlagCustomProperty(false);
    }

    // RPC用気絶処理呼び出し
    [PunRPC] private void DeathRPC(int playerId, int figureIdOnBoard)
    {
        GameObject _figure = figures[playerId][figureIdOnBoard];
        StartCoroutine(Death(_figure));
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
        foreach (int adjacentNode in edges[figure.GetComponent<FigureParameter>().GetPosition()])
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
        if (null == figure)
        {
            yield break;
        }

        bool isSurrounded = IsSurrounded(figure);
        if (isSurrounded)
        {
            int surroundedFigurePlayerId = figure.GetComponent<FigureParameter>().GetPlayerId();
            int surroundedFigureIdOnBoard = figure.GetComponent<FigureParameter>().GetFigureIdOnBoard();
            photonView.RPC(DEATH_RPC, RpcTarget.Others, surroundedFigurePlayerId, surroundedFigureIdOnBoard);

            isWaiting = true;
            while (isWaiting == true)
            {
                yield return null;
            }
            SetWaitFlagCustomProperty(true);
        }
    }

    /****************************************************************/
    /*             　ゲームの状態変数 アクセサ     　               */
    /****************************************************************/

    // ゲームの状態変数のセッター
    public IEnumerator SetPhaseState(PhaseState _tempState)
    {
        phaseState = _tempState;
        Debug.Log(GetPhaseState());

        // ターンの開始時
        if (phaseState == PhaseState.TurnStart)
        {

            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
            playerTurnText.GetComponent<TextMeshProUGUI>().color = Color.cyan * new Color(1, 1, 1, 0);
            playerTurnText.GetComponent<TextMeshProUGUI>().text = "PLAYER TURN";
            yield return FadeInOut(playerTurnText, 0.5f);

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
                currentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;
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
            currentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.green;

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
        }
        else if (phaseState == PhaseState.BattleEnd)
        {
            // バトルが終わったことをバトル仕掛けられた側に通知
            photonView.RPC(SEND_FLAG_TO_SPIN_CONTROLLER, RpcTarget.Others);

            //バトル後処理へ
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
            int currentMoveId = BattleResult.Item6;
            int opponentMoveId = BattleResult.Item7;

            Debug.Log("boardで" + BattleResult);
            if (currentMoveAwake)
            {
                yield return moveList.CallMoveEffect(currentMoveId, currentFigure.GetComponent<FigureParameter>().GetPlayerId());
            }
            if (opponentMoveAwake)
            {
                yield return moveList.CallMoveEffect(opponentMoveId, opponentFigure.GetComponent<FigureParameter>().GetPlayerId());
            }
            if (currentDeath)
            {
                //yield return Death(currentFigure);
                // yield returnしないと順番おかしくなる
                // でもIEnumeratorをRPCで呼ぶとおかしくなる→返り値を受け取らないため
                yield return StartCoroutine(Death(currentFigure));
                SetWaitFlagCustomProperty(true);
            }
            if (opponentDeath)
            {
                int opponentFigurePlayerId = opponentFigure.GetComponent<FigureParameter>().GetPlayerId();
                int opponentFigureIdOnBoard = opponentFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard();
                // ちゃんと呼ばれてない
                photonView.RPC(DEATH_RPC, RpcTarget.Others, opponentFigurePlayerId, opponentFigureIdOnBoard);

                isWaiting = true;
                while (isWaiting == true)
                {
                    yield return null;
                }
                SetWaitFlagCustomProperty(true);
            }
            Debug.Log("死んだ処理終わった");
            StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
        }
        else if (phaseState == PhaseState.TurnEnd)
        {
            // 相手のターンにして残りのターン数を更新

            whichTurn = (whichTurn + 1) % 2;
            restTurn--;
            SetTurnCustomProperty();

            currentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

            // カスタムプロパティ更新のとこに移動
            /*
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
            */
            turnEndButton.SetActive(false);
            // Debug.Log("プレイヤー" + turnNumber + "のターンです");

        }
        else if (phaseState == PhaseState.GameEnd)
        {
            // もう少し真面目にかけ
            // 実際はどっかのタイミングで全部のフィギュアの位置調べて相手のゴールにいればなんちゃらみたいな感じかな
            photonView.RPC(GAME_END_RPC, RpcTarget.All);
        }
        else if (phaseState == PhaseState.Forfeit)
        {
            // バグ修正のために今適当になってる
            informationText.SetActive(true);
            informationText.GetComponent<Text>().text = "投了ボタンが押されました";
            gameEndText.SetActive(true);
            gameEndText.GetComponent<TextMeshProUGUI>().color = Color.blue;
            gameEndText.GetComponent<TextMeshProUGUI>().text = "YOU LOSE!";
            backToLobbyButton.SetActive(true);
            // exitGameButton.SetActive(true);
            photonView.RPC(FORFEIT_RPC, RpcTarget.Others);
        }

        else if (phaseState == PhaseState.Lock)
        {
            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
            playerTurnText.GetComponent<TextMeshProUGUI>().color = Color.red * new Color(1, 1, 1, 0);
            playerTurnText.GetComponent<TextMeshProUGUI>().text = "OPPONENT TURN";
            yield return FadeInOut(playerTurnText, 0.5f);
        }
        else
        {
            yield return null;
        }
        // yield return null;
    }

    // ゲームの状態変数のゲッター
    public PhaseState GetPhaseState()
    {
        return phaseState;
    }

    /****************************************************************/
    /*             　 バトル前後の処理関数       　　               */
    /****************************************************************/

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
        /*
        if (myPlayerId == whichTurn)
        {
            StartCoroutine(SetPhaseState(PhaseState.BattleEnd));
        }
        */

    }

    // spinControllerで書いた方がよさそう
    private void SpinStartRPC()
    {
        StartCoroutine(spinMaster.GetComponent<SpinController>().SpinStart());
    }
    /****************************************************************/
    /*             　 コールバック関数           　　               */
    /****************************************************************/

    // シーン切り替わり時の処理
    void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
    }

    // Assets:  BoardScene\CanvasScreenSpace\TurnEndButton
    // ターンエンドボタンが押された時の処理
    public void OnTurnEnd()
    {
        StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
    }

    // 投了ボタンが押された時の処理
    // Assets:  BoardScene\CanvasScreenSpace\ForfeitButton
    public void OnForFeit()
    {
        StartCoroutine(SetPhaseState(PhaseState.Forfeit));
    }

    /****************************************************************/
    /*          カスタムプロパティ関連関数       　　               */
    /****************************************************************/

    // whichTurn, restTurnの送信
    public void SetTurnCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();

        // どちらのターンか、と残りターン数を共有
        roomHash.Add(WHICH_TURN, whichTurn);
        roomHash.Add(REST_TURN, restTurn);

        Debug.Log("ターン情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    public void SetWaitFlagCustomProperty(bool _flag)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        isWaiting = _flag;
        roomHash.Add(IS_WAITING, isWaiting);
        Debug.Log("動いて、いいよ......");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }
    // currentFigure情報の送信
    public void SetCurrentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // currentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if (currentFigure != null)
        {
            roomHash.Add(CURRENT_FIGURE_PLAYER_ID, currentFigure.GetComponent<FigureParameter>().GetPlayerId());
            roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, currentFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard());
        }
        // currentFigureがnullの場合は各変数に-1を入れて受信時にnullとして受け取る
        else
        {
            roomHash.Add(CURRENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(CURRENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("currentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    // opponentFigure情報の送信
    public void SetOpponentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // opponentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if (opponentFigure != null)
        {
            roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, opponentFigure.GetComponent<FigureParameter>().GetPlayerId());
            roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, opponentFigure.GetComponent<FigureParameter>().GetFigureIdOnBoard());
        }
        // opponentFigureがnullの場合は各変数に-1を入れて受信時にnullとして受け取る
        else
        {
            roomHash.Add(OPPONENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(OPPONENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("opponentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    // スピンの共有のために、バトルを仕掛ける側が回転の終点を送信
    // ここでいうsenderIdとは2枚のディスクのうちどちらか、という意味
    public void SetGoalAngleCustomProperty(int _senderId, float _goalAngle)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // 回転の終点を共有
        if (_senderId == 0)
        {
            roomHash.Add(GOAL_ANGLE_0, _goalAngle);
        }
        else
        {
            roomHash.Add(GOAL_ANGLE_1, _goalAngle);
        }

        // ここいまいち
        senderIdFromRouletteParent = _senderId;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }


    // カスタムプロパティに変更があった場合呼ばれるコールバック関数
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedRoomHash)
    {
        // 変更されたハッシュを受け取る
        // どちらのターンか
        {
            object value = null;
            if (changedRoomHash.TryGetValue(WHICH_TURN, out value))
            {
                whichTurn = (int)value;

            }
        }
        // 残りターン数
        {
            object value = null;
            if (changedRoomHash.TryGetValue(REST_TURN, out value))
            {
                restTurn = (int)value;

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

                if (myPlayerId == whichTurn)
                {
                    StartCoroutine(SetPhaseState(PhaseState.TurnStart));
                }
                else
                {
                    StartCoroutine(SetPhaseState(PhaseState.Lock));
                }
            }
        }

        {
            object value = null;
            if (changedRoomHash.TryGetValue(IS_WAITING, out value))
            {
                isWaiting = (bool)value;
            }
        }
        // currentFigure
        {
            object value0 = null;
            object value1 = null;
            if (changedRoomHash.TryGetValue(CURRENT_FIGURE_PLAYER_ID, out value0) &&
               changedRoomHash.TryGetValue(CURRENT_FIGURE_ID_ON_BOARD, out value1))
            {
                if ((int)value0 == -1 && (int)value1 == -1)
                {
                    currentFigure = null;
                }
                else
                {
                    currentFigure = figures[(int)value0][(int)value1];
                }
                Debug.Log("currentFigureは" + currentFigure);
            }

            // UIの非表示
            //turnEndButton.SetActive(false);

            // UIカット
            //restTurnText.GetComponent<TextMeshProUGUI>().enabled = false;

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
        // goalAngle
        // ここらへんもう少しスマートにしたい
        {
            object value = null;
            if (changedRoomHash.TryGetValue(GOAL_ANGLE_0, out value))
            {
                RouletteParents[0].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[0].GetComponent<DiskSpin>().SetReceiveFlag(true);
            }

        }
        {
            object value = null;
            if (changedRoomHash.TryGetValue(GOAL_ANGLE_1, out value))
            {
                RouletteParents[1].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[1].GetComponent<DiskSpin>().SetReceiveFlag(true);
            }
            // UIの表示
            // turnEndButton.SetActive(true);
            //turnEndButton.SetActive(true);
            // UIカット
            //restTurnText.GetComponent<TextMeshProUGUI>().enabled = true;

        }

    }

    /****************************************************************/
    /*      UI関係 or オブジェクト表示/非表示の関数                 */
    /****************************************************************/

    // UIのフェードイン/フェードアウト
    [PunRPC]
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

    // ボード用のオブジェクトを表示/非表示
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
        playerNameText.SetActive(_flag);
        opponentNameText.SetActive(_flag);


    }

    // スピン用の部ジェクトを表示/非表示
    private void ActivateSpinObjects(bool _flag)
    {
        // スピンの赤い矢
        arrow.SetActive(_flag);

        // データディスクの親要素
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
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
    private void GameEndRPC()
    {
        gameEndText.SetActive(true);
        if (myPlayerId == whichTurn)
        {
            gameEndText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            gameEndText.GetComponent<TextMeshProUGUI>().text = "YOU WIN!";
        }
        else
        {
            gameEndText.GetComponent<TextMeshProUGUI>().color = Color.blue;
            gameEndText.GetComponent<TextMeshProUGUI>().text = "YOU LOSE!";
        }
        backToLobbyButton.SetActive(true);
        // exitGameButton.SetActive(true);
    }
    [PunRPC]
    private void ForfeitRPC()
    {
        // バグ修正のために今適当になってる
        informationText.SetActive(true);
        informationText.GetComponent<Text>().text = "投了ボタンが押されました";
        gameEndText.SetActive(true);        
        gameEndText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        gameEndText.GetComponent<TextMeshProUGUI>().text = "YOU WIN!";
        
        backToLobbyButton.SetActive(true);
        // exitGameButton.SetActive(true);
    }

    /****************************************************************/
    /*      　　　　　　　各種アクセサ関数   　　　　               */
    /****************************************************************/

    // currentFigureのゲッター
    public GameObject GetCurrentFigure()
    {
        return currentFigure;
    }

    // opponentFigureのゲッター
    public GameObject GetOpponentFigure()
    {
        return opponentFigure;
    }

    // ボード上のノード(nodeId)にあるフィギュアオブジェクトを取得する
    // フィギュアが存在しない場合はnullを返す
    public GameObject GetFigureOnBoard(int nodeId)
    {
        for (int playerId = 0; NUMBER_OF_PLAYERS > playerId; playerId++)
        {
            foreach (GameObject figure in figures[playerId])
            {
                if (nodeId == figure.GetComponent<FigureParameter>().GetPosition())
                {
                    return figure;
                }
            }
        }

        return null;
    }

    // 引数に与えたフィギュアの持ち主の相手のPlayerIdを取得
    public int GetTheOtherPlayerId(int onePlayerId)
    {
        return (onePlayerId + 1) % 2;
    }

    // 自分のPlayerIdを取得
    public int GetMyPlayerId()
    {
        return myPlayerId;
    }

    // どちらのターンかを取得
    public int GetWhichTurn()
    {
        return whichTurn;
    }

    // Edgesを取得
    public List<int>[] GetEdges()
    {
        return edges;
    }

    /****************************************************************/
    /*      　　　　　　　Punコールバック   　　　　               */
    /****************************************************************/
    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log(player.NickName + "が退出しました");
        informationText.SetActive(true);
        informationText.GetComponent<Text>().text = "相手が退出したためロビーに戻ります";
        // StartCoroutine(SetPhaseState(PhaseState.Lock));
        backToLobbyButton.SetActive(true);
    }
    /****************************************************************/
    /*      　　　　　　　　　　その他　　   　　　　               */
    /****************************************************************/

    // バトル終了時の同期用(こういう処理をもう少しスマートに書きたい)
    [PunRPC] private void SendFlagToSpinController()
    {
        spinMaster.GetComponent<SpinController>().SetReceiveFlag(true);
    }

    [PunRPC] private void SetWaitCounterRPC(int _playerId, int _figureIdOnBoard, int _waitCount)
    {
        figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().SetWaitCount(_waitCount);
    }


}
