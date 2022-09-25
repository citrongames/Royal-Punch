using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewTypes;

public class GameManager : MonoBehaviour
{
    private Joystick _joystick;
    private InputSystem _inputSystem;
    private TouchInfo _touchInfo;
    private Player _player;

    // Start is called before the first frame update
    void Start()
    {
        _joystick = GameObject.FindObjectOfType<Joystick>();
        _inputSystem = new InputSystem();
        _player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        _touchInfo = _inputSystem.ReadInput();
        StateManager();
    }

    private void StateManager()
    {
        switch (_touchInfo.Phase)
        {
            case TouchPhase.Began:
                _joystick.ShowJoystick(true, _touchInfo.StartPos);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                _player.MoveTo(_joystick.MoveJoystick(_touchInfo.Direction));
                break;
            case TouchPhase.Ended:
                _player.StopMoving();
                _joystick.ShowJoystick(false, Vector3.zero);
                break;
        }
    }
}
