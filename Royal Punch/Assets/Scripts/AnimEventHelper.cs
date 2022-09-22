using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AnimEventHelper : MonoBehaviour
{
    public UnityAction<int> MyEvent;

    public void Event(int i)
    {
        MyEvent.Invoke(i);
    }
}
