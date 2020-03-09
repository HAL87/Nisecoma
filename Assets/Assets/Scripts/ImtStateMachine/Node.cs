using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

public class Node : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    private enum StateEventId
    {
        Enable,
        Disable,
    }

    // ステートマシン
    private ImtStateMachine<Node> stateMachine;

    // コンポーネントの初期化
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築
        stateMachine = new ImtStateMachine<Node>(this);
        stateMachine.AddTransition<EnabledState, DisabledState>((int)StateEventId.Disable);
        stateMachine.AddTransition<DisabledState, EnabledState>((int)StateEventId.Enable);

        // 起動時はDisabledState
        stateMachine.SetStartState<DisabledState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.Update();
    }

    // NodeステートマシンをEnabledStateに遷移
    public void EnableNode()
    {
        stateMachine.SendEvent((int)StateEventId.Enable);
    }

    // NodeステートマシンをDisabledStateに遷移
    public void DisableNode()
    {
        stateMachine.SendEvent((int)StateEventId.Disable);
    }

    // 有効状態
    private class EnabledState : ImtStateMachine<Node>.State
    {
        // Nodeがクリックされたときに呼ばれる関数
        protected int GetClickedNodeData()
        {
            Utility.LogCurrentMethod("【IN】");
            int ret = 0;
            Utility.LogCurrentMethod("【OUT】return = " + ret);
            return 0;
        }
    }

    // 無効状態
    private class DisabledState : ImtStateMachine<Node>.State
    {
        // 何もできない
    }
}
