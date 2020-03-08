using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

public class Figure : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    private enum StateEventId
    {
        Enable,
        Disable,
    }

    // ステートマシン
    private ImtStateMachine<Figure> stateMachine;

    // コンポーネントの初期化
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築
        stateMachine = new ImtStateMachine<Figure>(this);
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

    // FigureステートマシンをEnabledStateに遷移
    public void EnableFigure()
    {
        stateMachine.SendEvent((int)StateEventId.Enable);
    }

    // FigureステートマシンをDisabledStateに遷移
    public void DisableFigure()
    {
        stateMachine.SendEvent((int)StateEventId.Disable);
    }

    // 有効状態
    private class EnabledState : ImtStateMachine<Figure>.State
    {
        // Figureがクリックされたときに呼ばれる関数
        protected int GetClickedFigureData()
        {
            Utility.LogCurrentMethod("【IN】");
            int ret = 0;
            Utility.LogCurrentMethod("【OUT】return = " + ret);
            return 0;
        }
    }

    // 無効状態
    private class DisabledState : ImtStateMachine<Figure>.State
    {
        // 何もできない
    }

}
