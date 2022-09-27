using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AnimEventHelper : MonoBehaviour
{
    public UnityAction<int> MyEvent;
    public UnityAction MyEvent2;

    public void Event(int i)
    {
        MyEvent.Invoke(i);
    }

    public void Event2()
    {
        MyEvent2.Invoke();
    }
}
