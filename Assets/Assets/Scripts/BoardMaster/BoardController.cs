/*****************************************************************/
/* Class:   BoardController.cs                                   */
/* Assets:  BoardScene\DontDestroyObjects\BoardMaster            */
/*****************************************************************/
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class BoardController : MonoBehaviourPunCallbacks
{
    /****************************************************************/
    /*                          メンバ変数宣言                      */
    /****************************************************************/


    // ノードの親要素
    [field: SerializeField] [field: RenameField("Nodes")]
    public GameObject Nodes{ get; private set; }

    // エッジ描画のためのprefab
    [SerializeField] private GameObject drawEdgePrefab;

    // UI系統は全部Localに移植したいね
    // ボード用のUI
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

    /*
    // アナログを使いから消しそう
    // スピン用のオブジェクト
    [SerializeField] private GameObject arrow;
    [SerializeField] private List<GameObject> RouletteParentPlayer;
    // スピン用のUI
    [SerializeField] private GameObject spinText;
    // ルーレットの親要素
    [SerializeField] private List<GameObject> RouletteParents = new List<GameObject>();
    // スピン情報同期に使う
    private int senderIdFromRouletteParent;
    */
    // アナログ用のオブジェクト
    [SerializeField] private List<DeckManager> deckManager;
    [SerializeField] private List<RouletteManager> rouletteManager;
    [SerializeField] private List<GameObject> battleUi;


    // spinMaster
    [SerializeField] GameObject spinMaster;
    private LocalController localController;


    //テスト用
    //デッキに入れたいポケモンの文字列を渡してやる
    [SerializeField] private List<String> deckList0 = new List<String>();
    [SerializeField] private List<String> deckList1 = new List<String>();

    //カメラの位置
    [SerializeField] private Transform cameraTransform;

    // ゲームの進行に必要な変数

    // あるノードから別のノードへのエッジをListで表現
    // Edgesは配列であり、各成分がint型のList
    public List<int>[] Edges { get; private set; } = new List<int>[CList.NUMBER_OF_WALK_NODES];

    //特別なノードの意味づけ
    private int[][] entryNodeId = new int[CList.NUMBER_OF_PLAYERS][];
    private int[] goalNodeId = new int[CList.NUMBER_OF_PLAYERS];
    private int[][] benchNodeId = new int[CList.NUMBER_OF_PLAYERS][];
    private int[][] pcNodeId = new int[CList.NUMBER_OF_PLAYERS][];

    // 第一要素がプレイヤー番号(0 or 1)、第二要素がfigureIDOnBoard
    // ゲーム開始時に敵味方それぞれのフィギュアを認識してfigureIDOnBoardを振る
    public List<GameObject>[] Figures { get; private set; } = new List<GameObject>[CList.NUMBER_OF_PLAYERS];

    // 移動、バトルの主体となるフィギュア（"使用"するフィギュア）
    public GameObject CurrentFigure { get; private set; } = null;
    // バトル相手のフィギュア
    public GameObject OpponentFigure { get; private set; } = null;
    // 効果の対象などのフィギュア
    public GameObject TargetFigure { get; private set; } = null;

    // そのノードにつく前はどこにいたのかを表す
    private int[] prevNode = new int[CList.NUMBER_OF_WALK_NODES];
    // 歩行範囲
    private List<int> walkCandidates = new List<int>();
    // 攻撃範囲
    private List<int> attackCandidates = new List<int>();

    // どちらのターンかを表す。{0,1}で定められる
    public int WhichTurn { get; private set; } = 0;
    // 残りターン数
    private int restTurn = 300;

    // プレイヤーID。最初に入ったら0番、後に入ったら1番
    // Localに移動したい
    private int myPlayerId = -1;
    // 名前変えたい
    private int enemyPlayerId = -1;
    private int winPlayerId = -1;



    // 相手の行動を待っている状態 = true。相手の行動が終わったらfalseにセットしてもらってすぐtrueに戻す
    private bool isWaiting = true;

    // 自分の行動が終わったことをマスターに伝えるフラグ（isWaitingじゃなくてこっちをつかう）
    public bool[] DoneFlag { get; private set; } = new bool[CList.NUMBER_OF_PLAYERS];

    // ルーレット抽選
    public int[] SpinResult { get; private set; } = new int[CList.NUMBER_OF_PLAYERS];
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
        Battle,
        BattleEnd,
        AfterBattle,
        TurnEnd,
        GameEnd,
        Forfeit,
        Lock,
        MoveEffectInput, 
        MoveEffectFigureSelected, 
        AfterGameEnd
    };
    private PhaseState phaseState;

    MoveList moveList;

    // MoveEffectInput状態などで参照する技と技を出したフィギュア情報
    private (int, int) InterestedMoveEffect;

    /****************************************************************/
    /*                          関数定義                            */
    /****************************************************************/
    // Start is called before the first frame update
    void Start()
    {
        
        localController = GameObject.Find("LocalMaster").GetComponent<LocalController>();
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

        // この変数はLocalで宣言すべき
        //プレイヤーIDの設定
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        enemyPlayerId = (myPlayerId + 1) % 2;
        Debug.Log("プレイヤーIDは" + myPlayerId);

        // Localへ移行しよう
        // プレイヤーIDが1ならばUIとカメラの位置を動かす
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, -1, 0);
            cameraTransform.Rotate(0, 0, 180f);


            localController.RotateUi();
            // アナログへの移行
            /*
            for (int i = 0; i < spinText.transform.childCount; i++)
            {
                spinText.transform.GetChild(i).transform.Rotate(0, 0, 180f);
            }
            */


        }

        // イベントにイベントハンドラーを追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        informationText.SetActive(true);
        informationText.GetComponent<Text>().text = "対戦相手を待っています";
    }

    // ゲーム開始
    IEnumerator GameStart()
    {
        

        // 2人来るまで待機
        while (PhotonNetwork.PlayerList.Length < CList.NUMBER_OF_PLAYERS)
        {
            yield return null;
        }
        informationText.GetComponent<Text>().text = "";
        informationText.SetActive(false);
        // 部屋を隠す
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Localに移動させる
        playerNameText.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
        playerNameText.SetActive(true);
        opponentNameText.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerListOthers[0].NickName;
        opponentNameText.SetActive(true);

        // ここは本当は相手のフィギュアが現れたことを確認したら、に変える
        yield return new WaitForSeconds(1);

        // 自分と相手のフィギュア情報をキャッシュし、規定場所に配置する
        DivideFigures();

        // どちらのターンかランダムに決められる
        WhichTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties[CList.WHICH_TURN];
        //whichTurn = (int);

        if (myPlayerId == WhichTurn)
        {
            // 自分のターンならTurnStart状態へ
            StartCoroutine(SetPhaseState(PhaseState.TurnStart));
        }
        else
        {
            // 相手のターンなら何も動かせないようにする
            StartCoroutine(SetPhaseState(PhaseState.Lock));
        }


        Debug.Log("プレイヤー" + WhichTurn + "のターンです");
    }

    // ボード、フィギュアの初期化処理
    // ボードのデータ構造を表現
    void CreateBoard()
    {
        // 暫定のデータ構造
        // ベンチ、PC、US、除外を連番にするか別にするかは検討
        for (int i = 0; i < CList.NUMBER_OF_WALK_NODES; i++)
        {
            Nodes.transform.GetChild(i).GetComponent<NodeParameter>().NodeID = i;
            Edges[i] = new List<int>();
        }

        // フィールドノード間のエッジ定義
        Edges[0].Add(1); Edges[0].Add(7); Edges[0].Add(8);
        Edges[1].Add(0); Edges[1].Add(2);
        Edges[2].Add(1); Edges[2].Add(3); Edges[2].Add(9);
        Edges[3].Add(2); Edges[3].Add(4);
        Edges[4].Add(3); Edges[4].Add(5);
        Edges[5].Add(4); Edges[5].Add(6);
        Edges[6].Add(5); Edges[6].Add(10); Edges[6].Add(11);
        Edges[7].Add(0); Edges[7].Add(12);
        Edges[8].Add(0); Edges[8].Add(9); Edges[8].Add(13);
        Edges[9].Add(2); Edges[9].Add(8); Edges[9].Add(10);
        Edges[10].Add(6); Edges[10].Add(9); Edges[10].Add(14);
        Edges[11].Add(6); Edges[11].Add(15);
        Edges[12].Add(7); Edges[12].Add(16);
        Edges[13].Add(8); Edges[13].Add(17);
        Edges[14].Add(10); Edges[14].Add(19);
        Edges[15].Add(11); Edges[15].Add(20);
        Edges[16].Add(12); Edges[16].Add(21);
        Edges[17].Add(13); Edges[17].Add(18); Edges[17].Add(21);
        Edges[18].Add(17); Edges[18].Add(19); Edges[18].Add(25);
        Edges[19].Add(14); Edges[19].Add(18); Edges[19].Add(27);
        Edges[20].Add(15); Edges[20].Add(27);
        Edges[21].Add(16); Edges[21].Add(17); Edges[21].Add(22);
        Edges[22].Add(21); Edges[22].Add(23);
        Edges[23].Add(22); Edges[23].Add(24);
        Edges[24].Add(23); Edges[24].Add(25);
        Edges[25].Add(18); Edges[25].Add(24); Edges[25].Add(26);
        Edges[26].Add(25); Edges[26].Add(27);
        Edges[27].Add(19); Edges[27].Add(20); Edges[27].Add(26);

        // ベンチからエントリーへのエッジ(Player0側)
        Edges[28].Add(21); Edges[28].Add(27);
        Edges[29].Add(21); Edges[29].Add(27);
        Edges[30].Add(21); Edges[30].Add(27);
        Edges[31].Add(21); Edges[31].Add(27);
        Edges[32].Add(21); Edges[32].Add(27);
        Edges[33].Add(21); Edges[33].Add(27);

        // ベンチからエントリーへのエッジ(Player1側)
        Edges[34].Add(0); Edges[34].Add(6);
        Edges[35].Add(0); Edges[35].Add(6);
        Edges[36].Add(0); Edges[36].Add(6);
        Edges[37].Add(0); Edges[37].Add(6);
        Edges[38].Add(0); Edges[38].Add(6);
        Edges[39].Add(0); Edges[39].Add(6);

        // エントリー
        // entryNodeId[PlayerId][Left/Right]
        // 左右はPlayer0側視点
        for (int i = 0; i < entryNodeId.Length; i++)
        {
            entryNodeId[i] = new int[CList.NUMBER_OF_PLAYERS];
        }
        entryNodeId[0][0] = CList.NODE_ID_ENTRY_PLAYER0_LEFT; entryNodeId[0][1] = CList.NODE_ID_ENTRY_PLAYER0_RIGHT;
        entryNodeId[1][0] = CList.NODE_ID_ENTRY_PLAYER1_LEFT; entryNodeId[1][1] = CList.NODE_ID_ENTRY_PLAYER1_RIGHT;

        // ゴール
        // goal[PlayerId]
        goalNodeId[0] = CList.NODE_ID_GOAL_PLAYER0;
        goalNodeId[1] = CList.NODE_ID_GOAL_PLAYER1;

        // ベンチ
        // benchNodeId[PlayerId][figureParameter.benchId]
        for (int i = 0; i < benchNodeId.Length; i++)
        {
            benchNodeId[i] = new int[6];
        }
        for (int i = 0; i < 6; i++)
        {
            benchNodeId[0][i] = CList.NODE_ID_BENCH_PLAYER0_TOP + i;
        }
        for (int i = 0; i < 6; i++)
        {
            benchNodeId[1][i] = CList.NODE_ID_BENCH_PLAYER1_TOP + i;
        }

        // PC
        // pcNodeId[PlayerId][X]
        // X = 0: 先に入るPC, X = 1: 後に入るPC
        for (int i = 0; i < pcNodeId.Length; i++)
        {
            pcNodeId[i] = new int[CList.NUMBER_OF_PLAYERS];
        }
        pcNodeId[0][0] = CList.NODE_ID_PC_PLAYER0_0; pcNodeId[0][1] = CList.NODE_ID_PC_PLAYER0_1;
        pcNodeId[1][0] = CList.NODE_ID_PC_PLAYER1_0; pcNodeId[1][1] = CList.NODE_ID_PC_PLAYER1_1;
    }

    // エッジの描画
    // Localへ移行しよう
    void EdgeDraw()
    {
        for (int i = 0; i < CList.NUMBER_OF_FIELD_NODES; i++)
        {
            for (int j = 0; j < Edges[i].Count; j++)
            {
                GameObject obj = Instantiate(drawEdgePrefab, transform.position, Quaternion.identity);
                obj.transform.SetParent(transform);
                LineRenderer line = drawEdgePrefab.GetComponent<LineRenderer>();
                line.sortingOrder = 1;
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;

                // 頂点の数を決める
                line.positionCount = 2;

                Vector3 startDrawPosition = Nodes.transform.GetChild(i).position;

                Vector3 endDrawPosition = Nodes.transform.GetChild(Edges[i][j]).position;
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
        Figures[0] = new List<GameObject>();
        Figures[1] = new List<GameObject>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Figure");
        int count0 = 0;
        int count1 = 0;

        foreach (GameObject obj in objs)
        {
            FigureParameter figureParameter = obj.GetComponent<FigureParameter>();
            obj.GetComponent<FigureParameter>().PlayerId = obj.GetComponent<PhotonView>().OwnerActorNr - 1;
            if (figureParameter.PlayerId == 0)
            {
                // キャッシュ用のId(figureIdOnBoard)を配番する
                figureParameter.FigureIdOnBoard = Figures[0].Count;
                // フィギュア情報をキャッシュ
                Figures[0].Add(obj);
                // benchIdをセット
                obj.GetComponent<FigureParameter>().BenchId = count0 + 28;
                // 現在地情報更新
                obj.GetComponent<FigureParameter>().Position = count0 + 28;
                // 現在地へ移動
                obj.transform.position = Nodes.transform.GetChild(28 + count0).transform.position;
                // 相手のフィギュアなら輪郭を赤に
                if (myPlayerId == 1)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count0++;
            }
            else if (figureParameter.PlayerId == 1)
            {
                // キャッシュ用のId(figureIdOnBoard)を配番する
                figureParameter.FigureIdOnBoard = Figures[1].Count;
                // フィギュア情報をキャッシュ
                Figures[1].Add(obj);
                // プレイヤー1のフィギュアは向きを反転
                obj.transform.Rotate(0, 0, 180f);
                // benchIdをセット
                obj.GetComponent<FigureParameter>().BenchId = count1 + 34;
                // 現在地情報更新
                obj.GetComponent<FigureParameter>().Position = count1 + 34;
                // 現在地へ移動
                obj.transform.position = Nodes.transform.GetChild(34 + count1).transform.position;
                // 相手のフィギュアなら輪郭を赤に
                if (myPlayerId == 0)
                {
                    obj.transform.Find("FigureBack2").GetComponent<SpriteRenderer>().color = Color.red;
                }
                count1++;
            }

        }
        // アナログルーレット用にキャッシュ
        for (int i = 0; i < CList.NUMBER_OF_PLAYERS; i++)
        {
            deckManager[i].InitializeDeck(i);
        }

        foreach (GameObject obj in objs)
        {
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(true);
            }
            obj.GetComponent<FigureParameter>().WaitCounter.SetActive(false);
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
        bool[] maskNode = new bool[CList.NUMBER_OF_WALK_NODES];
        for (int i = 0; i < maskNode.Length; i++)
        {
            maskNode[i] = true;
        }
        for (int i = 0; i < Figures.Length; i++)
        {
            for (int j = 0; j < Figures[i].Count; j++)
            {
                FigureParameter figureParameter = Figures[i][j].GetComponent<FigureParameter>();
                maskNode[figureParameter.Position] = false;
            }
        }

        return maskNode;
    }

    // 始点から各ノードへの距離を計算
    (int[] distances, int[] prevNode) CalculateDistance(int _startNode, bool[] _maskNode)
    {
        int[] distances = new int[CList.NUMBER_OF_WALK_NODES];
        int[] prevNode = new int[CList.NUMBER_OF_WALK_NODES];
        // 幅優先探索を素直に実装
        Queue<int> q = new Queue<int>();

        // 各ノードの到着フラグ
        bool[] reachedFlag = new bool[CList.NUMBER_OF_WALK_NODES];

        // distancesと到着フラグの初期化
        for (int i = 0; i < CList.NUMBER_OF_WALK_NODES; i++)
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
            for (int i = 0; i < Edges[currentNode].Count; i++)
            {
                // 現在ノードにつながっているノード（=nextNode)を見る
                int nextNode = Edges[currentNode][i];
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
        for (int i = 0; i < CList.NUMBER_OF_FIELD_NODES; i++)
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

    //Localに入力チャンネルみたいなものを作り制御
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
                CurrentFigure = Figures[_playerId][_figureIdOnBoard];
                SetCurrentFigureCustomProperty();

                // 状態変数更新
                // moveCandidates, walkCandidates, prevNodeの更新
                StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                break;

            // フィギュア選択状態
            case PhaseState.FigureSelected:
                // 遷移2番: FigureSelect → Normal
                // 遷移条件: クリックしたフィギュアがcurrentFigureと同じ
                if (Figures[_playerId][_figureIdOnBoard] == CurrentFigure)
                {
                    StartCoroutine(SetPhaseState(PhaseState.Normal));
                }

                // 遷移3番: FigureSelect→FigureSelect
                else if (Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().PlayerId == WhichTurn)
                {
                    ClearNodesColor();
                    // 光沢を初期化
                    CurrentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

                    // currentFigure更新
                    CurrentFigure = Figures[_playerId][_figureIdOnBoard];
                    SetCurrentFigureCustomProperty();

                    // 状態変数更新
                    StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                }

                // 相手のコマをタッチしたとき
                else if (Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().PlayerId != WhichTurn)
                {
                    bool attackOK = false;
                    foreach (int i in attackCandidates)
                    {
                        if (i == Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().Position)
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
                        CurrentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

                        // currentFigure更新
                        CurrentFigure = Figures[_playerId][_figureIdOnBoard];
                        SetCurrentFigureCustomProperty();

                        // 状態変数更新
                        StartCoroutine(SetPhaseState(PhaseState.FigureSelected));
                    }

                    // 遷移5番: FigureSelect→Battle
                    // 敵が攻撃範囲にいたとき
                    else
                    {
                        if (CurrentFigure.GetComponent<FigureParameter>().Position >= 28)
                        {
                            return;
                        }
                        OpponentFigure = Figures[_playerId][_figureIdOnBoard];
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
                if (Figures[_playerId][_figureIdOnBoard] == CurrentFigure)
                {
                    // 情報表示だけ
                }

                // 自分のフィギュアをタッチしたとき
                // 遷移7番: AfterWalk→ConfirmFigure
                else if (Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().PlayerId == WhichTurn)
                {
                    StartCoroutine(SetPhaseState(PhaseState.ConfirmFigure));
                }

                // 相手フィギュアをタッチしたとき
                else if (Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().PlayerId != WhichTurn)
                {
                    bool attackOK = false;
                    foreach (int i in attackCandidates)
                    {
                        if (i == Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().Position)
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
                        if (CurrentFigure.GetComponent<FigureParameter>().Position >= 28)
                        {
                            return;
                        }
                        OpponentFigure = Figures[_playerId][_figureIdOnBoard];
                        SetOpponentFigureCustomProperty();

                        StartCoroutine(SetPhaseState(PhaseState.BattleStart));
                    }
                }
                break;

            // フィギュア確認状態
            case PhaseState.ConfirmFigure:
                // 遷移8番: ConfirmFigure →ConfirmFigure
                if (Figures[_playerId][_figureIdOnBoard] != CurrentFigure)
                {
                    // 情報表示だけ
                }
                // 遷移9番: ConfirmFigure→AfterWalk
                else if (Figures[_playerId][_figureIdOnBoard] == CurrentFigure)
                {
                    StartCoroutine(SetPhaseState(PhaseState.AfterWalk));
                }
                break;
            case PhaseState.MoveEffectInput:

                break;
            case PhaseState.MoveEffectFigureSelected:
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
        Debug.Log("phaseState = " + phaseState);
        // 状態チェック
        switch (phaseState)
        {
        case PhaseState.FigureSelected:
            // 選択フィギュアの所有権チェック
            if (CurrentFigure.GetComponent<FigureParameter>().PlayerId != WhichTurn)
            {

                Debug.Log("Current Player ID = " + CurrentFigure.GetComponent<FigureParameter>().PlayerId + ", turnNumber = " + WhichTurn);

                yield break;
            }

            // ウェイトチェック
            if (CurrentFigure.GetComponent<FigureParameter>().WaitCount >= 1)
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
                    if (CurrentFigure.GetComponent<FigureParameter>().Position >= CList.NUMBER_OF_FIELD_NODES)
                    {
                        route.Pop();
                        yield return StartCoroutine(CurrentFigure.GetComponent<FigureController>().FigureOneStepWalk(route.Peek()));
                    }

                    if (route.Count >= 2)
                    {
                        yield return StartCoroutine(CurrentFigure.GetComponent<FigureController>().Figurewalk(route));
                    }
                    StartCoroutine(SetPhaseState(PhaseState.AfterWalk));
                }
            }
            break;

        case PhaseState.MoveEffectInput:
            var roomHash = new ExitGames.Client.Photon.Hashtable();
            object currentMoveId;
            //object nodeId = _nodeId;
            roomHash.TryGetValue(CList.AFFECT_MOVE_ID, out currentMoveId);
            //Debug.Log("currentMoveID = " + (int)currentMoveId);
            Debug.Log("nodeId = " + _nodeId);
            Debug.Log("playerId = " + CurrentFigure.GetComponent<FigureParameter>().PlayerId);
            yield return moveList.CallMoveEffect(1, CurrentFigure.GetComponent<FigureParameter>().PlayerId, _nodeId);
            //StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
            break;

        case PhaseState.MoveEffectFigureSelected:
            break;

        default:
            break;
        }

        

        yield break;
    }

    // 気絶の処理
    // 移動部分はLocalで
    public IEnumerator Death(GameObject _figure)
    {
        GameObject backBenchFigure = null;
        GameObject moveAnotherPCFigure = null;
        int playerId = _figure.GetComponent<FigureParameter>().PlayerId;

        for (int i = 0; i < Figures[playerId].Count; i++)
        {
            if (Figures[playerId][i].GetComponent<FigureParameter>().Position == pcNodeId[playerId][1])
            {
                backBenchFigure = Figures[playerId][i];
            }
            else if (Figures[playerId][i].GetComponent<FigureParameter>().Position == pcNodeId[playerId][0])
            {
                moveAnotherPCFigure = Figures[playerId][i];
            }
        }

        // PC1からベンチへ移動
        if (backBenchFigure != null)
        {
            int backBenchFigurePlayerId = backBenchFigure.GetComponent<FigureParameter>().PlayerId;
            int backBenchFigureIdOnBoard = backBenchFigure.GetComponent<FigureParameter>().FigureIdOnBoard;
            // ウェイト2を付与
            photonView.RPC(CList.SET_WAIT_COUNTER_RPC, RpcTarget.All, backBenchFigurePlayerId, backBenchFigureIdOnBoard, 2);
            yield return backBenchFigure.GetComponent<FigureController>().FigureOneStepWalk(backBenchFigure.GetComponent<FigureParameter>().BenchId);
        }
        // PC0からPC1へ移動
        if (moveAnotherPCFigure != null)
        {
            yield return moveAnotherPCFigure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerId][1]);
        }
        // フィールドからPC0へ移動
        yield return _figure.GetComponent<FigureController>().FigureOneStepWalk(pcNodeId[playerId][0]);
        Debug.Log("やっと死んだ");

        // doneFlagに統合したい

        SetWaitFlagCustomProperty(false);
    }

    // RPC用気絶処理呼び出し
    [PunRPC] private void DeathRPC(int playerId, int figureIdOnBoard)
    {
        GameObject _figure = Figures[playerId][figureIdOnBoard];
        StartCoroutine(Death(_figure));
    }

    // figureが相手のフィギュアに包囲されているかを判定する
    public void IsSurrounded(GameObject figure)
    {
        // figureの所有者のID
        int currentId = figure.GetComponent<FigureParameter>().PlayerId;
        // figureに隣接するフィギュア
        GameObject adjacentFigure;

        // figureに隣接するフィギュアが全て相手のフィギュアでなければfalseを返す
        // それ以外であればtrueを返す
        foreach (int adjacentNode in Edges[figure.GetComponent<FigureParameter>().Position])
        {
            adjacentFigure = GetFigureOnBoard(adjacentNode);
            if (null == adjacentFigure)
            {
                return;
            }

            if (currentId == adjacentFigure.GetComponent<FigureParameter>().PlayerId)
            {
                return;
            }
        }
        if(figure.GetComponent<FigureParameter>().Position < 28)
        {
            figure.GetComponent<FigureParameter>().BeSurrounded = true;
        }

    }

    // figureの包囲判定と気絶処理を行う
    public IEnumerator KnockedOutBySurrounding(GameObject figure)
    {
        Debug.Log("包囲するで");
        // フラグ解除
        figure.GetComponent<FigureParameter>().BeSurrounded = false;
        if (figure.GetComponent<FigureParameter>().PlayerId == myPlayerId)
        {
            yield return StartCoroutine(Death(figure));
        }
        else
        {
            int surroundedFigurePlayerId = figure.GetComponent<FigureParameter>().PlayerId;
            int surroundedFigureIdOnBoard = figure.GetComponent<FigureParameter>().FigureIdOnBoard;
            photonView.RPC(CList.DEATH_RPC, RpcTarget.Others, surroundedFigurePlayerId, surroundedFigureIdOnBoard);
        }
        //doneFlagに統合したい
        WaitProcess();
        
        isWaiting = true;
        while (isWaiting == true)
        {
            yield return null;
        }
        SetWaitFlagCustomProperty(true);
        
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
            if (CurrentFigure != null)
            {
                CurrentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;
            }

            CurrentFigure = null;
            SetCurrentFigureCustomProperty();

            OpponentFigure = null;
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
            CurrentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.green;

            FigureParameter currentFigureParameter = CurrentFigure.GetComponent<FigureParameter>();
            // 他のフィギュアを障害物として認識
            bool[] maskWalkNode = CreateNormalMask();
            // 現在地から全ノードへの距離を計算
            var calculateWalkDistance = CalculateDistance(currentFigureParameter.Position, maskWalkNode);
            // この計算の時にDecideRouteに必要なprevNodeを記録
            prevNode = calculateWalkDistance.prevNode;
            // 現在地からmp以内で移動可能な全ノードの色を紫色にする
            if (restTurn == 300)
            {
                walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.Mp - 1, calculateWalkDistance.distances);
            }
            else
            {
                walkCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.Mp, calculateWalkDistance.distances);
            }

            for (int i = 0; i < walkCandidates.Count; i++)
            {
                Nodes.transform.GetChild(walkCandidates[i]).GetComponent<SpriteRenderer>().color = Color.magenta;
            }

            // 攻撃範囲の計算
            bool[] maskAttackNode = new bool[CList.NUMBER_OF_WALK_NODES];
            for (int i = 0; i < maskAttackNode.Length; i++)
            {
                maskAttackNode[i] = true;
            }

            var calculateAttackDistance = CalculateDistance(currentFigureParameter.Position, maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.AttackRange, calculateAttackDistance.distances);
        }
        else if (phaseState == PhaseState.AfterWalk)
        {
            ClearNodesColor();

            //全フィギュア対象に包囲フラグ確認
            for (int i = 0; i < CList.NUMBER_OF_PLAYERS; i++)
            {
                foreach (GameObject figure in Figures[i])
                {
                    IsSurrounded(figure);
                }
            }
            // 包囲フラグあるフィギュアに対して包囲処理
            for (int i = 0; i < CList.NUMBER_OF_PLAYERS; i++)
            {
                foreach (GameObject figure in Figures[i])
                {
                    if (figure.GetComponent<FigureParameter>().BeSurrounded == true)
                    {
                        yield return KnockedOutBySurrounding(figure);
                    }
                }
            }

            // 周囲に敵が誰もいなければターンエンド
            // 実際はこれに加えて眠り、氷、mpマイナスマーカーの味方がいたとき

            FigureParameter currentFigureParameter = CurrentFigure.GetComponent<FigureParameter>();
            bool opponentExistInAttackCandidates = false;   // バトル候補がいるかどうか
            int opponentID = GetTheOtherPlayerId(currentFigureParameter.PlayerId);

            bool[] maskAttackNode = new bool[CList.NUMBER_OF_FIELD_NODES];
            for (int i = 0; i < maskAttackNode.Length; i++)
            {
                maskAttackNode[i] = true;
            }
            var calculateAttackDistance = CalculateDistance(currentFigureParameter.Position, maskAttackNode);
            attackCandidates = FindCandidateofDestinaitonLessThan(currentFigureParameter.AttackRange, calculateAttackDistance.distances);

            for (int i = 0; i < Figures[opponentID].Count; i++)
            {
                foreach (int j in attackCandidates)
                {
                    if (j == Figures[opponentID][i].GetComponent<FigureParameter>().Position)
                    {
                        opponentExistInAttackCandidates = true;
                        Debug.Log("敵が隣におるやん");
                    }

                }
            }
            // 今ゴール処理は通常移動後しか考えてない。ワザの効果で移動するときもな
            if (GetFigureOnBoard(goalNodeId[myPlayerId]) != null &&
               GetFigureOnBoard(goalNodeId[myPlayerId]).GetComponent<FigureParameter>().PlayerId == enemyPlayerId)
            {
                winPlayerId = enemyPlayerId;
                StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }

            else if (GetFigureOnBoard(goalNodeId[enemyPlayerId]) != null &&
                     GetFigureOnBoard(goalNodeId[enemyPlayerId]).GetComponent<FigureParameter>().PlayerId == myPlayerId)
            {
                winPlayerId = myPlayerId;
                StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }
            // 今ゴール処理は通常移動後しか考えてない。ワザの効果で移動するときもな
            /*
            if (currentFigure.GetComponent<FigureParameter>().GetPosition() == goalNodeId[opponentID])
            {
                StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }
            */
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

            //　LocalMasterにバトル準備の指示を出す
            photonView.RPC(CList.PREPARE_BATTLE, RpcTarget.All);

            StartCoroutine(SetPhaseState(PhaseState.Battle));

        }
        else if(phaseState == PhaseState.Battle)
        {
            // ルーレットの抽選を行う
            SetSpinResultCustomProperty(0, UnityEngine.Random.Range(0, 96));
            SetSpinResultCustomProperty(1, UnityEngine.Random.Range(0, 96));

            // 待合
            yield return WaitProcess();

            photonView.RPC(CList.NOW_BATTLE, RpcTarget.All);

            // 両者のスピンが終わるまで待つ
            yield return WaitProcess();
            // 実際はこのあたりをリスピン処理やらで繰り返す

            StartCoroutine(SetPhaseState(PhaseState.BattleEnd));
        }
        else if (phaseState == PhaseState.BattleEnd)
        {
            // バトル終了したことをLocalに通達
            photonView.RPC(CList.END_BATTLE, RpcTarget.All);
            yield return WaitProcess();
           
            StartCoroutine(SetPhaseState(PhaseState.AfterBattle));
        }
        else if (phaseState == PhaseState.AfterBattle)
        {
           

            // バトル後の処理をここに
            // 今はアナログとの統合の都合上2回も出目の取得をしていて無駄
            SpinController spinController = spinMaster.GetComponent<SpinController>();
            MoveParameter[] mp = new MoveParameter[CList.NUMBER_OF_PLAYERS];
            int currentFigureId = CurrentFigure.GetComponent<FigureParameter>().PlayerId;
            int opponentFigureId = OpponentFigure.GetComponent<FigureParameter>().PlayerId;

            mp[0] = spinController.GetMoveParameterFromSpinResult(CurrentFigure, SpinResult[currentFigureId]);
            mp[1] = spinController.GetMoveParameterFromSpinResult(OpponentFigure, SpinResult[opponentFigureId]);

            var BattleResult = spinController.Judge(mp[0], mp[1]);

            int result = BattleResult.Item1;
            bool currentMoveAwake = BattleResult.Item2;
            bool opponentMoveAwake = BattleResult.Item3;
            bool currentDeath = BattleResult.Item4;
            bool opponentDeath = BattleResult.Item5;
            int currentMoveId = BattleResult.Item6;
            int opponentMoveId = BattleResult.Item7;
            if (currentMoveAwake)
            {
                yield return moveList.CallMoveEffect(currentMoveId, CurrentFigure.GetComponent<FigureParameter>().PlayerId, null);
            }
            if (opponentMoveAwake)
            {
                yield return moveList.CallMoveEffect(opponentMoveId, OpponentFigure.GetComponent<FigureParameter>().PlayerId, null);
            }
            if (currentDeath)
            {
                //yield return Death(currentFigure);
                // yield returnしないと順番おかしくなる
                // でもIEnumeratorをRPCで呼ぶとおかしくなる→返り値を受け取らないため
                yield return StartCoroutine(Death(CurrentFigure));
                SetWaitFlagCustomProperty(true);
            }
            if (opponentDeath)
            {
                int opponentFigurePlayerId = OpponentFigure.GetComponent<FigureParameter>().PlayerId;
                int opponentFigureIdOnBoard = OpponentFigure.GetComponent<FigureParameter>().FigureIdOnBoard;
 
                photonView.RPC(CList.DEATH_RPC, RpcTarget.Others, opponentFigurePlayerId, opponentFigureIdOnBoard);

                isWaiting = true;
                while (isWaiting == true)
                {
                    yield return null;
                }
                SetWaitFlagCustomProperty(true);
            }

            // MoveEffectInput or MoveEffectFigureSelected 以外ならターンエンド
            if (phaseState == PhaseState.AfterBattle)
            {
                Debug.Log("死んだ処理終わった");
                StartCoroutine(SetPhaseState(PhaseState.TurnEnd));
            }

        }
        else if (phaseState == PhaseState.MoveEffectInput)
        {
            // 今の所処理なし
        }
        else if (phaseState == PhaseState.MoveEffectFigureSelected)
        {
            // 今の所処理なし
        }
        else if (phaseState == PhaseState.TurnEnd)
        {
            // 本当はここで書きたくない
            if (GetFigureOnBoard(goalNodeId[myPlayerId]) != null &&
                GetFigureOnBoard(goalNodeId[myPlayerId]).GetComponent<FigureParameter>().PlayerId == enemyPlayerId)
            {
                winPlayerId = enemyPlayerId;
                yield return StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }

            else if (GetFigureOnBoard(goalNodeId[enemyPlayerId]) != null &&
                     GetFigureOnBoard(goalNodeId[enemyPlayerId]).GetComponent<FigureParameter>().PlayerId == myPlayerId)
            {
                winPlayerId = myPlayerId;
                yield return StartCoroutine(SetPhaseState(PhaseState.GameEnd));
            }

            else
            {
                // 相手のターンにして残りのターン数を更新
                WhichTurn = (WhichTurn + 1) % 2;
                restTurn--;
                SetTurnCustomProperty();

                CurrentFigure.transform.Find("FigureBack1").GetComponent<SpriteRenderer>().color = Color.white;

                turnEndButton.SetActive(false);
                // Debug.Log("プレイヤー" + turnNumber + "のターンです");
            }


        }
        else if (phaseState == PhaseState.GameEnd)
        {
            // もう少し真面目にかけ
            // 実際はどっかのタイミングで全部のフィギュアの位置調べて相手のゴールにいればなんちゃらみたいな感じかな
            photonView.RPC(CList.GAME_END_RPC, RpcTarget.All, winPlayerId);
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
            forfeitButton.SetActive(false);
            turnEndButton.SetActive(false);
            StartCoroutine(SetPhaseState(PhaseState.AfterGameEnd));
            // exitGameButton.SetActive(true);

            photonView.RPC(CList.FORFEIT_RPC, RpcTarget.Others);
        }

        else if (phaseState == PhaseState.Lock)
        {
            restTurnText.GetComponent<TextMeshProUGUI>().text = restTurn.ToString();
            playerTurnText.GetComponent<TextMeshProUGUI>().color = Color.red * new Color(1, 1, 1, 0);
            playerTurnText.GetComponent<TextMeshProUGUI>().text = "OPPONENT TURN";
            yield return FadeInOut(playerTurnText, 0.5f);
        }
        // 便宜上加えた
        // 大改修までの辛抱です
        else if (phaseState == PhaseState.AfterGameEnd)
        {
            yield return null;
        }
        else
        {
            yield return null;
        }
    }

    public void SetPhaseStateSimple(PhaseState _phaseState)
    {
        Debug.Log("phaseState = " + _phaseState);
        phaseState = _phaseState;
    }

    [PunRPC]
    public void SetPhaseStateSimpleRPC(int _phaseState)
    {
        SetPhaseStateSimple((PhaseState)_phaseState);
    }

    // ゲームの状態変数のゲッター
    public PhaseState GetPhaseState()
    {
        return phaseState;
    }

    // Localに移動させよう
    [PunRPC]
    // ノードの色変更
    public void IlluminateNodeRPC(int node, int color)
    {
        if(color == 1)
        {
            Nodes.transform.GetChild(node).GetComponent<SpriteRenderer>().color = Color.magenta;
        }
        else if (color == 0)
        {
            Nodes.transform.GetChild(node).GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    /****************************************************************/
    /*             　 バトル前後の処理関数       　　               */
    /****************************************************************/
    [PunRPC]
    private void PrepareBattle()
    {
        localController.PrepareBattle();
    }
    [PunRPC]
    private void NowBattle()
    {
        localController.NowBattle();
    }
    [PunRPC]
    private void EndBattle()
    {
        localController.EndBattle();
    }
    // 別のやつ作ったから使わない
    /*
    [PunRPC]
    private void OnBattleStart()
    {
        // ボードのオブジェクトを消す
        ActivateBoardObjects(false);
        // スピンのオブジェクトを出す
        // ActivateSpinObjects(true);

        // アナログ用オブジェクト配置
        ActivateBattleObjects(true);

        //プレイヤー1のみカメラ位置調整
        
        if (myPlayerId == 1)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(0, 1, 0);

        }

    }
    */
    /*
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

    }
    */
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
        roomHash.Add(CList.WHICH_TURN, WhichTurn);
        roomHash.Add(CList.REST_TURN, restTurn);

        Debug.Log("ターン情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    // doneFlagに統一したい
    
    public void SetWaitFlagCustomProperty(bool _flag)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        isWaiting = false;
        roomHash.Add(CList.IS_WAITING, isWaiting);
        Debug.Log("動いて、いいよ......");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }
    public void SetWaitFlag(bool _flag)
    {
        isWaiting = _flag;
    }
    public bool GetWaitFlagCustomProperty()
    {
        return isWaiting;
    }
    
    public void SetDoneFlagCustomProperty(int _playerId, bool _doneFlag)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        DoneFlag[_playerId] = false;
        roomHash.Add(CList.DONE_FLAG[_playerId], _doneFlag);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }
    // currentFigure情報の送信
    public void SetCurrentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // currentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if (CurrentFigure != null)
        {
            roomHash.Add(CList.CURRENT_FIGURE_PLAYER_ID, CurrentFigure.GetComponent<FigureParameter>().PlayerId);
            roomHash.Add(CList.CURRENT_FIGURE_ID_ON_BOARD, CurrentFigure.GetComponent<FigureParameter>().FigureIdOnBoard);
        }
        // currentFigureがnullの場合は各変数に-1を入れて受信時にnullとして受け取る
        else
        {
            roomHash.Add(CList.CURRENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(CList.CURRENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("currentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }

    // opponentFigure情報の送信
    public void SetOpponentFigureCustomProperty()
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // opponentFigureをPlayerIdとFigureIdOnBoardを介して共有
        if (OpponentFigure != null)
        {
            roomHash.Add(CList.OPPONENT_FIGURE_PLAYER_ID, OpponentFigure.GetComponent<FigureParameter>().PlayerId);
            roomHash.Add(CList.OPPONENT_FIGURE_ID_ON_BOARD, OpponentFigure.GetComponent<FigureParameter>().FigureIdOnBoard);
        }
        // opponentFigureがnullの場合は各変数に-1を入れて受信時にnullとして受け取る
        else
        {
            roomHash.Add(CList.OPPONENT_FIGURE_PLAYER_ID, -1);
            roomHash.Add(CList.OPPONENT_FIGURE_ID_ON_BOARD, -1);
        }

        Debug.Log("opponentFigure情報変更送信");
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }
    /*
    // スピンの共有のために、バトルを仕掛ける側が回転の終点を送信
    // ここでいうsenderIdとは2枚のディスクのうちどちらか、という意味
    public void SetGoalAngleCustomProperty(int _senderId, float _goalAngle)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // 回転の終点を共有
        if (_senderId == 0)
        {
            roomHash.Add(CList.GOAL_ANGLE_0, _goalAngle);
        }
        else
        {
            roomHash.Add(CList.GOAL_ANGLE_1, _goalAngle);
        }

        // ここいまいち
        senderIdFromRouletteParent = _senderId;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }
    */
    // アナログコマ用ルーレット抽選結果
    public void SetSpinResultCustomProperty(int _playerId, int _spinResult)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();
        // 回転の終点を共有
        roomHash.Add(CList.SPIN_RESULT[_playerId], _spinResult);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);

    }
    // 各側で技効果を呼んだり呼ばなかったりするための共有
    public void SetBattleResultCustomProperty(bool currentMoveAwake, bool opponentMoveAwake, bool currentDeath, bool oppnentDeath, int currentMoveId, int opponentMoveId)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();

        roomHash.Add(CList.BE_AFFECTED_DEATH, currentDeath);
        roomHash.Add(CList.AFFECT_MOVE_AWAKE, opponentMoveAwake);
        roomHash.Add(CList.AFFECT_DEATH, oppnentDeath);
        roomHash.Add(CList.AFFECT_MOVE_ID, opponentMoveId);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    // phaseStateを変更する
    public void SetPhaseStateCustomProperty(int _phaseState)
    {
        var roomHash = new ExitGames.Client.Photon.Hashtable();

        roomHash.Add(CList.PHASE_STATE, _phaseState);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    // InterestedMofeEffectを共有
    public void SetInterestedMoveEffectCustomProperty(int _playerId, int _moveId)
    {
        InterestedMoveEffect.Item1 = _playerId;
        InterestedMoveEffect.Item2 = _moveId;

        var roomHash = new ExitGames.Client.Photon.Hashtable();

        roomHash.Add(CList.PHASE_STATE, _playerId);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }
    // これとかLocalMasterに書くべきな気がする
    // カスタムプロパティに変更があった場合呼ばれるコールバック関数
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedRoomHash)
    {
        // 変更されたハッシュを受け取る
        // どちらのターンか
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.WHICH_TURN, out value))
            {
                WhichTurn = (int)value;

            }
        }
        // 残りターン数
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.REST_TURN, out value))
            {
                restTurn = (int)value;

                // ウェイトが付いているフィギュアのウェイトを更新
                // ウェイト0になったらウェイトカウンターの描画を終了する
                for (int i = 0; i < 2; i++)
                {
                    List<GameObject> deck = Figures[i];
                    foreach (GameObject figure in deck)
                    {
                        if (figure.GetComponent<FigureParameter>().WaitCount >= 1)
                        {
                            figure.GetComponent<FigureParameter>().DecreaseWaitCount();
                        }
                    }
                }

                if (myPlayerId == WhichTurn)
                {
                    StartCoroutine(SetPhaseState(PhaseState.TurnStart));
                }
                else
                {
                    StartCoroutine(SetPhaseState(PhaseState.Lock));
                }
            }
        }

        // doneFlagに統合したい
        
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.IS_WAITING, out value))
            {
                isWaiting = (bool)value;
            }
        }
        
        //doneFlag
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.DONE_FLAG[0], out value))
            {
                DoneFlag[0] = (bool)value;
            }
        }
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.DONE_FLAG[1], out value))
            {
                DoneFlag[1] = (bool)value;
            }
        }
        // currentFigure
        {
            object value0 = null;
            object value1 = null;
            if (changedRoomHash.TryGetValue(CList.CURRENT_FIGURE_PLAYER_ID, out value0) &&
               changedRoomHash.TryGetValue(CList.CURRENT_FIGURE_ID_ON_BOARD, out value1))
            {
                if ((int)value0 == -1 && (int)value1 == -1)
                {
                    CurrentFigure = null;
                }
                else
                {
                    CurrentFigure = Figures[(int)value0][(int)value1];
                }
                Debug.Log("currentFigureは" + CurrentFigure);
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
            if (changedRoomHash.TryGetValue(CList.OPPONENT_FIGURE_PLAYER_ID, out value0) &&
               changedRoomHash.TryGetValue(CList.OPPONENT_FIGURE_ID_ON_BOARD, out value1))
            {
                if ((int)value0 == -1 && (int)value1 == -1)
                {
                    OpponentFigure = null;
                }
                else
                {
                    OpponentFigure = Figures[(int)value0][(int)value1];
                }
                Debug.Log("opponentFigureは" + OpponentFigure);
            }

        }
        // goalAngleは使わない方向で
        // goalAngle
        // ここらへんもう少しスマートにしたい
        /*
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.GOAL_ANGLE_0, out value))
            {
                RouletteParents[0].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[0].GetComponent<DiskSpin>().SetReceiveFlag(true);
            }

        }
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.GOAL_ANGLE_1, out value))
            {
                RouletteParents[1].GetComponent<DiskSpin>().SetGoalAngle((float)value);
                RouletteParents[1].GetComponent<DiskSpin>().SetReceiveFlag(true);
            }

        }
        */
        // spinResult
        {
            object value = null;
            if(changedRoomHash.TryGetValue(CList.SPIN_RESULT[0], out value))
            {
                SpinResult[0] = (int)value;
                SetDoneFlagCustomProperty(myPlayerId,true);
            }
        }

        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.SPIN_RESULT[1], out value))
            {
                SpinResult[1] = (int)value;
                SetDoneFlagCustomProperty(myPlayerId, true);
            }
        }

        // phaseState
        {
            object value = null;
            if (changedRoomHash.TryGetValue(CList.PHASE_STATE, out value))
            {
                Debug.Log("unchi");
                SetPhaseStateSimple((PhaseState)value);
            }
        }

        // BattleState
        // 処理未記述
        {
            object affectMoveAwake = null;
            object beAffectedMoveAwake = null;
            object affectDeath = null;
            object beAffectedDeath = null;
            object affectMoveId = null;
            object beAffectedMoveId = null;

            if (changedRoomHash.TryGetValue(CList.AFFECT_MOVE_AWAKE, out affectMoveAwake))
            {
                
            }

            if (changedRoomHash.TryGetValue(CList.BE_AFFECTED_MOVE_AWAKE, out beAffectedMoveAwake))
            {
                
            }

            if (changedRoomHash.TryGetValue(CList.AFFECT_DEATH, out affectDeath))
            {
                
            }

            if (changedRoomHash.TryGetValue(CList.BE_AFFECTED_DEATH, out beAffectedDeath))
            {
                
            }

            if (changedRoomHash.TryGetValue(CList.AFFECT_MOVE_ID, out affectMoveId))
            {
                
            }

            if (changedRoomHash.TryGetValue(CList.BE_AFFECTED_MOVE_ID, out beAffectedMoveId))
            {
                
            }
        }
    }

    /****************************************************************/
    /*      UI関係 or オブジェクト表示/非表示の関数                 */
    /****************************************************************/
    // 全部LocalMasterに移すべき
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
        for (int i = 0; i < CList.NUMBER_OF_WALK_NODES; i++)
        {
            Nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = _flag;
        }
        // エッジを表示/非表示にする
        for (int i = 0; i < this.transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LineRenderer>().enabled = _flag;
        }

        // フィギュアを表示/非表示にする
        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject obj in Figures[i])
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

    // アナログに切り替えるのでこれら使わない
    // スピン用の部ジェクトを表示/非表示
    /*
    private void ActivateSpinObjects(bool _flag)
    {
        // スピンの赤い矢
        arrow.SetActive(_flag);

        // データディスクの親要素
        for (int i = 0; i < CList.NUMBER_OF_PLAYERS; i++)
        {
            RouletteParentPlayer[i].SetActive(_flag);
        }

        // バトル結果の文字
        spinText.SetActive(_flag);
    }
    */

    //ノードの色を初期化
    private void ClearNodesColor()
    {
        for (int i = 0; i < CList.NUMBER_OF_FIELD_NODES; i++)
        {
            Nodes.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    [PunRPC]
    private void GameEndRPC(int _winPlayerId)
    {
        gameEndText.SetActive(true);
        if (myPlayerId == _winPlayerId)
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
        forfeitButton.SetActive(false);
        turnEndButton.SetActive(false);
        StartCoroutine(SetPhaseState(PhaseState.AfterGameEnd));
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
        forfeitButton.SetActive(false);
        turnEndButton.SetActive(false);
        StartCoroutine(SetPhaseState(PhaseState.AfterGameEnd));
        // exitGameButton.SetActive(true);
    }

    /****************************************************************/
    /*      　　　　　　　各種アクセサ関数   　　　　               */
    /****************************************************************/


    // ボード上のノード(nodeId)にあるフィギュアオブジェクトを取得する
    // フィギュアが存在しない場合はnullを返す
    public GameObject GetFigureOnBoard(int nodeId)
    {
        for (int playerId = 0; CList.NUMBER_OF_PLAYERS > playerId; playerId++)
        {
            foreach (GameObject figure in Figures[playerId])
            {
                if (nodeId == figure.GetComponent<FigureParameter>().Position)
                {
                    return figure;
                }
            }
        }

        return null;
    }

    // figuresからFigureを取得
    public GameObject GetFigureFromFigures(int _playerId, int _figureIdOnBoard)
    {
        return Figures[_playerId][_figureIdOnBoard];
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
        forfeitButton.SetActive(false);
        turnEndButton.SetActive(false);
        StartCoroutine(SetPhaseState(PhaseState.AfterGameEnd));
    }
    /****************************************************************/
    /*      　　　　　　　　　　その他　　   　　　　               */
    /****************************************************************/

    /*
    // バトル終了時の同期用(こういう処理をもう少しスマートに書きたい)
    [PunRPC] private void SendFlagToSpinController()
    {
        spinMaster.GetComponent<SpinController>().SetReceiveFlag(true);
    }
    */
    [PunRPC] private void SetWaitCounterRPC(int _playerId, int _figureIdOnBoard, int _waitCount)
    {
        Figures[_playerId][_figureIdOnBoard].GetComponent<FigureParameter>().SetWaitCount(_waitCount);
    }

    private IEnumerator WaitProcess()
    {
        while (DoneFlag[0] == false || DoneFlag[1] == false)
        {
            yield return null;
        }
        SetDoneFlagCustomProperty(0, false);
        SetDoneFlagCustomProperty(1, false);
    }

}
