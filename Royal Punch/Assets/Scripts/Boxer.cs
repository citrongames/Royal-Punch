using UnityEngine;

public class Boxer : MonoBehaviour
{
    //Base components
    [SerializeField] protected BoxerData _data;
    protected Rigidbody _ridigbody;
    protected Animator _animator;
    protected int _health;

    //Move logic
    [SerializeField] protected float _moveSpeed;
    protected bool _isMoving = false;
    protected Vector3 _movePosition;
    protected int _dirXHash;
    protected int _dirYHash;
    protected Vector2 _animMoveDirection;

    //Rotate logic
    [SerializeField] protected GameObject _lockTarget;
    [SerializeField] protected float _turnSpeed;
    protected int _animIsTurning;
    protected bool _isRotating = false;
    protected const float MIN_ROT = 0.01f;

    //Punching logic
    [SerializeField] protected float _punchAnimChangeSpeed;
    [SerializeField] protected ParticleSystem _leftPunch;
    [SerializeField] protected ParticleSystem _rightPunch;
    protected const int PUNCH_LAYER = 1;
    protected bool _isPunhingStarted = false;
    protected bool _isPunchingEnded = false;
    protected float _currentPunchWeight;
    protected float _punchAnimLerpTime;
    protected int _animPunching;
    protected AnimEventHelper _punchAnimEvents;
    protected Boxer _punchObject;

    //Healthbar logic
    [SerializeField] protected Healthbar _healthbar;

    protected virtual void Init()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _animIsTurning = Animator.StringToHash("IsTurning");

        _punchAnimEvents = GetComponentInChildren<AnimEventHelper>();
        _punchAnimEvents.MyEvent += Punch;

        _healthbar = GetComponentInChildren<Healthbar>();

        _health = _data.Health;

        if (_healthbar != null)
        {
            _healthbar.SetHealth(_health, _data.Health);
        }
    }

    protected virtual void UpdateFixed()
    {
        //
    }

    protected virtual void UpdateNormal()
    {
        if (_isMoving)
        {
            StartMoving();
        }

        RotateTo(_lockTarget);

        if (_isPunhingStarted)
        {
            PunchTransition(1);
        }   
        if (_isPunchingEnded)
        {
            PunchTransition(0);
        } 

        _isMoving = false;
    }

    protected virtual void CollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Boxer"))
        {
            InitPunchTransition(true, other.gameObject.GetComponent<Boxer>());
        }
    }

    protected virtual void CollisionExit(Collision other) 
    {
        if (other.gameObject.CompareTag("Boxer"))
        {
            InitPunchTransition(false, null);
        }
    }

    protected void InitPunchTransition(bool state, Boxer punchObject)
    {
        _isPunhingStarted = state;
        _isPunchingEnded = !state;

        _currentPunchWeight = _animator.GetLayerWeight(PUNCH_LAYER);
        _punchAnimLerpTime = 0;

        if (state)
            _animator.Play(_animPunching, PUNCH_LAYER, 0);

        _punchObject = punchObject;
    }

    protected void PunchTransition(int targetWeight)
    {
        _animator.SetLayerWeight(PUNCH_LAYER, Mathf.Lerp(_currentPunchWeight, targetWeight, _punchAnimLerpTime));
        _punchAnimLerpTime += Time.deltaTime * _punchAnimChangeSpeed;
        if (_punchAnimLerpTime > 1f)
        {
            _isPunhingStarted = false;
        }
    }

    protected void Punch(int hand)
    {
        if (!_isPunchingEnded)
        {
            _punchObject.Damage(_data.Power);
            PlayPunchParticle(hand);
        }
    }

    protected void PlayPunchParticle(int hand)
    {
        if (_leftPunch != null && _rightPunch != null)
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

    public void Damage(int value)
    {
        _health -= value;
        if (_health <= 0)
        {
            Debug.Log("No health " + gameObject.name);
            _health = 0;
        }

        if (_healthbar != null)
        {
            _healthbar.SetHealth(_health, _data.Health);
        }
    }

    protected void RotateTo(GameObject lockTarget)
    {
        if (lockTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_lockTarget.transform.position - transform.position);
            if (Quaternion.Angle(transform.rotation, targetRotation) > MIN_ROT && !_isRotating)
            {
                _isRotating = true;
                if (!_isMoving)
                    _animator.SetBool(_animIsTurning, true);
            }
            else if (Quaternion.Angle(transform.rotation, targetRotation) < MIN_ROT && _isRotating)
            {
                _isRotating = false;
                if (!_isMoving)
                    _animator.SetBool(_animIsTurning, false);
            }
            if (_isRotating)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveTo(Vector3 newPosition)
    { 
        //translate vector to local movement
        _movePosition = transform.forward * newPosition.y + transform.right * newPosition.x;
        _animMoveDirection = new Vector2(newPosition.x, newPosition.y);

        if (!Mathf.Approximately(_movePosition.sqrMagnitude, 0))
        {
            _isMoving = true;
        }    
    }

    public void StopMoving()
    {
        AnimSetMoveState(Vector2.zero);
        _isMoving = false;
    }

    protected void StartMoving()
    {
        _ridigbody.velocity = _movePosition * _moveSpeed;
        AnimSetMoveState(_animMoveDirection);
    }

    protected void AnimSetMoveState(Vector2 state)
    {
        _animator.SetFloat(_dirXHash, state.x);
        _animator.SetFloat(_dirYHash, state.y);
    }
}
