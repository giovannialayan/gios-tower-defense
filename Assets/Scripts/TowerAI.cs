using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAI : MonoBehaviour
{
    //range of tower
    private float rangeRadius;

    //range view of tower
    private GameObject rangeView;
    private Transform enemyInRange;
    private BoxCollider2D towerCol;

    //reference to MousePickUp script
    private MousePickUp mousePickUp;

    //bullet object
    public GameObject bullet;

    //tower attack speed
    private float attackSpeed;
    private float timeSinceAttack;

    // Start is called before the first frame update
    void Start()
    {
        //set base range of tower
        rangeRadius = 5;
        rangeView = transform.GetChild(0).gameObject;
        ChangeTowerRange(rangeRadius);
        towerCol = transform.GetComponent<BoxCollider2D>();

        mousePickUp = GetComponent<MousePickUp>();

        enemyInRange = null;

        //attack speed set up
        attackSpeed = 1;
        timeSinceAttack = attackSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(enemyInRange + ", " + timeSinceAttack);

        if (enemyInRange != null && mousePickUp.ItemPlaced)
        {
            ShootAt(enemyInRange);
        }
        else if (timeSinceAttack < attackSpeed)
        {
            timeSinceAttack = attackSpeed;
        }

        //show range when hovering over tower and stop when mouse leaves tower
        if (towerCol.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
        {
            rangeView.GetComponent<SpriteRenderer>().enabled = true;
        }
        else if (mousePickUp.ItemPlaced)
        {
            rangeView.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    //property for enemyInRange so it can be set from the EnemyFinder script
    public Transform EnemyInRange
    {
        get { return enemyInRange; }
        set { enemyInRange = value; }
    }

    //change range of tower
    private void ChangeTowerRange(float radius)
    {
        rangeView.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
    }

    //shoot bullets at enemies when they enter range
    private void ShootAt(Transform target)
    {
        if (timeSinceAttack >= attackSpeed)
        {
            Instantiate(bullet, transform.position, Quaternion.identity, target.transform);
            timeSinceAttack = 0;
        }

        timeSinceAttack += Time.fixedDeltaTime;
    }

    public void ChangeTarget(Transform target)
    {
        if (target != enemyInRange)
        {
            timeSinceAttack = attackSpeed;
        }
        enemyInRange = target;
    }
}
