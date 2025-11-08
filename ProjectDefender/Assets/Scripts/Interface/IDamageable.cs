using UnityEngine;

/// <summary>
/// Defines a contract for any object that can receive damage.
/// Implemented by enemies and the player castle.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Method called to apply a specified amount of damage.
    /// </summary>
    void TakeDamage(float damage);
}