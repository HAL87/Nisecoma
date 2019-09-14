using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeGameBlind : MonoBehaviour
{
    [SerializeField] GameObject DontDestroyObject;
    [SerializeField] GameObject BeforeStart;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartButtonClick()
    {
        GetComponent<Animator>().enabled = true;
    }
    public void EndFadein()
    {
        DontDestroyObject.SetActive(true);
        Destroy(BeforeStart);
    }
}
