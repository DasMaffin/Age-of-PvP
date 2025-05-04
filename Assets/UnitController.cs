using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class UnitController : MonoBehaviour
{
    public Unit unit;

    private UnitController attackedEnemy;
    private bool canMove = true;
    private bool canAttack
    {
        set
        {
            if(value)
            {
                InvokeRepeating("Attack", 0, unit.attackCooldown);
            }
            else
            {
                CancelInvoke("Attack");
            }
        }
    }

    private void Awake()
    {
        unit.OnUnitDeath += () =>
        {
            Destroy(gameObject);
        };
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if(!canMove) return;
        if(tag == "MyUnit")
            transform.Translate(Vector3.right * Time.deltaTime * unit.moveSpeed);
        else if(tag == "EnemyUnit")
            transform.Translate(Vector3.left * Time.deltaTime * unit.moveSpeed);
    }

    private void Attack()
    {
        attackedEnemy.unit.TakeDamage(unit.unitDamage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 9) return;
        canMove = false;
        canAttack = true;
        attackedEnemy = collision.gameObject.GetComponent<UnitController>();
        attackedEnemy.unit.OnUnitDeath += () => // register this on every attacker, so they get notified if their enemy dies.
        {
            attackedEnemy = null;
            canMove = true;
            canAttack = false;
        };
    }
}
