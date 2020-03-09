using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

public class SceneController : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    private enum StateEventId
    {
        Login,
        Logout,
        DuelStart,
        DuelEnd,
        Reset,
    }

    // ステートマシン
    private ImtStateMachine<SceneController> stateMachine;

    // コンポーネントの初期化
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築
        stateMachine = new ImtStateMachine<SceneController>(this);
        stateMachine.AddTransition<LauncherState, LobbyState>((int)StateEventId.Login);
        stateMachine.AddTransition<LobbyState, LauncherState>((int)StateEventId.Logout);
        stateMachine.AddTransition<LobbyState, DuelState>((int)StateEventId.DuelStart);
        stateMachine.AddTransition<DuelState, ResultState>((int)StateEventId.DuelEnd);
        stateMachine.AddTransition<ResultState, LobbyState>((int)StateEventId.Reset);

        // 起動時はLauncherState
        stateMachine.SetStartState<LauncherState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.Update();
    }

    // ログイン画面状態
    private class LauncherState : ImtStateMachine<SceneController>.State
    {

    }

    // ロビー画面状態
    private class LobbyState : ImtStateMachine<SceneController>.State
    {

    }

    // デュエル中状態
    private class DuelState : ImtStateMachine<SceneController>.State
    {

    }

    // リザルト画面状態
    private class ResultState : ImtStateMachine<SceneController>.State
    {

    }
}
