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
    private GameManager gamemanager;

    //enemy origin wave
    private int enemyWave;
    public int maxEnemiesInWave { get; set; }

    //enemy class variables
    private EnemyClass enemyClass;
    private SpriteRenderer paladin_shield;
    private int paladin_shield_count;
    private float paladin_shield_timer;
    private SpriteRenderer succubus_kiss;
    private float succubus_kiss_timer;
    private SpriteRenderer warlock_negater;
    private float warlock_negater_timer;
    private float assassin_original_speed;
    private float assassin_original_attack;

    //challenge variables
    ChallengeManager challengeManager;
    private bool chaos_challenge = false;
    private bool fire_challenge = false;
    private bool nature_challenge = false;
    private Vector3 nature_challenge_pos;
    private bool firstTimeStunned = true;
    private bool earth_challenge = false;

    //enemypedia variables
    private bool cur_found = false;
    private bool knight_found = false;
    private bool monk_found = false;
    private bool assassin_found = false;
    private bool paladin_found = false;
    private bool armorer_found = false;
    private bool succubus_found = false;
    private bool warlock_found = false;

    //skill tree variables
    private float paladinShieldMaxTime = 5;
    private float assassinMultiplierSkill = 1;
    private float succubusHealPercent = .3f;
    private float monkSlowPercent = 1;
    private float knightBonusDamage = 0;
    private float armorerBonusDamage = 0;
    private float warlockNegateThreshhold = .25f;

    //audio
    public GameObject enemyDeathSound;
    private float deathSoundVolume = 1;

    //element damage multiplier
    private float elementDamageMultiplier = 2;

    // Start is called before the first frame update
    void Start()
    {
        gamemanager = FindObjectOfType<GameManager>();
        paladinShieldMaxTime = gamemanager.SkillTree["enemypaladin"];
        assassinMultiplierSkill = gamemanager.SkillTree["enemyassassin"];
        succubusHealPercent = gamemanager.SkillTree["enemysuccubus"];
        monkSlowPercent = gamemanager.SkillTree["enemymonk"];
        armorerBonusDamage = gamemanager.SkillTree["enemyarmorer"];
        warlockNegateThreshhold = gamemanager.SkillTree["enemywarlock"];
        
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

        challengeManager = FindObjectOfType<ChallengeManager>();
        //chaos_challenge = !challengeManager.ChallengeDictionary["chaosskill"];
        //fire_challenge = !challengeManager.ChallengeDictionary["fireskill"];
        //nature_challenge = !challengeManager.ChallengeDictionary["natureskill"];
        //earth_challenge = !challengeManager.ChallengeDictionary["earthskill"];

        //enemypedia
        cur_found = challengeManager.Enemypedia["cur"];
        knight_found = challengeManager.Enemypedia["knight"];
        monk_found = challengeManager.Enemypedia["monk"];
        assassin_found = challengeManager.Enemypedia["assassin"];
        paladin_found = challengeManager.Enemypedia["paladin"];
        armorer_found = challengeManager.Enemypedia["armorer"];
        succubus_found = challengeManager.Enemypedia["succubus"];
        warlock_found = challengeManager.Enemypedia["warlock"];

        //sound
        deathSoundVolume = gamemanager.sfxVolume;
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
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[tileI + 1].x, path[tileI + 1].y, 0), moveSpeed * Time.deltaTime);

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
            if (stunned && stunTimer <= 0)
            {
                moveSpeed = realSpeed;
                stunned = false;
            }
        }

        //enemy class update
        switch (enemyClass)
        {
            case EnemyClass.Paladin:
                if (paladin_shield_timer > 0)
                {
                    paladin_shield_timer -= Time.deltaTime;

                    if (paladin_shield_timer <= 0)
                    {
                        paladin_shield_count = 0;
                        Color c = paladin_shield.color;
                        c.a = 0;
                        paladin_shield.color = c;
                    }
                }
                break;

            case EnemyClass.Succubus:
                if (succubus_kiss_timer > 0)
                {
                    succubus_kiss_timer -= Time.deltaTime;

                    Color succCol = succubus_kiss.color;
                    succCol.a = succubus_kiss_timer;
                    succubus_kiss.color = succCol;

                    if (succubus_kiss_timer <= 0)
                    {
                        Color c = succubus_kiss.color;
                        c.a = 0;
                        succubus_kiss.color = c;
                    }
                }
                break;

            case EnemyClass.Warlock:
                if (warlock_negater_timer > 0)
                {
                    warlock_negater_timer -= Time.deltaTime;

                    Color warCol = warlock_negater.color;
                    warCol.a = warlock_negater_timer;
                    warlock_negater.color = warCol;

                    if (warlock_negater_timer <= 0)
                    {
                        Color c = warlock_negater.color;
                        c.a = 0;
                        warlock_negater.color = c;
                    }
                }
                break;
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
        //only trigger if it is a bullet that is targeting this enemy
        if (collision.transform.tag == "bullet" && collision.transform.IsChildOf(transform))
        {
            BulletAI bullet = collision.gameObject.GetComponent<BulletAI>();
            float beforeHealth = health;
            health -= bullet.DamageToDeal(type);
            float preChaosHealth = health;

            switch (bullet.StatusEffect)
            {
                //earth tower
                case 1:
                    slowed = true;
                    slowTimer = 2;
                    //see game_balancing.txt for information on this equation
                    slowAmount = (1 / maxHealth) * (maxHealth - bullet.DamageToDeal(ElementTypes.Error));
                    moveSpeed *= slowAmount;

                    //earth challenge
                    if (earth_challenge && !challengeManager.earthChallengeCounter.Contains(this))
                    {
                        challengeManager.earthChallengeCounter.Add(this);

                        if (challengeManager.earthChallengeCounter.Count >= 10)
                        {
                            //challengeManager.SaveChallengeState("earthskill");
                            earth_challenge = false;
                        }
                    }
                    break;
                //nature tower
                case 2:
                    stunned = true;
                    stunTimer = bullet.DamageToDeal(ElementTypes.Error) / 3f;
                    moveSpeed = 0;

                    //nature challenge
                    if (firstTimeStunned)
                    {
                        nature_challenge_pos = transform.position;
                        firstTimeStunned = false;
                    }
                    break;
                //chaos tower
                case 3:
                    health = beforeHealth - (beforeHealth - health) * .1f;
                    health -= maxHealth * .1f;
                    break;
                //earth + chaos tower
                case 4:
                    slowed = true;
                    slowTimer = 2;
                    slowAmount = (1 / maxHealth) * (bullet.DamageToDeal(ElementTypes.Error) - maxHealth);
                    moveSpeed *= slowAmount;
                    health = beforeHealth - (beforeHealth - health) * .1f;
                    health -= maxHealth * .1f;

                    //earth challenge
                    if (earth_challenge && !challengeManager.earthChallengeCounter.Contains(this))
                    {
                        challengeManager.earthChallengeCounter.Add(this);

                        if (challengeManager.earthChallengeCounter.Count >= 10)
                        {
                            //challengeManager.SaveChallengeState("earthskill");
                            earth_challenge = false;
                        }
                    }
                    break;
                //nature + chaos tower
                case 5:
                    stunned = true;
                    stunTimer = bullet.DamageToDeal(ElementTypes.Error) / 3f;
                    moveSpeed = 0;
                    health = beforeHealth - (beforeHealth - health) * .1f;
                    health -= maxHealth * .1f;

                    //nature challenge
                    if (firstTimeStunned)
                    {
                        nature_challenge_pos = transform.position;
                        firstTimeStunned = false;
                    }
                    break;

                    //currently if an earth bullet and nature bullet hit an enemy on the same frame i think only one of them will happen
                    //idk how to fix this since it would be 2 different objects that cant interact with each other
                    //i also dont know if only one of them will happen or if it will be fine and both will happen
                    //i kind of assume that because they're two different objects it will work fine
                    //but i dont know until i test it and im not sure how to force 2 bullets to hit an enemy on the same frame
            }

            //do effects of special enemies
            switch (enemyClass)
            {
                //knight bonus damage skill
                case EnemyClass.Knight:
                    health -= (beforeHealth - preChaosHealth) * knightBonusDamage;
                    break;

                //gain a shield for 5 seconds after being hit 3 times
                case EnemyClass.Paladin:
                    if (paladin_shield_timer <= 0)
                    {
                        paladin_shield_count++;
                    }
                    else
                    {
                        health = beforeHealth;
                    }

                    if (paladin_shield_count >= 3)
                    {
                        paladin_shield_timer = paladinShieldMaxTime;
                        Color palCol = paladin_shield.color;
                        palCol.a = 1;
                        paladin_shield.color = palCol;
                        paladin_shield_count = 0;
                    }
                    break;

                //increase speed and attack based on missing health
                case EnemyClass.Assassin:
                    moveSpeed = assassin_original_speed + ((maxHealth * 5 - health * 6) / maxHealth) + 1; //as health approaches 0, speed approaches +6
                    realSpeed = moveSpeed;
                    attack = assassin_original_attack + ((maxHealth * enemyWave * assassinMultiplierSkill - health * (enemyWave * assassinMultiplierSkill - 1)) / maxHealth); //health go boo attack go woo
                    break;

                //ignores movement impairing effects and heals when hit by them
                case EnemyClass.Succubus:
                    if (slowed || stunned)
                    {
                        health += maxHealth * succubusHealPercent;
                        slowed = false;
                        stunned = false;
                        moveSpeed = realSpeed;

                        Color succCol = succubus_kiss.color;
                        succCol.a = 1;
                        succubus_kiss.color = succCol;
                        succubus_kiss_timer = .25f;

                        if (health > maxHealth)
                        {
                            health = maxHealth;
                        }
                    }
                    break;

                //takes less damage the farther the bullet travels
                case EnemyClass.Armorer:
                    float dist = Vector3.Distance(transform.position, bullet.tower.position);
                    float damageMod = (51 - dist) / 50;

                    if (bullet.tower.GetComponent<TowerAI>().Range / 2 > dist)
                    {
                        health -= (beforeHealth - health) * armorerBonusDamage;
                    }

                    health = beforeHealth - ((beforeHealth - health) * damageMod);
                    break;

                //takes no damage if the damage is less than 25% of its max health
                case EnemyClass.Warlock:
                    if ((beforeHealth - health) / maxHealth < warlockNegateThreshhold)
                    {
                        health = beforeHealth;
                        Color warCol = warlock_negater.color;
                        warCol.a = 1;
                        warlock_negater.color = warCol;
                        warlock_negater_timer = .25f;
                    }
                    break;
            }

            //Debug.LogError(health);

            Destroy(collision.gameObject);
            UpdateHealthBar(health, maxHealth);

            if (health <= 0)
            {
                //chaos challenge
                if (chaos_challenge && preChaosHealth > 0)
                {
                    challengeManager.chaosChallengeCounter++;

                    if (challengeManager.chaosChallengeCounter >= 10)
                    {
                        //challengeManager.SaveChallengeState("chaosskill");
                        chaos_challenge = false;
                    }
                }

                //fire challenge
                if (fire_challenge && enemyClass == EnemyClass.Knight && beforeHealth == maxHealth)
                {
                    //challengeManager.SaveChallengeState("fireskill");
                    fire_challenge = false;
                }

                //nature challenge
                if (nature_challenge && nature_challenge_pos == transform.position)
                {
                    //challengeManager.SaveChallengeState("natureskill");
                    nature_challenge = false;
                }

                //enemypedia
                switch (enemyClass)
                {
                    case EnemyClass.Cur:
                        if (!cur_found)
                        {
                            challengeManager.SaveEnemypediaToFile("cur");
                        }
                        break;

                    case EnemyClass.Knight:
                        if (!knight_found)
                        {
                            challengeManager.SaveEnemypediaToFile("knight");
                        }
                        break;

                    case EnemyClass.Monk:
                        if (!monk_found)
                        {
                            challengeManager.SaveEnemypediaToFile("monk");
                        }
                        break;

                    case EnemyClass.Assassin:
                        if (!assassin_found)
                        {
                            challengeManager.SaveEnemypediaToFile("assassin");
                        }
                        break;

                    case EnemyClass.Paladin:
                        if (!paladin_found)
                        {
                            challengeManager.SaveEnemypediaToFile("paladin");
                        }
                        break;

                    case EnemyClass.Armorer:
                        if (!armorer_found)
                        {
                            challengeManager.SaveEnemypediaToFile("armorer");
                        }
                        break;

                    case EnemyClass.Succubus:
                        if (!succubus_found)
                        {
                            challengeManager.SaveEnemypediaToFile("succubus");
                        }                        
                        break;

                    case EnemyClass.Warlock:
                        if (!warlock_found)
                        {
                            challengeManager.SaveEnemypediaToFile("warlock");
                        }                        
                        break;
                }

                //Debug.LogError("destroyed");
                //Debug.LogError(currency);
                AudioSource deathsound = Instantiate(enemyDeathSound).GetComponent<AudioSource>();
                deathsound.volume = deathSoundVolume;
                Destroy(transform.gameObject);
                currency.CurrentCurrency += (uint)Mathf.FloorToInt(killWorth);
            }
        }
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

    //set the stats of the enemy
    public void SetEnemyStats(float hlth, float atk, float spd, float wrth, int ewave, ElementTypes etype, EnemyClass eclass)
    {
        health = hlth;
        maxHealth = hlth;
        attack = atk;
        moveSpeed = spd;
        realSpeed = spd;
        killWorth = wrth;
        type = etype;
        enemyClass = eclass;
        enemyWave = ewave;

        switch (enemyClass)
        {
            case EnemyClass.Monk:
                moveSpeed *= monkSlowPercent;
                break;

            case EnemyClass.Paladin:
                paladin_shield = transform.GetChild(2).GetComponent<SpriteRenderer>();
                paladin_shield_count = 0;
                paladin_shield_timer = 0;
                break;

            case EnemyClass.Assassin:
                assassin_original_speed = spd;
                assassin_original_attack = atk;
                break;

            case EnemyClass.Succubus:
                succubus_kiss = transform.GetChild(2).GetComponent<SpriteRenderer>();
                succubus_kiss_timer = 0;
                break;

            case EnemyClass.Warlock:
                warlock_negater = transform.GetChild(2).GetComponent<SpriteRenderer>();
                warlock_negater_timer = 0;
                break;
        }

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

        switch (enemyClass)
        {
            case EnemyClass.Paladin:
                Color cp = transform.GetComponent<SpriteRenderer>().color;
                cp.a = 0;
                paladin_shield.color = cp;
                break;
            case EnemyClass.Succubus:
                Color cs = transform.GetComponent<SpriteRenderer>().color;
                cs.a = 0;
                succubus_kiss.color = cs;
                break;
            case EnemyClass.Warlock:
                Color cw = transform.GetComponent<SpriteRenderer>().color;
                cw.a = 0;
                warlock_negater.color = cw;
                break;
        }
    }

    //return the type fo the enemy
    public ElementTypes EnemyType
    {
        get { return type; }
    }

    //damage enemy, used by pulse attack so everything hacing to do with pulse attacks can be handled in the pulse attack script so nothing fucky happens
    public void TakeDamage(float damage, int statusEffect, ElementTypes[] types, Transform tower)
    {
        switch (type)
        {
            case ElementTypes.Lightning:
                if (types[0] == ElementTypes.Earth || types[1] == ElementTypes.Earth)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Fire:
                if (types[0] == ElementTypes.Water || types[1] == ElementTypes.Water)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Whimsical:
                if (types[0] == ElementTypes.Nature || types[1] == ElementTypes.Nature)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Order:
                if (types[0] == ElementTypes.Chaos || types[1] == ElementTypes.Chaos)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Nature:
                if (types[0] == ElementTypes.Fire || types[1] == ElementTypes.Fire)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Earth:
                if (types[0] == ElementTypes.Whimsical || types[1] == ElementTypes.Whimsical)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Chaos:
                if (types[0] == ElementTypes.Order || types[1] == ElementTypes.Order)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            case ElementTypes.Water:
                if (types[0] == ElementTypes.Lightning || types[1] == ElementTypes.Lightning)
                {
                    damage *= elementDamageMultiplier;
                }
                break;

            default:
                break;
        }

        float beforeHealth = health;
        health -= damage;
        float preChaosHealth = health;
        
        if (statusEffect == 3)
        {
            health = beforeHealth - (beforeHealth - health) * .1f;
            health -= maxHealth * .1f;
        }

        //do effects of special enemies
        switch (enemyClass)
        {
            //knight bonus damage skill
            case EnemyClass.Knight:
                health -= (beforeHealth - preChaosHealth) * knightBonusDamage;
                break;

            //gain a shield for 5 seconds after being hit 3 times
            case EnemyClass.Paladin:
                if (paladin_shield_timer <= 0)
                {
                    paladin_shield_count++;
                }
                else
                {
                    health = beforeHealth;
                }

                if (paladin_shield_count >= 3)
                {
                    paladin_shield_timer = paladinShieldMaxTime;
                    Color palCol = paladin_shield.color;
                    palCol.a = 1;
                    paladin_shield.color = palCol;
                    paladin_shield_count = 0;
                }
                break;

            //increase speed and attack based on missing health
            case EnemyClass.Assassin:
                moveSpeed = assassin_original_speed + ((maxHealth * 5 - health * 6) / maxHealth) + 1; //as health approaches 0, speed approaches +6
                realSpeed = moveSpeed;
                attack = assassin_original_attack + ((maxHealth * enemyWave * assassinMultiplierSkill - health * (enemyWave * assassinMultiplierSkill - 1)) / maxHealth); //health go boo attack go woo
                break;

            //ignores movement impairing effects and heals when hit by them
            case EnemyClass.Succubus:
                if (slowed || stunned)
                {
                    health += maxHealth * succubusHealPercent;
                    slowed = false;
                    stunned = false;
                    moveSpeed = realSpeed;

                    Color succCol = succubus_kiss.color;
                    succCol.a = 1;
                    succubus_kiss.color = succCol;
                    succubus_kiss_timer = .25f;

                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                }
                break;

            //takes less damage the farther the bullet travels
            case EnemyClass.Armorer:
                float dist = Vector3.Distance(transform.position, tower.position);
                float damageMod = (51 - dist) / 50;

                if (tower.GetComponent<TowerAI>().Range / 2 > dist)
                {
                    health -= (beforeHealth - health) * armorerBonusDamage;
                }

                health = beforeHealth - ((beforeHealth - health) * damageMod);
                break;

            //takes no damage if the damage is less than 20% of its max health
            case EnemyClass.Warlock:
                Color warCol = warlock_negater.color;
                warCol.a = 1;
                warlock_negater.color = warCol;
                warlock_negater_timer = .25f;

                if ((beforeHealth - health) / maxHealth < warlockNegateThreshhold)
                {
                    health = beforeHealth;
                }
                break;
        }
        
        UpdateHealthBar(health, maxHealth);

        if (health <= 0)
        {
            //chaos challenge
            if (chaos_challenge && preChaosHealth > 0)
            {
                challengeManager.chaosChallengeCounter++;

                if (challengeManager.chaosChallengeCounter >= 10)
                {
                    //challengeManager.SaveChallengeState("chaosskill");
                    chaos_challenge = false;
                }
            }

            //fire challenge
            if (fire_challenge && enemyClass == EnemyClass.Knight && beforeHealth == maxHealth)
            {
                //challengeManager.SaveChallengeState("fireskill");
                fire_challenge = false;
            }

            //nature challenge
            if (nature_challenge && nature_challenge_pos == transform.position)
            {
                //challengeManager.SaveChallengeState("natureskill");
                nature_challenge = false;
            }

            //enemypedia
            switch (enemyClass)
            {
                case EnemyClass.Cur:
                    if (!cur_found)
                    {
                        challengeManager.SaveEnemypediaToFile("cur");
                    }
                    break;

                case EnemyClass.Knight:
                    if (!knight_found)
                    {
                        challengeManager.SaveEnemypediaToFile("knight");
                    }
                    break;

                case EnemyClass.Monk:
                    if (!monk_found)
                    {
                        challengeManager.SaveEnemypediaToFile("monk");
                    }
                    break;

                case EnemyClass.Assassin:
                    if (!assassin_found)
                    {
                        challengeManager.SaveEnemypediaToFile("assassin");
                    }
                    break;

                case EnemyClass.Paladin:
                    if (!paladin_found)
                    {
                        challengeManager.SaveEnemypediaToFile("paladin");
                    }
                    break;

                case EnemyClass.Armorer:
                    if (!armorer_found)
                    {
                        challengeManager.SaveEnemypediaToFile("armorer");
                    }
                    break;

                case EnemyClass.Succubus:
                    if (!succubus_found)
                    {
                        challengeManager.SaveEnemypediaToFile("succubus");
                    }
                    break;

                case EnemyClass.Warlock:
                    if (!warlock_found)
                    {
                        challengeManager.SaveEnemypediaToFile("warlock");
                    }
                    break;
            }

            //Debug.LogError("destroyed");
            //Debug.LogError(currency);
            AudioSource deathsound = Instantiate(enemyDeathSound).GetComponent<AudioSource>();
            deathsound.volume = deathSoundVolume;
            Destroy(transform.gameObject);
            currency.CurrentCurrency += (uint)Mathf.FloorToInt(killWorth);
        }
    }

    //property for enemyWave
    public int EnemyWaveNumber
    {
        get { return enemyWave; }
    }

    //property for sound volume
    public float DeathSoundVolume
    {
        get { return deathSoundVolume; }
        set { deathSoundVolume = value; }
    }
}
