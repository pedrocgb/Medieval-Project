using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerBase))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables and Properties
    private PlayerBase _playerBase;

    private Vector2 _inputDir;
    private Vector2 _desiredVelocity;
    private float _currentMaxSpeed;
    private float _storedBaseVisionRotationSmoothing;
    private float _baseVisionRotationSmoothing = 10f;

    // Input
    private float _horizontalAxis;
    private float _verticalAxis;

    [FoldoutGroup("Components")]
    [SerializeField] private PlayerVisionLight _visionLight;
    private Rigidbody2D _rb;

    // -------------------------------------------------
    // Dodge
    // -------------------------------------------------

    private Vector2 _dodgeVelocity = Vector2.zero;
    private float _dodgeTimeRemaining = 0f;
    private float _dodgeCooldownRemaining = 0f;

    // simple edge-detection for the dodge button
    private bool _dodgeButtonWasHeld = false;

    private bool IsDodging => _dodgeTimeRemaining > 0f;
    #endregion

    // ==============================================================

    #region Unity Methods
    void Awake()
    {
        _playerBase = GetComponent<PlayerBase>();
        _rb = GetComponent<Rigidbody2D>();

        if (_visionLight == null)
            _visionLight = GetComponentInChildren<PlayerVisionLight>();

        if (_visionLight != null)
            _storedBaseVisionRotationSmoothing = _visionLight.rotationSmoothing;
        else
            _storedBaseVisionRotationSmoothing = _baseVisionRotationSmoothing;

        UpdateLoadModifiers();
    }

    void Update()
    {
        MovementInputHandler();
        DodgeInputHandler();
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    // ==============================================================

    private void MovementInputHandler()
    {
        _horizontalAxis = _playerBase.ControlScheme.GetAxis("Horizontal Axis");
        _verticalAxis = _playerBase.ControlScheme.GetAxis("Vertical Axis");

        if (Mathf.Approximately(_horizontalAxis, 0f) &&
            Mathf.Approximately(_verticalAxis, 0f))
        {
            _inputDir = Vector2.zero;
            _desiredVelocity = Vector2.zero;
            _currentMaxSpeed = 0f;
            return;
        }

        _inputDir = new Vector2(_horizontalAxis, _verticalAxis);

        if (_inputDir.sqrMagnitude > 1f)
            _inputDir.Normalize();

        bool wantsRun = _playerBase.ControlScheme.GetButton("Run");

        float targetSpeed = _playerBase.MoveSpeed.Value * _playerBase.CurrentLoadConfig.moveSpeedMultiplier;

        if (wantsRun && _playerBase.CanSprint() && _inputDir.sqrMagnitude > 0.01f && !_playerBase.StaminaDepleted)
        {
            targetSpeed *= _playerBase.RunMultiplier.Value;
            _playerBase.DrainSprintStamina();
            _playerBase.DrainSprintFatigue();
        }
        else
        {
            _playerBase.DrainWalkFatigue();
        }

        if (_playerBase.CurrentLoadStage == CarryLoadStage.Overburdened)
        {
            targetSpeed = Mathf.Min(targetSpeed, CarryWeightSettings.MaxOverburdenedSpeed);
        }

        _currentMaxSpeed = targetSpeed;

        _desiredVelocity = _inputDir * _currentMaxSpeed;

        if (_visionLight != null)
        {
            float turnMult = _playerBase.CurrentLoadConfig.turnSpeedMultiplier;
            _visionLight.rotationSmoothing = _storedBaseVisionRotationSmoothing * turnMult;
        }
    }

    private void Move()
    {
        Vector2 currentVel = _rb.linearVelocity;

        // If we are dodging, ignore normal movement input
        Vector2 baseTargetVel = IsDodging ? Vector2.zero : _desiredVelocity;

        // Dodge (and other external forces) are added here
        Vector2 targetVel = baseTargetVel + GetExternalForcesVelocity();

        Vector2 velDiff = targetVel - currentVel;

        float accelRate =
            (_inputDir.sqrMagnitude > 0.01f) ? _playerBase.Acceleration : _playerBase.Deceleration;

        Vector2 change = Vector2.ClampMagnitude(velDiff, accelRate * Time.fixedDeltaTime);
        _rb.linearVelocity = currentVel + change;
    }

    // ==============================================================

    private void UpdateLoadModifiers()
    {

    }

    // ---------------------------------
    // Dodge logic
    // ---------------------------------

    private void DodgeInputHandler()
    {
        // Update timers
        if (_dodgeTimeRemaining > 0f)
        {
            _dodgeTimeRemaining -= Time.deltaTime;
            if (_dodgeTimeRemaining < 0f)
                _dodgeTimeRemaining = 0f;
        }

        if (_dodgeCooldownRemaining > 0f)
        {
            _dodgeCooldownRemaining -= Time.deltaTime;
            if (_dodgeCooldownRemaining < 0f)
                _dodgeCooldownRemaining = 0f;
        }

        // Read button
        bool dodgeButtonHeld = _playerBase.ControlScheme.GetButton("Dodge");
        bool dodgePressedThisFrame = dodgeButtonHeld && !_dodgeButtonWasHeld;
        _dodgeButtonWasHeld = dodgeButtonHeld;

        // Only start a new dodge if:
        // - we just pressed the button
        // - not already dodging
        // - cooldown is ready
        // - and we have a movement direction
        if (dodgePressedThisFrame &&
            !IsDodging &&
            _dodgeCooldownRemaining <= 0f &&
            _inputDir.sqrMagnitude > 0.001f &&
            _playerBase.CanDodge())
        {
            Vector2 dodgeDir = _inputDir.normalized;
            _dodgeVelocity = dodgeDir * _playerBase.DodgePower.Value;;
            _dodgeTimeRemaining = _playerBase.DodgeDuration.Value;
            _dodgeCooldownRemaining = _playerBase.DodgeCooldown.Value;

            _playerBase.DrainDodgeStamina();
            _playerBase.DrainDodgeFatigue();
        }

        // If dodge has ended, clear velocity
        if (!IsDodging)
        {
            _dodgeVelocity = Vector2.zero;
        }
    }

    // ---------------------------------
    // External forces hook
    // ---------------------------------

    /// <summary>
    /// Returns extra velocity from dodge, knockback, wind, etc.
    /// </summary>
    private Vector2 GetExternalForcesVelocity()
    {
        Vector2 extra = Vector2.zero;

        // Dodge burst
        if (IsDodging)
        {
            extra += _dodgeVelocity;
        }

        // Add other external forces here in the future (knockback, wind, etc.)

        return extra;
    }
}
