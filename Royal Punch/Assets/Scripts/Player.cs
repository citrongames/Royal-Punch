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

    protected override void Init()
    {
        base.Init();
        _dirXHash = Animator.StringToHash("DirX");
        _dirYHash = Animator.StringToHash("DirY");
        _animPunching = Animator.StringToHash("Armature|GGPunch");
    }
}
