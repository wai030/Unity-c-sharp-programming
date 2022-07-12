using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ai))]
public class wall : MonoBehaviour
{
    [SerializeField] Transform SelfTr;
    [SerializeField] ai AI;
    [SerializeField] float distance;
    [SerializeField] LayerMask Layer;
    RaycastHit hit;
    private void Update()
    {
        if (Physics2D.Raycast(SelfTr.position, new Vector2(AI.direction, 0), distance, Layer))
            AI.wallcrashed = true;
        else
            AI.wallcrashed = false;
    }
}
