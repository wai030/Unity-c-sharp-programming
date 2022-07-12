using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kat3 : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<take_damage>().takedamage(20);
        }
    }
}
