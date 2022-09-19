using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private GameObject _lockTarget;
    private bool _isMoving = false;
    private Vector3 _movePosition;
    private Rigidbody _ridigbody;
    private Transform _model;
    private Animator _animator;
    private int _dirXHash;
    private int _dirYHash;
    private Vector2 _animDir;

    // Start is called before the first frame update
    void Start()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _model = _animator.gameObject.transform;

        _dirXHash = Animator.StringToHash("DirX");
        _dirYHash = Animator.StringToHash("DirY");
    }

    public void Move(Vector3 newPosition)
    { 
        //translate vector to local movement
        _movePosition = transform.forward * newPosition.y + transform.right * newPosition.x;
        _animDir = new Vector2(newPosition.x, newPosition.y);

        if (!Mathf.Approximately(_movePosition.sqrMagnitude, 0))
        {
            _isMoving = true;
        }    
    }

    public void Stop()
    {
        AnimSetState(Vector2.zero);
        _isMoving = false;
    }

    private void AnimSetState(Vector2 state)
    {
        _animator.SetFloat(_dirXHash, state.x);
        _animator.SetFloat(_dirYHash, state.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isMoving)
        {
            if (_lockTarget != null) transform.LookAt(_lockTarget.transform);
            _ridigbody.velocity = _movePosition * _moveSpeed;
            AnimSetState(_animDir);
            
            _isMoving = false;
        }
    }
}
