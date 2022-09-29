using UnityEngine;
using NewTypes;

public class Boxer : MonoBehaviour
{
    //Base components
    [SerializeField] protected BoxerData _data;
    protected Rigidbody _ridigbody;
    private Collider _collider;
    protected Animator _animator;
    private GameObject _model;
    protected int _health;
    public int Health { get => _health; }
    protected BoxerState _state;

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
    protected const float MIN_ROT = 0.0001f;

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

    //No health logic
    protected int _animIsNoHealth;

    //Final punch logic
    protected int _animIsFinalPunch;
    protected int _animFinalPunch;

    //Ragdoll logic
    private bool _startRagdoll = false;
    private Rigidbody[] _ragdollRigidbodies;
    private CharacterJoint[] _ragdollJoints;
    private Collider[] _ragdollColliders;

    protected virtual void Init()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();
        _model = _animator.gameObject;

        _punchAnimEvents = GetComponentInChildren<AnimEventHelper>();
        _punchAnimEvents.MyEvent += Punch;
        _punchAnimEvents.MyEvent2 += FinalPunch;

        _healthbar = GetComponentInChildren<Healthbar>();

        _health = _data.Health;

        if (_healthbar != null)
        {
            _healthbar.SetHealth(_health, _data.Health);
        }

        _animIsTurning = Animator.StringToHash("IsTurning");
        _animIsNoHealth = Animator.StringToHash("IsNoHealth");
        _animIsFinalPunch = Animator.StringToHash("IsFinalPunch");
        _dirXHash = Animator.StringToHash("DirX");
        _dirYHash = Animator.StringToHash("DirY");
        _animPunching = Animator.StringToHash("Armature|GGPunch");
        _animFinalPunch = Animator.StringToHash("Armature|BossSuper2");

        _ragdollRigidbodies = _model.GetComponentsInChildren<Rigidbody>();
        _ragdollJoints = _model.GetComponentsInChildren<CharacterJoint>();
        _ragdollColliders = _model.GetComponentsInChildren<Collider>();
        _startRagdoll = false;
        EnableRagdoll(_startRagdoll);

        SetState(BoxerState.Fighting);
    }

    protected virtual void UpdateFixed()
    {
        //
    }

    protected virtual void UpdateNormal()
    {
        if (_state == BoxerState.Fighting || _state == BoxerState.FinalPunch)
        {
            RotateTo(_lockTarget);

            if (_isMoving)
            {
                StartMoving();
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
    }

    protected virtual void CollisionEnter(Collision other) 
    {
        if (_state == BoxerState.Fighting || _state == BoxerState.FinalPunch)
        {
            if (other.gameObject.CompareTag("Boxer"))
            {
                InitPunchTransition(true, other.gameObject.GetComponent<Boxer>());
            }
        }
    }

    protected virtual void CollisionExit(Collision other) 
    {
        if (_state == BoxerState.Fighting || _state == BoxerState.FinalPunch)
        {
            if (other.gameObject.CompareTag("Boxer"))
            {
                InitPunchTransition(false, null);
            }
        }
    }

    protected void InitPunchTransition(bool state, Boxer punchObject)
    {
        _isPunhingStarted = state;
        _isPunchingEnded = !state;

        _currentPunchWeight = _animator.GetLayerWeight(PUNCH_LAYER);
        _punchAnimLerpTime = 0;

        if (state)
        {
            if (_state == BoxerState.Fighting)
                _animator.Play(_animPunching, PUNCH_LAYER, 0);
            else if (_state == BoxerState.FinalPunch)
                _animator.Play(_animFinalPunch, PUNCH_LAYER, 0);
        }
        else
        {
            _animator.StopPlayback();
        }

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

    protected void FinalPunch()
    {
        _animator.SetBool(_animIsFinalPunch, false);
        if (_punchObject != null)
        {
            _punchObject.Damage(_data.Power);
        }
        InitPunchTransition(false, null);
    }

    protected void Punch(int hand)
    {
        if (!_isPunchingEnded)
        {
            _punchObject.Damage(_data.Power);
            PlayPunchParticle(hand);

            if (_punchObject.Health <= _data.Power)
            {
                SetState(BoxerState.FinalPunch);
            }
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
            SetState(BoxerState.Ragdoll);
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
            if (Quaternion.Angle(transform.rotation, targetRotation) >= MIN_ROT && !_isRotating)
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

        _isMoving = false;
    }

    protected void AnimSetMoveState(Vector2 state)
    {
        _animator.SetFloat(_dirXHash, state.x);
        _animator.SetFloat(_dirYHash, state.y);
    }

    private void EnableRagdoll(bool isEnabled)
    {
        //base physics
        _animator.enabled = !isEnabled;
        _ridigbody.detectCollisions = !isEnabled;
        _ridigbody.useGravity = !isEnabled;
        _collider.enabled = !isEnabled;

        //ragdoll components
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            ragdollRigidbody.detectCollisions = isEnabled;
            ragdollRigidbody.useGravity = isEnabled;
        }
        foreach (CharacterJoint ragdollJoint in _ragdollJoints)
        {
            ragdollJoint.enableCollision = isEnabled;
        }
        foreach (Collider ragdollCollider in _ragdollColliders)
        {
            ragdollCollider.enabled = isEnabled;
        }
    }

    private void ShowHealthBar(bool state)
    {
        _healthbar.GetComponent<Canvas>().enabled = state;
    }

    public void SetState(BoxerState state)
    {
        _state = state;
        switch (_state)
        {
            case BoxerState.Fighting:
                break;
            case BoxerState.NoHealth:
                _isMoving = false;
                _isPunchingEnded = true;
                _isPunhingStarted = false;
                _isRotating = false;
                _animator.SetBool(_animIsNoHealth, true);
                _animator.SetLayerWeight(PUNCH_LAYER, 0);
                break;
            case BoxerState.FinalPunch:
                _animator.SetBool(_animIsFinalPunch, true);
                break;
            case BoxerState.Ragdoll:
                _isMoving = false;
                _isPunchingEnded = true;
                _isPunhingStarted = false;
                _isRotating = false;
                _animator.SetBool(_animIsFinalPunch, true);
                _animator.SetLayerWeight(PUNCH_LAYER, 0);
                EnableRagdoll(true);
                ShowHealthBar(false);
                break;
        }
    }
}
