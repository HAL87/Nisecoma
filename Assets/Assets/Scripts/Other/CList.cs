using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CList
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
    public readonly static string ROOM_CREATOR = "RoomCreator";
    public readonly static string DAMMY = "dammy";
    public readonly static string WHICH_TURN = "whichTurn";
    public readonly static string REST_TURN = "restTurn";

    public readonly static string CURRENT_FIGURE_PLAYER_ID = "currentFigurePlayerId";
    public readonly static string CURRENT_FIGURE_ID_ON_BOARD = "currentFigureIdOnBoard";

    public readonly static string OPPONENT_FIGURE_PLAYER_ID = "opponentFigurePlayerId";
    public readonly static string OPPONENT_FIGURE_ID_ON_BOARD = "opponentFigureIdOnBoard";


    public readonly static string GOAL_ANGLE_0 = "goalAngle0";
    public readonly static string GOAL_ANGLE_1 = "goalAngle1";
    public readonly static string[] SPIN_RESULT = { "spinResult0", "spinResult1" };
    
    public readonly static string[] DONE_FLAG = { "doneFlag0", "doneFlag1" };
    public readonly static string IS_WAITING = "isWaiting";
    public readonly static string AFFECT_MOVE_AWAKE = "affectMoveAwake";
    public readonly static string BE_AFFECTED_MOVE_AWAKE = "beAffectedMoveAwake";
    public readonly static string AFFECT_DEATH = "affectDeath";
    public readonly static string BE_AFFECTED_DEATH = "beAffectedDeath";
    public readonly static string AFFECT_MOVE_ID = "affectMoveId";
    public readonly static string BE_AFFECTED_MOVE_ID = "beAffectedMoveId";

    public readonly static string PHASE_STATE = "phaseState";
    // RPC用関数名
    public readonly static string PREPARE_BATTLE = "PrepareBattle";
    public readonly static string NOW_BATTLE = "NowBattle";
    public readonly static string END_BATTLE = "EndBattle";
    public readonly static string ON_BATTLE_START = "OnBattleStart";
    public readonly static string ON_BATTLE_END = "OnBattleEnd";
    public readonly static string DEATH_RPC = "DeathRPC";
    public readonly static string SEND_FLAG_TO_SPIN_CONTROLLER = "SendFlagToSpinController";
    public readonly static string FIGURE_ONE_STEP_WALK = "FigureOneStepWalk";
    public readonly static string GAME_END_RPC = "GameEndRPC";
    public readonly static string FORFEIT_RPC = "ForfeitRPC";
    public readonly static string SET_WAIT_COUNTER_RPC = "SetWaitCounterRPC";
    public readonly static string FIGURE_ONE_STEP_WALK_RPC = "FigureOneStepWalkRPC";
    public readonly static string SET_PHASE_STATE_SIMPLE_RPC = "SetPhaseStateSimpleRPC";
    public readonly static string ILLUMINATE_NODE_RPC = "IlluminateNodeRPC";
}
