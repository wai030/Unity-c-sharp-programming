using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPScal : MonoBehaviour
{
    [SerializeField] internal int CPS, curTime, spam, unspamC;
    // Start is called before the first frame update
    void Start()
    {
        curTime = 0;
        CPS = 0;
        spam = 0;
        unspamC = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1")){
            CPS += 1;
        }
        if (Time.time - curTime >= 1)
        {
            if (CPS >= 4&& spam <= 10)
            {
                spam += 1;
            }
            CPS = 0;
            curTime = Mathf.RoundToInt(Time.time);
        }
    }
}
