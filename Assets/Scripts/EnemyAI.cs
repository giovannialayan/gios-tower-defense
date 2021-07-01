using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    //move speed
    private float moveSpeed;

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

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 2 * Time.fixedDeltaTime;
        tileI = 0;
        
        killWorth = health;

        currency = GameObject.FindGameObjectWithTag("currency").GetComponent<CurrencyManager>();

        healthBarParent = transform.GetChild(0);
        healthBarSprite = healthBarParent.GetChild(0).GetComponent<SpriteRenderer>();
        healthBarBackground = transform.GetChild(1).GetComponent<SpriteRenderer>();
        ishealthShown = false;
        healthShowTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    //property for path so it can be given to the enemy from outside
    public List<Vector3Int> Path
    {
        get { return path; }
        set { path = value; }
    }

    //change pos to center of tile (fix later to add offset of tilemap)
    private Vector3 PathPosToTileCenter(Vector3Int pathTile)
    {
        return new Vector3(pathTile.x + .5f, pathTile.y + .5f);
    }

    //take damage from bullets
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "bullet")
        {
            health -= collision.gameObject.GetComponent<BulletAI>().DamageToDeal();
            Destroy(collision.gameObject);
            UpdateHealthBar(health, maxHealth);

            if (health <= 0)
            {
                currency.CurrentCurrency += Mathf.FloorToInt(killWorth);
                Destroy(transform.gameObject);
            }
        }
    }

    //increase health based on time
    public void IncreaseHealth(float time)
    {
        maxHealth += Mathf.FloorToInt(time / 10);
        killWorth = maxHealth;
        health = maxHealth;
    }

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
}
