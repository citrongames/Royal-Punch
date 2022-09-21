using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private GameObject _lockTarget;
    [SerializeField] private ParticleSystem _leftPunch;
    [SerializeField] private ParticleSystem _rightPunch;
    private bool _isMoving = false;
    private Vector3 _movePosition;
    private Rigidbody _ridigbody;
    private Transform _model;
    private Animator _animator;
    private int _dirXHash;
    private int _dirYHash;
    private Vector2 _animDir;
    private AnimEventHelper _animeEventHelper;
    private const int PUNCH_LAYER = 1;

    // Start is called before the first frame update
    void Start()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _model = _animator.gameObject.transform;
        _animeEventHelper = GetComponentInChildren<AnimEventHelper>();
        _animeEventHelper.MyEvent.AddListener(PlayPunchParticle);

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

    public void PlayPunchParticle(int hand)
    {
        switch (hand)
        {
            case 1: //Left
                _leftPunch.Play();
                break;
            case 2: //Right
                _rightPunch.Play();
                break;
            default:
                Debug.Log("No such hand number " + hand);
                break;
        }
    }

    public void Punch(bool state)
    {
        if (state)
        {
            _animator.SetLayerWeight(PUNCH_LAYER, 1);
        }
        else
        {
            _animator.SetLayerWeight(PUNCH_LAYER, 0);
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

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Punch(true);
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Punch(false);
        }
    }
}
