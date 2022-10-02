using UnityEngine;
using NewTypes;
using System.Collections.Generic;

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
    private TargetPoint _targetPoint;

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
    [SerializeField] private Rigidbody _impactPoint;
    [SerializeField] private float _impactPower;
    private bool _startRagdoll = false;
    private Rigidbody[] _ragdollRigidbodies;
    private CharacterJoint[] _ragdollJoints;
    private Collider[] _ragdollColliders;


    //Bone restore logic
    [SerializeField] private float _getUpCooldownTime;
    [SerializeField] private float _boneRestoreSpeed;
    [SerializeField] private GameObject _armatureRoot;
    private List<BoneTransform> _bonesOriginalPose = new List<BoneTransform>();
    private List<BoneTransform> _bonesStartPose = new List<BoneTransform>();
    private float _boneRestoreLerp = 0;
    private float _getUpTimer;

    protected virtual void Init()
    {
        _ridigbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();
        _model = _animator.gameObject;
        _targetPoint = GetComponentInChildren<TargetPoint>();

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
        EnableBasePhysics(true);
        RecordBones();

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

        if (_state == BoxerState.Ragdoll)
        {
            _getUpTimer += Time.deltaTime;
            if (_getUpTimer >= _getUpCooldownTime)
                SetState(BoxerState.RestoreBones);

            _targetPoint.transform.position = new Vector3(_armatureRoot.transform.position.x, 0, _armatureRoot.transform.position.z);
        }

        if (_state == BoxerState.RestoreBones)
        {
            RestoringBones();
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
            targetRotation.eulerAngles =  new Vector3(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

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
                transform.eulerAngles =  new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
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

    private void EnableBasePhysics(bool isEnabled)
    {
        _animator.enabled = isEnabled;
        _ridigbody.detectCollisions = isEnabled;
        _ridigbody.useGravity = isEnabled;
        _collider.enabled = isEnabled;
    }

    private void ApplyRagdollForce(Vector3 direction, float power)
    {
        if (_impactPoint != null)
        {
            _impactPoint.AddRelativeForce(direction * power, ForceMode.Impulse);
        }
    }

    private void ShowHealthBar(bool state)
    {
        _healthbar.GetComponent<Canvas>().enabled = state;
    }

    private void RecordBones()
    {
        _bonesOriginalPose.Clear();
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            _bonesOriginalPose.Add(new BoneTransform(ragdollRigidbody.transform.localPosition, ragdollRigidbody.transform.localRotation));
        }
    }

    private void TranslateBones()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            positions.Add(ragdollRigidbody.transform.position);
        }

        //move parent to ragdoll root
        this.transform.position = new Vector3(_armatureRoot.transform.position.x, this.transform.position.y, _armatureRoot.transform.position.z);

        int i = 0;
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            ragdollRigidbody.transform.position = positions[i];
            i++;
        }
    }

    private void RestoreBones()
    {
        TranslateBones();
        _bonesStartPose.Clear();
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            _bonesStartPose.Add(new BoneTransform(ragdollRigidbody.transform.localPosition, ragdollRigidbody.transform.localRotation));
        }
        _boneRestoreLerp = 0;
    }

    private void RestoringBones()
    {
        int i = 0;
        bool _restoreBones = false;

        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            ragdollRigidbody.transform.localPosition = Vector3.Lerp(_bonesStartPose[i].Position, _bonesOriginalPose[i].Position, _boneRestoreLerp * _boneRestoreSpeed);
            _restoreBones |= (ragdollRigidbody.transform.localPosition != _bonesOriginalPose[i].Position);

            ragdollRigidbody.transform.localRotation = Quaternion.Lerp(_bonesStartPose[i].Rotation, _bonesOriginalPose[i].Rotation, _boneRestoreLerp * _boneRestoreSpeed);
            _restoreBones |= (Quaternion.Angle(ragdollRigidbody.transform.localRotation, _bonesOriginalPose[i].Rotation) >= MIN_ROT);

            i++;
        }

        _boneRestoreLerp += Time.deltaTime;

        if (!_restoreBones)
        {
            SetState(BoxerState.Fighting);
            _targetPoint.transform.localPosition = Vector3.zero;
        }
    }

    public void SetState(BoxerState state)
    {
        _state = state;
        switch (_state)
        {
            case BoxerState.Fighting:
                _isMoving = false;
                _isPunchingEnded = false;
                _isPunhingStarted = false;
                _isRotating = false;
                _animator.SetBool(_animIsFinalPunch, false);
                _animator.SetLayerWeight(PUNCH_LAYER, 0);
                EnableBasePhysics(true);
                EnableRagdoll(false);
                ShowHealthBar(true);
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
                _isPunchingEnded = false;
                _isPunhingStarted = false;
                _isRotating = false;
                _animator.SetLayerWeight(PUNCH_LAYER, 0);
                _animator.SetBool(_animIsFinalPunch, false);
                EnableBasePhysics(false);
                EnableRagdoll(true);
                ApplyRagdollForce(Vector3.back, _impactPower);
                ShowHealthBar(false);
                _getUpTimer = 0;
                break;
            case BoxerState.RestoreBones:
                _isMoving = false;
                _isPunchingEnded = false;
                _isPunhingStarted = false;
                _isRotating = false;
                _animator.SetLayerWeight(PUNCH_LAYER, 0);
                EnableBasePhysics(false);
                EnableRagdoll(false);
                RestoreBones();
                break;
        }
    }
}
