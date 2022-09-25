using UnityEngine;

public class Enemy : Boxer
{
    //Internal Unity Functions
    void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        UpdateFixed();
    }

    void Update()
    {
        UpdateNormal();
    }

    void OnCollisionEnter(Collision other)
    {
        CollisionEnter(other);
    }

    void OnCollisionExit(Collision other)
    {
        CollisionExit(other);
    }

    //Custom override
    protected override void Init()
    {
        base.Init();
        _animPunching = Animator.StringToHash("Armature|BossPunch");
    }
}
