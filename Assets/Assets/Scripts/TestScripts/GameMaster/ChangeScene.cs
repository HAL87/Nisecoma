using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //シーンをまたいでもGameMasterは消えない
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //GameSceneからSpinSceneへ
    public void FromGameToSpin()
    {      
        SceneManager.LoadScene("SpinScene");
    }
    //SpinSceneからGameSceneへ
    public void FromSpinToGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
