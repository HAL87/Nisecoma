using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// これも捨てる
public class RouletteParentParameter : MonoBehaviour
{
    [SerializeField] private int playerId = -1;

    public int GetPlayerId()
    {
        return playerId;
    }
}
