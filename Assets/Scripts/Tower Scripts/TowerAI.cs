using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerAI : MonoBehaviour
{
    //range of tower
    //private float rangeRadius;

    //range view of tower
    private GameObject rangeView;
    private Transform enemyInRange;
    private BoxCollider2D towerCol;

    //reference to MousePickUp script
    private MousePickUp mousePickUp;

    //bullet object
    public GameObject bullet;
    //private float attackDamage;

    //tower attack speed
    //private float attackSpeed;
    private float timeSinceAttack;

    //tower UI
    private GameObject towerUI;
    private bool towerInfoOn;
    private float maxWidth = 20;
    private float minWidth = -20;
    private float maxHeight = 11;
    private float minHeight = -11;
    private float infoWidth = 10;
    private float infoHeight = 10;
    private bool infoSet;

    //upgrades
    private CurrencyManager currency;
    private ulong attackUpgradeCost;
    private ulong ASUpgradeCost;
    private ulong rangeUpgradeCost;
    private int ASUpgradeNumber;

    //tower stats
    private BaseTower stats;

    //upgrade modifiers
    private float attackMod; // attackMod is added to attack every buy
    private float attackSpeedMod; // attackSpeedMod is multiplied by attackSpeed every buy
    private float rangeMod; // rangeMod is added to range every buy

    //special variables for specific element types
    private List<Transform> multiEnemiesInRange = new List<Transform>();
    private GameObject ord_pulseObject;
    private int chaosModOn = 0;
    private int whims_maxTargets;
    private bool hasWater;
    private int nature_attackNumber;

    //gamemanager reference
    public GameManager gamemanager { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gamemanager.IsPaused)
        {
            return;
        }

        //Debug.Log(enemyInRange + ", " + timeSinceAttack);

        if ((enemyInRange != null || multiEnemiesInRange.Count > 0 || stats.ImmutableElement == ElementTypes.Chaos || stats.ImmutableElement == ElementTypes.Water) && mousePickUp.ItemPlaced)
        {
            switch (stats.ImmutableElement)
            {
                case ElementTypes.Chaos:
                    if (timeSinceAttack >= stats.AttackSpeed)
                    {
                        SetChaosOnAdjacentTowers();
                        timeSinceAttack = 0;
                    }
                    else
                    {
                        timeSinceAttack += Time.deltaTime;
                    }
                    break;
                case ElementTypes.Water:
                    if (timeSinceAttack >= stats.AttackSpeed)
                    {
                        SetWaterOnTowers();
                        timeSinceAttack = 0;
                    }
                    else
                    {
                        timeSinceAttack += Time.deltaTime;
                    }
                    break;
                case ElementTypes.Order:
                    PulseAttack();
                    break;
                case ElementTypes.Whimsical:
                    MultiAttack();
                    break;
                case ElementTypes.Nature:
                    if (nature_attackNumber < 2)
                    {
                        ShootAt(enemyInRange, 0);
                        nature_attackNumber++;
                    }
                    else
                    {
                        ShootAt(enemyInRange, 2);
                        nature_attackNumber = 0;
                    }
                    break;
                case ElementTypes.Earth:
                    ShootAt(enemyInRange, 1);
                    break;
                default:
                    ShootAt(enemyInRange, 0);
                    break;
            }
        }
        else if (timeSinceAttack < stats.AttackSpeed && stats.ImmutableElement != ElementTypes.Order && stats.ImmutableElement != ElementTypes.Chaos && stats.ImmutableElement != ElementTypes.Water)
        {
            timeSinceAttack += Time.deltaTime;
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

        if (mousePickUp.ItemPlaced && !infoSet)
        {
            //make sure the tower info is displayed to the left of the tower if it goes off screen to the right
            if (maxWidth - (transform.position.x + infoWidth + 1) < 0)
            {
                towerUI.transform.localPosition = new Vector3(towerUI.transform.localPosition.x * -1, towerUI.transform.localPosition.y);
            }

            if (maxHeight - (transform.position.y + infoHeight / 2) < 0)
            {
                towerUI.transform.localPosition = new Vector3(towerUI.transform.localPosition.x, towerUI.transform.localPosition.y - 2);
            }

            if (Mathf.Abs(minHeight) - (Mathf.Abs(transform.position.y) + infoHeight / 2) < 0)
            {
                towerUI.transform.localPosition = new Vector3(towerUI.transform.localPosition.x, towerUI.transform.localPosition.y + 1);
            }

            infoSet = true;
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
        //dont fuck shit up if its a chaos tower
        if (radius == 0)
        {
            return;
        }

        if (stats.ImmutableElement == ElementTypes.Water)
        {
            radius += .5f;
        }

        rangeView.transform.localScale = new Vector3(radius * 2, radius * 2, 1);

        if (stats.ImmutableElement == ElementTypes.Order)
        {
            ord_pulseObject.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        }
        else if (stats.ImmutableElement == ElementTypes.Whimsical)
        {
            whims_maxTargets = Mathf.FloorToInt(stats.Range / 3f);
        }
    }

    //shoot bullets at enemies when they enter range
    private void ShootAt(Transform target, int statusEffect)
    {
        if (timeSinceAttack >= stats.AttackSpeed)
        {
            BulletAI newBullet = Instantiate(bullet, transform.position, Quaternion.identity, target.transform).GetComponent<BulletAI>();
            newBullet.BulletDamage = stats.Attack;
            ElementTypes[] ttypes = { stats.Element, stats.ImmutableElement };
            newBullet.BulletType = ttypes;
            newBullet.StatusEffect = statusEffect + chaosModOn;
            timeSinceAttack = 0;
        }
        else
        {
            timeSinceAttack += Time.fixedDeltaTime;
        }
    }

    //change target to an enemy in range
    public void ChangeTarget(Transform target)
    {
        if (target != enemyInRange)
        {
            timeSinceAttack = stats.AttackSpeed;
        }
        enemyInRange = target;
    }

    //check for click on tower
    private void OnMouseDown()
    {
        if (mousePickUp.ItemPlaced && infoSet && !gamemanager.IsPaused && stats.ImmutableElement != ElementTypes.Chaos)
        {
            ToggleTowerInfo();
        }
    }

    //toggle showing the tower's information and upgrades
    private void ToggleTowerInfo()
    {
        if (towerInfoOn)
        {
            towerInfoOn = false;

            towerUI.SetActive(towerInfoOn);
        }
        else
        {
            UpdateTowerInfo();

            towerInfoOn = true;

            towerUI.SetActive(towerInfoOn);
        }
    }

    //update tower info text
    public void UpdateTowerInfo()
    {
        towerUI.transform.GetChild(1).GetComponent<Text>().text = string.Format("{1} - Attack: {0}", stats.Attack, attackUpgradeCost);
        towerUI.transform.GetChild(2).GetComponent<Text>().text = string.Format("{1} - ASpeed: {0:F2}", stats.AttackSpeed, ASUpgradeCost);
        towerUI.transform.GetChild(3).GetComponent<Text>().text = string.Format("{1} - Range: {0}", stats.Range, rangeUpgradeCost);
        towerUI.transform.GetChild(4).GetComponent<Text>().text = string.Format("{0} - Upgrade All", attackUpgradeCost + ASUpgradeCost + rangeUpgradeCost);
    }

    //increase attack
    public void UpgradeAttack()
    {
        if (currency.CurrentCurrency >= attackUpgradeCost)
        {
            currency.CurrentCurrency -= attackUpgradeCost;
            attackUpgradeCost += attackUpgradeCost / 2;

            stats.Attack += attackMod;
            
            UpdateTowerInfo();
        }
    }

    //increase attack speed
    public void UpgradeAttackSpeed()
    {
        if (currency.CurrentCurrency >= ASUpgradeCost)
        {
            currency.CurrentCurrency -= ASUpgradeCost;
            ASUpgradeCost += ASUpgradeCost / 2;

            ASUpgradeNumber++;
            stats.AttackSpeed = GetAttackSpeedValue(attackSpeedMod, ASUpgradeNumber);

            UpdateTowerInfo();
        }
    }

    //increase range
    public void UpgradeRange()
    {
        if (currency.CurrentCurrency >= rangeUpgradeCost)
        {
            currency.CurrentCurrency -= rangeUpgradeCost;
            rangeUpgradeCost += rangeUpgradeCost / 2;

            stats.Range += rangeMod;
            ChangeTowerRange(stats.Range);

            UpdateTowerInfo();
        }
    }

    //increases all of the above
    public void UpgradeAll()
    {
        if (currency.CurrentCurrency >= attackUpgradeCost + ASUpgradeCost + rangeUpgradeCost)
        {
            currency.CurrentCurrency -= attackUpgradeCost + ASUpgradeCost + rangeUpgradeCost;
            attackUpgradeCost += attackUpgradeCost / 2;
            ASUpgradeCost += ASUpgradeCost / 2;
            rangeUpgradeCost += rangeUpgradeCost / 2;

            stats.Attack += attackMod;
            ASUpgradeNumber++;
            stats.AttackSpeed = GetAttackSpeedValue(attackSpeedMod, ASUpgradeNumber);
            stats.Range += rangeMod;
            ChangeTowerRange(stats.Range);

            UpdateTowerInfo();
        }
    }

    //creates a tower based on a type
    public void CreateTower(ElementTypes type)
    {
        switch (type)
        {
            case ElementTypes.Lightning:
                stats = new BaseTower(1, 1, 5, ElementTypes.Lightning);
                attackMod = .1f;
                attackSpeedMod = 2;
                rangeMod = 1;
                break;

            case ElementTypes.Fire:
                stats = new BaseTower(5, 5, 5, ElementTypes.Fire);
                attackMod = 2;
                attackSpeedMod = 10;
                rangeMod = 1;
                break;

            case ElementTypes.Whimsical:
                stats = new BaseTower(2, 3, 10, ElementTypes.Whimsical);
                attackMod = .5f;
                attackSpeedMod = 6;
                rangeMod = 3;
                multiEnemiesInRange = new List<Transform>();
                whims_maxTargets = Mathf.FloorToInt(stats.Range / 3f);
                break;

            case ElementTypes.Order:
                stats = new BaseTower(2, 2, 5, ElementTypes.Order);
                attackMod = .5f;
                attackSpeedMod = 4;
                rangeMod = .5f;
                ord_pulseObject = transform.GetChild(3).gameObject;
                multiEnemiesInRange = new List<Transform>();
                break;

            case ElementTypes.Nature:
                stats = new BaseTower(1.5f, 4, 5, ElementTypes.Nature);
                attackMod = .25f;
                attackSpeedMod = 8;
                rangeMod = 1;
                nature_attackNumber = 0;
                break;

            case ElementTypes.Earth:
                stats = new BaseTower(1, 2, 5, ElementTypes.Earth);
                attackMod = .5f;
                attackSpeedMod = 4;
                rangeMod = 1;
                break;

            case ElementTypes.Chaos:
                stats = new BaseTower(0, 1, 0, ElementTypes.Chaos);
                attackMod = 0;
                attackSpeedMod = 0;
                rangeMod = 0;
                break;

            case ElementTypes.Water:
                stats = new BaseTower(0, 1, 3, ElementTypes.Water);
                attackMod = 0;
                attackSpeedMod = 0;
                rangeMod = 1;
                break;

            default:
                Debug.Log("default ocurred in CreateTower switch, fuck");
                stats = new BaseTower(5, 5, 5, ElementTypes.Error);
                attackMod = .1f;
                attackSpeedMod = 10;
                rangeMod = 1;
                break;
        }

        //set base range of tower
        rangeView = transform.GetChild(0).gameObject;
        ChangeTowerRange(stats.Range);
        towerCol = transform.GetComponent<BoxCollider2D>();

        mousePickUp = GetComponent<MousePickUp>();

        enemyInRange = null;

        //attack speed set up
        timeSinceAttack = stats.AttackSpeed;

        //tower UI
        towerUI = transform.GetChild(1).gameObject;
        towerUI.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        towerInfoOn = false;
        infoSet = false;

        //currency manager reference
        currency = GameObject.FindGameObjectWithTag("money").GetComponent<CurrencyManager>();
        attackUpgradeCost = 2;
        ASUpgradeCost = 2;
        rangeUpgradeCost = 2;
        ASUpgradeNumber = 1;
    }

    //multi attack for whimsical tower
    private void MultiAttack()
    {
        if (timeSinceAttack >= stats.AttackSpeed)
        {
            for (int i = 0; i < multiEnemiesInRange.Count; i++)
            {
                if (multiEnemiesInRange.Count > 0 && multiEnemiesInRange[i] != null && i < whims_maxTargets)
                {
                    BulletAI newBullet = Instantiate(bullet, transform.position, Quaternion.identity, multiEnemiesInRange[i].transform).GetComponent<BulletAI>();
                    newBullet.BulletDamage = stats.Attack;
                    ElementTypes[] ttypes = { stats.Element, stats.ImmutableElement };
                    newBullet.BulletType = ttypes;
                    newBullet.StatusEffect = chaosModOn;
                }
                else if(multiEnemiesInRange.Count > 0 && i < whims_maxTargets)
                {
                    multiEnemiesInRange.RemoveAt(i);
                    i--;
                }
            }

            timeSinceAttack = 0;
        }
        else
        {
            timeSinceAttack += Time.fixedDeltaTime;
        }
    }

    //pulse attack for order tower
    private void PulseAttack()
    {
        if (timeSinceAttack >= stats.AttackSpeed)
        {
            ord_pulseObject.SetActive(true);
            PulseAttack pulse = ord_pulseObject.GetComponent<PulseAttack>();

            pulse.StartPulse();

            ElementTypes[] ttypes = { stats.Element, stats.ImmutableElement };
            int numEnemies = multiEnemiesInRange.Count;
            float pulseDamage = stats.Attack / numEnemies;

            for (int i = 0; i < numEnemies; i++)
            {
                if (multiEnemiesInRange[i] != null)
                {
                    multiEnemiesInRange[i].GetComponent<EnemyAI>().TakeDamage(pulseDamage, chaosModOn, ttypes);
                }
            }

            multiEnemiesInRange.Clear();

            timeSinceAttack = 0;
        }
        else
        {
            timeSinceAttack += Time.fixedDeltaTime;
        }
    }

    //immutable type of this tower
    public ElementTypes ImmutableType
    {
        get { return stats.ImmutableElement; }
    }

    //secondary type of this tower
    public ElementTypes SecondaryType
    {
        get { return stats.Element; }
        set { stats.Element = value; }
    }

    //multiple enemies to attack
    public List<Transform> MultiEnemiesInRange
    {
        get { return multiEnemiesInRange; }
        set { multiEnemiesInRange = value; }
    }

    //chaos tower on = 3, off = 0, to be changed by chaos tower
    public int ChaosMod
    {
        get { return chaosModOn; }
        set { chaosModOn = value; }
    }

    //property so water tower can influence towers
    public bool HasWater
    {
        get { return stats.Watered; }
        set { stats.Watered = value; }
    }

    //chaos tower gives adjacent towers chaosmod
    private void SetChaosOnAdjacentTowers()
    {
        //send 2d raycast in 4 cardinal directions
        RaycastHit2D[] hits = new RaycastHit2D[4];
        hits[0] = Physics2D.Raycast(transform.position, Vector2.up, 1);
        hits[1] = Physics2D.Raycast(transform.position, Vector2.down, 1);
        hits[2] = Physics2D.Raycast(transform.position, Vector2.left, 1);
        hits[3] = Physics2D.Raycast(transform.position, Vector2.right, 1);
        TowerAI hitTower;

        //foreach hit if the hit isnt null check its distance to this
        for (int i = 0; i < hits.Length; i++)
        {
            if (
                hits[i].collider != null && //isnt null
                hits[i].collider.tag == "structure" && //is a structure
                hits[i].collider.TryGetComponent<TowerAI>(out hitTower)//has towerai script
            )
            {
                //congrats, you passed, set chaosmod to 3
                hitTower.ChaosMod = 3;
                //Debug.Log(hitTower.name);
            }
        }

        //later when i let the player delete towers do the same thing to set chaosmod to 0, 
        //if there is another chaos tower adjacent to the tower that chaosmod was removed from it will give it back after max 1 second
    }

    //water tower gives towers in range buffs
    private void SetWaterOnTowers()
    {
        //send raycasts vertically throughout every column within the range of the water tower
        int range = Mathf.FloorToInt(stats.Range * 2 + 1);
        RaycastHit2D[][] hits = new RaycastHit2D[range][];
        int halfRange = range / 2;
        for (int i = -halfRange; i <= halfRange; i++)
        {
            hits[i + halfRange] = Physics2D.RaycastAll(transform.position + new Vector3(i, halfRange, 0), Vector2.down, range - 1);
            //Debug.DrawRay(transform.position + new Vector3(i, halfRange, 0), Vector2.down * (range - 1), Color.white, 1);
        }

        TowerAI hitTower;

        //give water to every tower hit by the rays
        for (int i = 0; i < hits.Length; i++)
        {
            for (int j = 0; j < hits[i].Length; j++)
            {
                if (
                    hits[i] != null && //there is an array here
                    hits[i][j].collider != null && //there is a collider here
                    hits[i][j].collider.tag == "structure" && //is a structure
                    hits[i][j].collider.TryGetComponent<TowerAI>(out hitTower) && //has towerai script
                    hits[i][j].collider.gameObject != gameObject // make sure it aint this one
                )
                {
                    hitTower.hasWater = true;
                    hitTower.UpdateTowerInfo();
                    //Debug.Log(hitTower.name);
                }

            }
        }
    }

    //hyperbolic curve for attack speed
    private float GetAttackSpeedValue(float attackspeedmod, int x)
    {
        return (1 * attackspeedmod) / (x + 1);
    }
}
