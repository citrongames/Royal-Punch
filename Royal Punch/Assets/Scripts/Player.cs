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

    //Punching logic
    [SerializeField] private float _punchAnimChangeSpeed;
    [SerializeField] private ParticleSystem _leftPunch;
    [SerializeField] private ParticleSystem _rightPunch;
    private const int PUNCH_LAYER = 1;
    private bool _isPunhingStarted = false;
    private bool _isPunchingEnded = false;
    private float _currentPunchWeight;
    private float _punchAnimLerpTime;
    private int _animPunching;
    private AnimEventHelper _punchAnimEvents;

    // Start is called before the first frame update
    void Start()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _model = _animator.gameObject.transform;
        _punchAnimEvents = GetComponentInChildren<AnimEventHelper>();
        _punchAnimEvents.MyEvent += Punch;

        _dirXHash = Animator.StringToHash("DirX");
        _dirYHash = Animator.StringToHash("DirY");
        _animPunching = Animator.StringToHash("Armature|GGPunch");
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
        if (_lockTarget != null) 
        {
            //Quaternion targetRotation = Quaternion.LookRotation(_lockTarget.transform.position - transform.position);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);

            transform.LookAt(_lockTarget.transform);
        }

        if (_isMoving)
        {
            _ridigbody.velocity = _movePosition * _moveSpeed;
            AnimSetState(_animDir);
            
            _isMoving = false;
        }

        if (_isPunhingStarted)
        {
            PunchTransition(1);
        }   
        if (_isPunchingEnded)
        {
            PunchTransition(0);
        } 
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            InitPunchTransition(true);
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            InitPunchTransition(false);
        }
    }

    private void InitPunchTransition(bool state)
    {
        _isPunhingStarted = state;
        _isPunchingEnded = !state;

        _currentPunchWeight = _animator.GetLayerWeight(PUNCH_LAYER);
        _punchAnimLerpTime = 0;

        if (state)
            _animator.Play(_animPunching, PUNCH_LAYER, 0);
    }

    private void PunchTransition(int targetWeight)
    {
        _animator.SetLayerWeight(PUNCH_LAYER, Mathf.Lerp(_currentPunchWeight, targetWeight, _punchAnimLerpTime));
        _punchAnimLerpTime += Time.deltaTime * _punchAnimChangeSpeed;
        if (_punchAnimLerpTime > 1f)
        {
            _isPunhingStarted = false;
        }
    }

    public void Punch(int hand)
    {
        if (!_isPunchingEnded)
        {
            //Add punch damage here
            PlayPunchParticle(hand);
        }
    }

    private void PlayPunchParticle(int hand)
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
}
