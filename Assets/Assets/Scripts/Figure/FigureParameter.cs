using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureParameter : MonoBehaviour
{
    [SerializeField] private int mp;
    [SerializeField] private int position;
    private int figureIDOnBoard;
    [SerializeField] private GameObject data;
    [SerializeField] private int playerID;
    private bool beSelected = false;

    public int GetMp()
    {
        return mp;
    }

    public void SetPosition(int _position)
    {
        position = _position;
    }
    public int GetPosition()
    {
        return position;
    }
    public void SetFigureIDOnBoard(int _figureIDOnBoard)
    {
        figureIDOnBoard = _figureIDOnBoard;
    }
    public int GetFigureIDOnBoard()
    {
        return figureIDOnBoard;
    }
    public bool GetBeSelected()
    {
        return beSelected;
    }
    public void SetBeSelected(bool _beSelcted)
    {
        beSelected = _beSelcted;
    }
    public int GetPlayerID()
    {
        return playerID;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
