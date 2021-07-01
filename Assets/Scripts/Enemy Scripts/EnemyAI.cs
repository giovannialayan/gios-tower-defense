using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    //move speed
    private float moveSpeed;
    private float realSpeed;

    //path for enemy to follow
    private List<Vector3Int> path;

    //iteration for path to follow
    private int tileI;

    //enemy health and health bar
    private float health = 4;
    private float maxHealth = 4;
    private Transform healthBarParent;
    private SpriteRenderer healthBarSprite;
    private SpriteRenderer healthBarBackground;
    private bool ishealthShown;
    private float healthShowTimer;

    //currency manager reference to give it money
    private CurrencyManager currency;
    private float killWorth;

    //damage it deals
    private float attack;

    //element type
    private ElementTypes type;

    //base reference
    private BaseManager playerBase;

    //status effect variables
    private bool slowed;
    private bool stunned;
    private float slowTimer;
    private float stunTimer;
    private float slowAmount;

    //gamemanager
    public GameManager gamemanager { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 2 * Time.fixedDeltaTime;
        realSpeed = moveSpeed;
        tileI = 0;
        
        killWorth = health;
        attack = health / 3;

        currency = GameObject.FindGameObjectWithTag("money").GetComponent<CurrencyManager>();
        //Debug.LogError(currency);
        playerBase = GameObject.FindGameObjectWithTag("base").GetComponent<BaseManager>();

        healthBarParent = transform.GetChild(0);
        healthBarSprite = healthBarParent.GetChild(0).GetComponent<SpriteRenderer>();
        healthBarBackground = transform.GetChild(1).GetComponent<SpriteRenderer>();
        ishealthShown = false;
        healthShowTimer = 0;

        slowed = false;
        stunned = false;
        slowTimer = 0;
        stunTimer = 0;
        slowAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (gamemanager.IsPaused)
        {
            return;
        }

        //continue moving as long as there is a tile to move to
        if (tileI < path.Count - 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[tileI + 1].x, path[tileI + 1].y, 0), moveSpeed);

            //when it reaches the tile start moving to the next tile
            if (Vector3.Distance(transform.position, new Vector3(path[tileI + 1].x, path[tileI + 1].y, 0)) < .01f)
            {
                tileI++;
            }
        }
        //it has reached the end of its path, so it must have hit the base
        else
        {
            playerBase.DecreaseHealth(attack);
            Destroy(transform.gameObject);
        }

        //fade out the health bar if nothing is happening to it
        if (ishealthShown)
        {
            if (healthShowTimer >= 1)
            {
                Color cFront = healthBarSprite.color;
                Color cBack = healthBarBackground.color;
                cFront.a -= .1f;
                cBack.a -= .1f;
                healthBarSprite.color = cFront;
                healthBarBackground.color = cBack;

                if (cFront.a == 0)
                {
                    ishealthShown = false;
                    healthShowTimer = 0;
                }
            }
            healthShowTimer += Time.fixedDeltaTime;
        }

        //status effect active
        if (stunned || slowed)
        {
            slowTimer -= Time.deltaTime;
            stunTimer -= Time.deltaTime;

            if (slowed && slowTimer <= 0)
            {
                moveSpeed = realSpeed;
                slowed = false;
            }
            if(stunned && stunTimer <= 0)
            {
                moveSpeed = realSpeed;
                stunned = false;
            }
        }
    }

    //property for path so it can be given to the enemy from outside
    public List<Vector3Int> Path
    {
        get { return path; }
        set { path = value; }
    }

    //change pos to center of tile
    private Vector3 PathPosToTileCenter(Vector3Int pathTile)
    {
        return new Vector3(pathTile.x + .5f, pathTile.y + .5f);
    }

    //take damage from bullets
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "bullet")
        {
            BulletAI bullet = collision.gameObject.GetComponent<BulletAI>();
            health -= bullet.DamageToDeal(type);

            switch (bullet.StatusEffect)
            {
                //earth tower
                case 1:
                    slowed = true;
                    slowTimer = 2;
                    //see game_balancing.txt for information on this equation
                    slowAmount = (1 / maxHealth) * (maxHealth - bullet.DamageToDeal(ElementTypes.Error));
                    moveSpeed *= slowAmount;
                    break;
                //nature tower
                case 2:
                    stunned = true;
                    stunTimer = bullet.DamageToDeal(ElementTypes.Error) / 3f;
                    moveSpeed = 0;
                    break;
                //chaos tower
                case 3:
                    health -= maxHealth * .1f;
                    break;
                //earth + chaos tower
                case 4:
                    slowed = true;
                    slowTimer = 2;
                    slowAmount = (1 / maxHealth) * (bullet.DamageToDeal(ElementTypes.Error) - maxHealth);
                    moveSpeed *= slowAmount;
                    health -= maxHealth * .1f;
                    break;
                //nature + chaos tower
                case 5:
                    stunned = true;
                    stunTimer = bullet.DamageToDeal(ElementTypes.Error) / 3f;
                    moveSpeed = 0;
                    health -= maxHealth * .1f;
                    break;

                    //currently if an earth bullet and nature bullet hit an enemy on the same frame i think only one of them will happen
                    //idk how to fix this since it would be 2 different objects that cant interact with each other
                    //i also dont know if only one of them will happen or if it will be fine and both will happen
                    //i kind of assume that because they're two different objects it will work fine
                    //but i dont know until i test it and im not sure how to force 2 bullets to hit an enemy on the same frame
            }

            //Debug.LogError(health);

            Destroy(collision.gameObject);
            UpdateHealthBar(health, maxHealth);

            if (health <= 0)
            {
                //Debug.LogError("destroyed");
                //Debug.LogError(currency);
                Destroy(transform.gameObject);
                currency.CurrentCurrency += (uint)Mathf.FloorToInt(killWorth);
            }
        }
    }

    //increase health based on time
    //public void IncreaseHealth(float time)
    //{
    //    maxHealth += Mathf.FloorToInt(time / 10);
    //    killWorth = maxHealth;
    //    health = maxHealth;
    //}

    //update the healthbar to reflect the amount of health remaining
    private void UpdateHealthBar(float amount, float max)
    {
        healthBarParent.localScale = new Vector3(amount / max, 1, 1);

        healthBarSprite.color = new Color(healthBarSprite.color.r, healthBarSprite.color.g, healthBarSprite.color.b, 1);
        healthBarBackground.color = new Color(healthBarBackground.color.r, healthBarBackground.color.g, healthBarBackground.color.b, 1);
        ishealthShown = true;
    }

    //get number of tiles from end
    public int DistToEnd()
    {
        return path.Count - tileI;
    }

    //set the stats of the enemy
    public void SetEnemyStats(float hlth, float atk, float spd, float wrth, ElementTypes etype)
    {
        health = hlth;
        maxHealth = hlth;
        attack = atk;
        moveSpeed = spd;
        killWorth = wrth;
        type = etype;
        ChangeEnemyColor(etype);
    }

    //change color of enemy based on type
    private void ChangeEnemyColor(ElementTypes etype)
    {
        switch ((int)etype)
        {
            case 0:
                transform.GetComponent<SpriteRenderer>().color = new Color(.2f, .2f, .2f);
                break;
            case 1:
                transform.GetComponent<SpriteRenderer>().color = new Color(.8f, .8f, .8f);
                break;
            case 2:
                transform.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
                break;
            case 3:
                transform.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 1f);
                break;
            case 4:
                transform.GetComponent<SpriteRenderer>().color = new Color(.8f, .4f, .1f);
                break;
            case 5:
                transform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 0f);
                break;
            case 6:
                transform.GetComponent<SpriteRenderer>().color = new Color(.7f, 0f, .8f);
                break;
            case 7:
                transform.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f);
                break;
            default:
                transform.GetComponent<SpriteRenderer>().color = Color.white;
                break;
        }
    }

    //return the type fo the enemy
    public ElementTypes EnemyType
    {
        get { return type; }
    }

    //damage enemy, used by pulse attack so everything hacing to do with pulse attacks can be handled in the pulse attack script so nothing fucky happens
    public void TakeDamage(float damage, int statusEffect, ElementTypes[] types)
    {
        switch (type)
        {
            case ElementTypes.Lightning:
                if (types[0] == ElementTypes.Earth || types[1] == ElementTypes.Earth)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Fire:
                if (types[0] == ElementTypes.Water || types[1] == ElementTypes.Water)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Whimsical:
                if (types[0] == ElementTypes.Nature || types[1] == ElementTypes.Nature)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Order:
                if (types[0] == ElementTypes.Chaos || types[1] == ElementTypes.Chaos)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Nature:
                if (types[0] == ElementTypes.Fire || types[1] == ElementTypes.Fire)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Earth:
                if (types[0] == ElementTypes.Whimsical || types[1] == ElementTypes.Whimsical)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Chaos:
                if (types[0] == ElementTypes.Order || types[1] == ElementTypes.Order)
                {
                    damage *= 1.5f;
                }
                break;

            case ElementTypes.Water:
                if (types[0] == ElementTypes.Lightning || types[1] == ElementTypes.Lightning)
                {
                    damage *= 1.5f;
                }
                break;

            default:
                break;
        }

        health -= damage;

        if (statusEffect == 3)
        {
            health -= maxHealth * .1f;
        }

        UpdateHealthBar(health, maxHealth);

        if (health <= 0)
        {
            currency.CurrentCurrency += (uint)Mathf.FloorToInt(killWorth);
            Destroy(transform.gameObject);
        }
    }
}
