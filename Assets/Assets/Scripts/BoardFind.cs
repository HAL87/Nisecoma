using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardFind : MonoBehaviour
{
    //private int[] nodes = new int[28];
    private List<int>[] edges = new List<int>[28];
    private int[] distances = new int[28];
    private int[] prevNode = new int[28];

    private List<int> candidates = new List<int>();
    Stack<int> route = new Stack<int>();

    [SerializeField] private int startNode;
    [SerializeField] private int mp;
    [SerializeField] private int goalNode;
    // Start is called before the first frame update
    void Start()
    {
        CreateBoard();
        CaliculateDistance(startNode);
        //DecideRoute(startNode, goalNode);
        //FindCandidateofDestinaitonEqual(mp);
        FindCandidateofDestinaitonLessThan(mp);
    }
    void CreateBoard()
    {
        for (int i = 0; i < 28; i++)
        {
            //nodes[i] = i;
            edges[i] = new List<int>();
        }
        edges[0].Add(1); edges[0].Add(7); edges[0].Add(8);
        edges[1].Add(0); edges[1].Add(2);
        edges[2].Add(1); edges[2].Add(3); edges[2].Add(9);
        edges[3].Add(2); edges[3].Add(4);
        edges[4].Add(3); edges[4].Add(3);
        edges[5].Add(4); edges[5].Add(6);
        edges[6].Add(5); edges[6].Add(10); edges[6].Add(11);
        edges[7].Add(0); edges[7].Add(12);
        edges[8].Add(0); edges[8].Add(9); edges[8].Add(13);
        edges[9].Add(2); edges[9].Add(8); edges[9].Add(10);
        edges[10].Add(6); edges[10].Add(9); edges[10].Add(14);
        edges[11].Add(6); edges[11].Add(15);
        edges[12].Add(7); edges[12].Add(16);
        edges[13].Add(8); edges[13].Add(17);
        edges[14].Add(10); edges[14].Add(19);
        edges[15].Add(11); edges[15].Add(20);
        edges[16].Add(12); edges[16].Add(21);
        edges[17].Add(13); edges[17].Add(18); edges[17].Add(21);
        edges[18].Add(17); edges[18].Add(19); edges[18].Add(25);
        edges[19].Add(14); edges[19].Add(18); edges[19].Add(27);
        edges[20].Add(15); edges[20].Add(27);
        edges[21].Add(16); edges[21].Add(17); edges[21].Add(22);
        edges[22].Add(21); edges[22].Add(23);
        edges[23].Add(22); edges[23].Add(24);
        edges[24].Add(23); edges[24].Add(25);
        edges[25].Add(18); edges[25].Add(24); edges[25].Add(26);
        edges[26].Add(25); edges[26].Add(27);
        edges[27].Add(19); edges[27].Add(20); edges[27].Add(26);
    }

    void CaliculateDistance(int startNode)
    {
        Queue<int> q = new Queue<int>();
        bool[] reachedFlag = new bool[28];
        bool exitFlag = false;
        for (int i = 0; i < 28; i++)
        {
            distances[i] = 99;
            reachedFlag[i] = false;
        }
        q.Enqueue(startNode);
        reachedFlag[startNode] = true;
        distances[startNode] = 0;
        while (true)
        {
            exitFlag = true;
            int currentNode = q.Dequeue();
            //Debug.Log(currentNode);
            for(int i = 0; i < edges[currentNode].Count; i++)
            {
                int nextNode = edges[currentNode][i];
                if(reachedFlag[nextNode] == false)
                {
                    q.Enqueue(nextNode);
                    reachedFlag[nextNode] = true;
                    distances[nextNode] = distances[currentNode] + 1;
                    prevNode[nextNode] = currentNode;
                }

            }
            for(int i = 0; i < 28; i++)
            {
                if(reachedFlag[i] == false)
                {
                    exitFlag = false;
                }
            }
            //Debug.Log(exitFlag);
            if (exitFlag == true)
            {
                break;
            }
        }
        /*
        for(int i = 0; i < 28; i ++)
        {
            Debug.Log("ノード" + i + ":"+ distances[i]);
        }
        */
    }

    void FindCandidateofDestinaitonEqual(int mp)
    {
        candidates.Clear();
        for (int i = 0; i < 28; i++)
        {
            if (distances[i] == mp)
            {
                candidates.Add(i);
            }
        }
        for(int i = 0; i < candidates.Count; i++)
        {
            Debug.Log(candidates[i]);
        }
    }
    void FindCandidateofDestinaitonLessThan(int mp)
    {
        candidates.Clear();
        for (int i = 0; i < 28; i++)
        {
            if (distances[i] <= mp)
            {
                candidates.Add(i);
            }
        }
        for (int i = 0; i < candidates.Count; i++)
        {
            Debug.Log(candidates[i]);
        }
    }

    void DecideRoute(int startNode, int goalNode)
    {
        int currentNode = goalNode;
        while (true)
        {
            route.Push(currentNode);
            currentNode = prevNode[currentNode];
            if(currentNode == startNode)
            {
                route.Push(startNode);
                break;
            }
        }
        while(route.Count > 0)
        {
            Debug.Log(route.Pop());
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
