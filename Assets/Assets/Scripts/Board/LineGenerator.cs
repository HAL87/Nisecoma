using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{
    LineRenderer line;
    [SerializeField] private Transform squareTransform;
    // Start is called before the first frame update
    void Start()
    {

        //コンポーネントを取得する
        this.line = GetComponent<LineRenderer>();

        //線の幅を決める
        this.line.startWidth = 0.03f;
        this.line.endWidth = 0.03f;

        //頂点の数を決める
        this.line.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        //0や1は頂点の順番(多分)
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, new Vector3(1f, 1f, 0f));
    }
}