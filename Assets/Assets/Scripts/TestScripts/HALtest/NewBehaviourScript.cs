using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Vector3 target = new Vector3(0, 5, 0);
    private Vector3 target2 = new Vector3(-2, 0, 0);
    // Start is called before the first frame update
    void Start()
    {
        //yield return StartCoroutine(Movement(target));

        //yield return StartCoroutine(Movement(target2));
    }
    private void Update()
    {
        //Debug.Log("アイカツ");
        //transform.position = transform.position + new Vector3(1, 0, 0) * Time.deltaTime;
    }
    public void Touch()
    {
        Debug.Log("touch");
        //StartCoroutine(TouchControll2());
        StartCoroutine(TouchControll3());
        //Debug.Log("一つ目の移動終了");
        //yield return StartCoroutine(Movement(target2));

    }

    public IEnumerator TouchControll1()
    {
        Debug.Log("あ");
        var coroutine1 = Movement(target);
        var coroutine2 = Movement(target2);

        Debug.Log("い");
        yield return coroutine1;
        Debug.Log("う");
        yield return coroutine2;
        Debug.Log("え");
        //yield return StartCoroutine(Movement(target));
        //Debug.Log("一つ目の移動終わり");
        //yield return StartCoroutine(Movement(target2));
        //Debug.Log("二つ目の移動終わり");
    }
    public IEnumerator TouchControll2()
    {
        var ie1 = Movement(target);
        var ie2 = Movement(target2);
        var coroutine1 = StartCoroutine(ie1);
        Debug.Log("け");
        var coroutine2 = StartCoroutine(ie2);
        Debug.Log("こ");
        yield return coroutine1;
        Debug.Log("さ");
        yield return coroutine2;
        Debug.Log("し");
    }
    public IEnumerator TouchControll3()
    {
        var ie1 = Movement(target);
        var ie2 = Movement(target2);
        var coroutine1 = StartCoroutine(ie1);
        yield return coroutine1;
        Debug.Log("け");
        var coroutine2 = StartCoroutine(ie2);
        Debug.Log("こ");
        yield return coroutine1;
        Debug.Log("さ");
        yield return coroutine2;
    }
    IEnumerator Movement(Vector3 _target)
    {
        Debug.Log(_target);
        while (Vector3.Distance(transform.position, _target) > 0.05f)
        {
            transform.position += (_target - transform.position).normalized * 2 * Time.deltaTime; ;
            //Debug.Log(transform.position);
            yield return null;
        }

    }
}

