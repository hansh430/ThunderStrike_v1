using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static ButtonInputHandler instance;
    public bool shoot, jump;
    public string btnName;
    private void Awake()
    {
        instance = this; 
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (btnName == "Shoot")
        {
            shoot = false;
        }
        else if (btnName == "Jump")
        {
            jump = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(btnName=="Shoot")
        {
            shoot = true;
        }
        else if(btnName == "Jump")
        {
            jump = true;
        }
    }


}
