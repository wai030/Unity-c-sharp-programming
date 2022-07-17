using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class healthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    public void setbar(float i)
    {
        slider.value = i;
    }
}
