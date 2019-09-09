using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Vector3 target = new Vector3(0, 0, 10);
    //private Vector3 target2 = new Vector3(10, 0, 10);
    // Start is called before the first frame update
    IEnumerator Start()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(Movement(target));
            target *= -1;
        }

        //yield return StartCoroutine(Movement(target));

        //yield return StartCoroutine(Movement(target2));
    }

    // Update is called once per frame
    IEnumerator Movement(Vector3 _target)
    {
        while (Vector3.Distance(transform.position, _target) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, _target, 3 * Time.deltaTime);
            //Debug.Log(transform.position);
            yield return null;
            //Debug.Log(target);
        }
    }
}

