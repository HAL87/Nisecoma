using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinControllerDebug : MonoBehaviour
{
    private RouletteMaker ruletteMaker0;
    private RouletteMaker ruletteMaker1;
    private DiskSpin diskSpin0;
    private DiskSpin diskSpin1;
    private GetInfo getInfo;
    [SerializeField] private List<GameObject> datadisks;
    // Start is called before the first frame update
    void Start()
    {
        //今とりあえずPlayer1だけでやってる
        //
        ruletteMaker0 = datadisks[0].GetComponent<RouletteMaker>();
        ruletteMaker1 = datadisks[1].GetComponent<RouletteMaker>();



        getInfo = GameObject.Find("GameMaster").GetComponent<GetInfo>();
        //Debug.Log(getInfo.GetData(0));
        //ルーレット作成メソッドの呼び出し
        ruletteMaker0.CreateRulette(getInfo.GetData(0));
        diskSpin0 = datadisks[0].GetComponent<DiskSpin>();
        diskSpin1 = datadisks[1].GetComponent<DiskSpin>();
    }

    public void SpinStart()
    {
        
        StartCoroutine(diskSpin0.Spin(getInfo.GetData(0)));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SpinPrepare()
    {
        ruletteMaker0.CreateRulette(getInfo.GetData(0));
    }

}
