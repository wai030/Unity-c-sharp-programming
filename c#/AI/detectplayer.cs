using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ai))]
public class detectplayer : MonoBehaviour
{
    [SerializeField] ai AI;
    [SerializeField] Transform PlayTr, SelfTr;
    [SerializeField] float Range;
    internal bool inRange {
        get{
            return _inRange;
        }
        set{
            _inRange = value;
            AI.inrange = value;
        }
        }

    bool _inRange;

    private void Update()
    {
        if (Mathf.Abs(PlayTr.position.x - SelfTr.position.x) < Range)
            inRange = true;
        else
            inRange = false;
    }
}
