using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiController : MonoBehaviour
{
    // ボードのUI
    [SerializeField] private GameObject startText;
    [SerializeField] private List<GameObject> playerTurnText;
    [SerializeField] private GameObject restTurnText;
    [SerializeField] private GameObject gameEndText;
    [SerializeField] private GameObject turnEndButton;
    [SerializeField] private GameObject forfeitButton;

    //スピンのUI
    [SerializeField] private GameObject arrow;
    [SerializeField] private List<GameObject> RouletteParentPlayer;
    [SerializeField] private GameObject spinText;
    /*
    public void ActivateRestTurnButton(bool _flag)
    {
        restTurnText.GetComponent<TextMeshProUGUI>().enabled = _flag;
    }
    public void UpdateRestTurnText(int _restTurn)
    {
        restTurnText.GetComponent<TextMeshProUGUI>().text = _restTurn.ToString();
    }

    public void ActivateTurnEndButton(bool _flag)
    {
        turnEndButton.SetActive(_flag);
    }

    public void ActivateForfeitButton(bool _flag)
    {
        forfeitButton.SetActive(_flag);
    }

    public void OnGoal(GameObject _currentFigure)
    {
        gameEndText.SetActive(true);
        gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + _currentFigure.GetComponent<FigureParameter>().GetPlayerId() + " WIN!";
    }
    public void OnForfeit(int _turnNumber)
    {
        gameEndText.SetActive(true);
        gameEndText.GetComponent<TextMeshProUGUI>().text = "PLAYER" + (_turnNumber + 1) % 2 + " WIN!";
    }


    public void ActivateArrow(bool _flag)
    {
        arrow.SetActive(_flag);
    }

    public void ActivateRoulette
    */
}
