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

    //Punching logic
    [SerializeField] private float _punchAnimChangeSpeed;
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
        _animator = GetComponentInChildren<Animator>();
        _animIsTurning = Animator.StringToHash("IsTurning");
        _animPunching = Animator.StringToHash("Armature|BossPunch");

        _punchAnimEvents = GetComponentInChildren<AnimEventHelper>();
        _punchAnimEvents.MyEvent += Punch;
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

    void Update()
    {
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
        if (other.gameObject.CompareTag("Player"))
        {
            InitPunchTransition(true);
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if (other.gameObject.CompareTag("Player"))
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
            Debug.Log("Enemy hit hand" + hand);
        }
    }
}
