using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Testing : MonoBehaviour
{
    float previousValue = 0;
    float value = 0;
    public GameObject obj;
    public Slider sl;

    public void GetSliderValue()
    {
        value = sl.value;
        float delta = value - previousValue;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x+delta, obj.transform.localScale.y, obj.transform.localScale.z);
        previousValue = value;
    }
}
