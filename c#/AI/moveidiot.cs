using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveidiot : MonoBehaviour
{
    [SerializeField]Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {

        rb.velocity = new Vector2(-1 * Time.fixedDeltaTime, -9.81f);
    }
}
