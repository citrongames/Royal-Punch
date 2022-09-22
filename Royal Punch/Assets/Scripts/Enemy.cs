using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject _lockTarget;
    [SerializeField] private float _turnSpeed;
    private Animator _animator;
    private int _animIsTurning;
    private bool _isRotating = false;
    private const float MIN_ROT = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _animIsTurning = Animator.StringToHash("IsTurning");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_lockTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_lockTarget.transform.position - transform.position);
            if (Quaternion.Angle(transform.rotation, targetRotation) > MIN_ROT && !_isRotating)
            {
                _isRotating = true;
                _animator.SetBool(_animIsTurning, true);
            }
            else if (Quaternion.Angle(transform.rotation, targetRotation) < MIN_ROT && _isRotating)
            {
                _isRotating = false;
                _animator.SetBool(_animIsTurning, false);
            }
            if (_isRotating)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
