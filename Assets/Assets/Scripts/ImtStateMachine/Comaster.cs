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
        StartMyTurn,
        UsePlate,
        CancelPlate,
        UseAbility,
        CancelAbility,
        SelectFigure,
        CancelFigure,
        SelectAnotherFigure,
        SelectPlateTarget,
        SelectNode,
        StopWalking,
        SelectBattleTarget,
        EndBattle,
        TurnEnd,
        StartEnemyTurn,
        EndEnemyTurn,
        GameEnd,
    }

    // ステートマシン
    private ImtStateMachine<Comaster> stateMachine;
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築
        stateMachine = new ImtStateMachine<Comaster>(this);
        stateMachine.AddTransition<StandbyState, TurnStartState>((int)StateEventId.DuelStartFirst); // 先攻
        stateMachine.AddTransition<StandbyState, EnemyTurnState>((int)StateEventId.DuelStartSecond); // 後攻
        stateMachine.AddTransition<TurnStartState, NormalState>((int)StateEventId.StartMyTurn);
        stateMachine.AddTransition<NormalState, UsingPlateState>((int)StateEventId.UsePlate);
        stateMachine.AddTransition<UsingPlateState, NormalState>((int)StateEventId.CancelPlate);
        stateMachine.AddTransition<NormalState, FigureSelectedState>((int)StateEventId.SelectFigure);
        stateMachine.AddTransition<FigureSelectedState, NormalState>((int)StateEventId.CancelFigure);
        stateMachine.AddTransition<FigureSelectedState, FigureSelectedState>((int)StateEventId.SelectAnotherFigure);
        stateMachine.AddTransition<NormalState, UsingAbilityState>((int)StateEventId.UseAbility);
        stateMachine.AddTransition<UsingAbilityState, NormalState>((int)StateEventId.CancelAbility);
        stateMachine.AddTransition<UsingPlateState, AfterWalkState>((int)StateEventId.SelectPlateTarget);
        stateMachine.AddTransition<FigureSelectedState, WalkingState>((int)StateEventId.SelectNode);
        stateMachine.AddTransition<WalkingState, AfterWalkState>((int)StateEventId.StopWalking);
        stateMachine.AddTransition<FigureSelectedState, BattleState>((int)StateEventId.SelectBattleTarget);
        stateMachine.AddTransition<AfterWalkState, BattleState>((int)StateEventId.SelectBattleTarget);
        stateMachine.AddTransition<AfterWalkState, FigureSelectedAfterWalkState>((int)StateEventId.SelectFigure);
        stateMachine.AddTransition<FigureSelectedAfterWalkState, AfterWalkState>((int)StateEventId.CancelFigure);
        stateMachine.AddTransition<BattleState, AfterBattleState>((int)StateEventId.EndBattle);
        stateMachine.AddTransition<UsingPlateState, TurnEndState>((int)StateEventId.TurnEnd);
        stateMachine.AddTransition<UsingAbilityState, TurnEndState>((int)StateEventId.TurnEnd);
        stateMachine.AddTransition<AfterWalkState, TurnEndState>((int)StateEventId.TurnEnd);
        stateMachine.AddTransition<FigureSelectedAfterWalkState, TurnEndState>((int)StateEventId.TurnEnd);
        stateMachine.AddTransition<AfterBattleState, TurnEndState>((int)StateEventId.TurnEnd);
        stateMachine.AddTransition<TurnEndState, EnemyTurnState>((int)StateEventId.StartEnemyTurn);
        stateMachine.AddTransition<EnemyTurnState, TurnStartState>((int)StateEventId.EndEnemyTurn);
        stateMachine.AddTransition<UsingPlateState, GameEndState>((int)StateEventId.GameEnd);
        stateMachine.AddTransition<UsingAbilityState, GameEndState>((int)StateEventId.GameEnd);
        stateMachine.AddTransition<AfterWalkState, GameEndState>((int)StateEventId.GameEnd);
        stateMachine.AddTransition<AfterBattleState, GameEndState>((int)StateEventId.GameEnd);
        stateMachine.AddTransition<TurnEndState, GameEndState>((int)StateEventId.GameEnd);
        stateMachine.AddTransition<EnemyTurnState, GameEndState>((int)StateEventId.GameEnd);

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

    // デュエル決着状態
    private class GameEndState : ImtStateMachine<Comaster>.State
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

    // 特性使用時状態
    private class UsingAbilityState : ImtStateMachine<Comaster>.State
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

    // ターンエンド状態
    private class TurnEndState : ImtStateMachine<Comaster>.State
    {

    }

    // 相手ターン中状態(Enemyなの？Opponentなの？)
    private class EnemyTurnState : ImtStateMachine<Comaster>.State
    {

    }
}
