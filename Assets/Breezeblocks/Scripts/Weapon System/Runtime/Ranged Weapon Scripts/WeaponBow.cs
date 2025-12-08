using UnityEngine;
using ObjectPool;

public class WeaponBow : WeaponBase, IWeapon
{
    protected RangedStats _bowStats;

    private float _currentDrawTime = 0f;
    private float DrawTimePercentage { get { return _currentDrawTime / _bowStats.MaxDrawTime; } }

    // ======================================================================

    private void Start()
    {
        _bowStats = _config.RangedStats;
    }

    private void Update()
    {
        if (!_owner.IsDrawingBow || _owner == null)
            return;

        float dt = Time.deltaTime;

        // 1) Increase draw time (clamped)
        _currentDrawTime += dt;
        if (_currentDrawTime >= _bowStats.MaxDrawTime)
        {
            _currentDrawTime = _bowStats.MaxDrawTime;
            // Optional: auto-fire at full charge
            // ReleaseDraw();
            // return;
        }

        // 2) Drain stamina over time
        _owner.DrainStamina(_bowStats.DrawStaminaDrain * dt);
        _owner.DrainFatigue(5f * dt);

        // 3) Draw Indicator
        _owner.UI.UpdateBowIndicator(DrawTimePercentage);

        // 3) Auto-release if stamina is gone
        if (!HasAnyStaminaLeft())
        {
            ReleaseDraw();
        }
    }

    // ======================================================================

    public override void OnUnequipped()
    {
        CancelDraw();
    }

    // ======================================================================

    public override void PerformPrimaryAttack() { }
    public override void PerformSecondaryAttack() { }

    public override void HoldPrimaryAttack() { }
    public override void ReleasePrimaryAttack() { }
    public override void HoldSecondaryAttack()
    {
        if (!_owner.IsDrawingBow)
            BeginDraw();   
    }
    public override void ReleaseSecondaryAttack() 
    { 
         ReleaseDraw();
    }

    // ======================================================================

    public void BeginDraw()
    {
        if (_owner.IsDrawingBow)
            return;

        // Optional: check if we have at least some stamina to start
        if (!HasEnoughStaminaToStart())
            return;

        _owner.UI.EnableBowIndicator(true);

        _owner.IsDrawingBow = true;
        _currentDrawTime = 0f;
    }

    public void ReleaseDraw()
    {
        if (!_owner.IsDrawingBow)
            return;

        _owner.UI.EnableBowIndicator(false);
        FireArrow();
        _owner.IsDrawingBow = false;
        _currentDrawTime = 0f;
    }

    public void CancelDraw()
    {
        _owner.UI.EnableBowIndicator(false);
        _owner.IsDrawingBow = false;
        _currentDrawTime = 0f;
        // Optionally play "relax bow" animation here
    }

    private void FireArrow()
    {
        if (_bowStats.ProjectilePrefab == null || _owner == null)
            return;

        float chargePercent = Mathf.Clamp01(_currentDrawTime / _bowStats.MaxDrawTime);

        float speed = Mathf.Lerp(_bowStats.MinArrowSpeed, _bowStats.MaxArrowSpeed, chargePercent);
        float spreadAngle = Mathf.Lerp(_bowStats.MaxSpreadAngle, _bowStats.MinSpreadAngle, chargePercent);
        float distance = Mathf.Lerp(_bowStats.MinDistance, _bowStats.MaxDistance, chargePercent);
        float power = Mathf.Lerp(_bowStats.MinDamageMult, _bowStats.MaxDamageMult, chargePercent);

        float randomAngle = Random.Range(-spreadAngle, spreadAngle);

        Quaternion rot = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, 0f, 90f));

        float finalMinDamage = _owner.GetDamageWithModifier(_bowStats.MinDamage, DamageType.Piercing);
        float finalMaxDamage = _owner.GetDamageWithModifier(_bowStats.MaxDamage, DamageType.Piercing);
        float randomizeDamage = Random.Range(finalMinDamage, finalMaxDamage);
        float finalDamage = randomizeDamage * power;

        Debug.Log("Bow base Min Damage: " + _bowStats.MinDamage + 
            "\n Bow base Max Damage: " + _bowStats.MaxDamage +
            "\n Bow Min Damage with player's modifier: " + finalMinDamage +
            "\n Bow Max Damage with player's modifier: " + finalMaxDamage + 
            "\n Randomized damage: " + randomizeDamage + 
            "\n Final damage with power multiplier: " + finalDamage);

        ProjectileBase arrow = ObjectPooler.instance.SpawnFromPool(_bowStats.ProjectilePrefab, transform.position, rot).GetComponent<ProjectileBase>();
        arrow.Initialize(speed, distance, randomAngle, finalDamage, _bowStats);
    }

    private bool HasAnyStaminaLeft()
    {

        return _owner.CurrentStamina > 0f;
    }

    private bool HasEnoughStaminaToStart()
    {
        return _owner.CurrentStamina > _bowStats.DrawStaminaDrain;
    }

    // ======================================================================
}

