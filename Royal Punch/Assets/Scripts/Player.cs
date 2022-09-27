using UnityEngine;

public class Player : Boxer
{
    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateNormal();
    }

    void FixedUpdate()
    {
        UpdateFixed();
    }

    private void OnCollisionEnter(Collision other) 
    {
        CollisionEnter(other);
    }

    private void OnCollisionExit(Collision other) 
    {
        CollisionExit(other);
    }
}
