using System;

[System.Serializable]
public class Unit
{
    public string unitName;
    public string unitDescription;
    public int unitCost;
    public int unitHealth;
    public int unitDamage;
    public float moveSpeed;
    public float attackCooldown;
    public float unitRange; // 0 = melee
    public Era age;

    public event Action OnUnitDeath;

    internal void TakeDamage(int unitDamage)
    {
        unitHealth -= unitDamage;
        if(unitHealth <= 0)
        {
            OnUnitDeath?.Invoke();
        }
    }
}
