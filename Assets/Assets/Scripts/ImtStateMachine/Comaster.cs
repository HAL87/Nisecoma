using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

public class Comaster : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    private enum StateEventId
    {
        DuelStartFirst,
        DuelStartSecond,
        Ready,
        UsePlate,
        CancelPlate,
        SelectFigure,
        CancelFigure,
        SelectAnotherFigure,
        SelectPlateTarget,
        SelectNode,
        StopWalking,
        SelectBattleTarget,
        EndBattle,
        TurnEnd,
        
    }

    // ステートマシン
    private ImtStateMachine<Comaster> stateMachine;
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築
        stateMachine = new ImtStateMachine<Comaster>(this);
        stateMachine.AddTransition<StandbyState, TurnStartState>((int)StateEventId.DuelStartFirst);
        stateMachine.AddTransition<StandbyState, EnemyTurnState>((int)StateEventId.DuelStartSecond);

        // 起動時はStandbyState
        stateMachine.SetStartState<StandbyState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.Update();
    }

    // 非デュエル状態
    private class StandbyState : ImtStateMachine<Comaster>.State
    {

    }

    // ターン開始状態
    private class TurnStartState : ImtStateMachine<Comaster>.State
    {

    }

    // ノーマル状態
    private class NormalState : ImtStateMachine<Comaster>.State
    {

    }

    // プレート使用時状態
    private class UsingPlateState : ImtStateMachine<Comaster>.State
    {

    }

    // フィギュア選択中状態
    private class FigureSelectedState : ImtStateMachine<Comaster>.State
    {

    }

    // フィギュア移動中状態
    private class WalkingState : ImtStateMachine<Comaster>.State
    {

    }

    // フィギュア移動後状態
    private class AfterWalkState : ImtStateMachine<Comaster>.State
    {

    }

    // フィギュア選択中(フィギュア移動後)状態
    private class FigureSelectedAfterWalkState : ImtStateMachine<Comaster>.State
    {

    }

    // バトル中状態
    private class BattleState : ImtStateMachine<Comaster>.State
    {

    }

    // バトル後状態
    private class AfterBattleState : ImtStateMachine<Comaster>.State
    {

    }

    // 相手ターン中状態(Enemyなの？Opponentなの？)
    private class EnemyTurnState : ImtStateMachine<Comaster>.State
    {

    }
}
