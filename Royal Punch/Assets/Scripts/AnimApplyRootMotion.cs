using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimApplyRootMotion : MonoBehaviour
{
    [SerializeField] private List<string> _animations;
    private List<int> _animHashes = new List<int>();
    private Animator _animator;
    private AnimatorClipInfo[] _animClipInfo;

    void Start()
    {
        _animator = GetComponent<Animator>();
        foreach (string anim in _animations)
        {
            _animHashes.Add(Animator.StringToHash(anim));
        }
    }

    void OnAnimatorMove()
    {
        _animClipInfo = _animator.GetCurrentAnimatorClipInfo(0);
        foreach (int animHash in _animHashes)
        {
            if (Animator.StringToHash(_animClipInfo[0].clip.name) == animHash)
            {
                _animator.ApplyBuiltinRootMotion();
            }
        }
        
    }
}
