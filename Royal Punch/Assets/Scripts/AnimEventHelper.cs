using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimEventHelper : MonoBehaviour
{
    public UnityEvent<int> MyEvent;
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (MyEvent == null)
        {
            MyEvent = new UnityEvent<int>();
        }
    }

    public void Event(int i)
    {
        MyEvent.Invoke(i);
    }
}
