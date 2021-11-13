using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Numerics;

public class TowerButton : MonoBehaviour
{
    private GameObject towerObject;
    private ElementTypes towerType;

    public Text currencyText;

    private BigInteger towerCost;
    private float towerCostModifier;

    public CurrencyManager currencyManager;

    //default tower is fire
    public GameObject defaultTower;

    //gamemanager reference
    public GameManager gameManager;

    //tower limit
    private int towerLimit = 0;
    private int currentNumTowers = 0;
    public Tilemap tilemap;
    public Sprite wallTile;

    // Start is called before the first frame update
    void Start()
    {
        towerCost = (int)gameManager.SkillTree["money1"];
        towerCostModifier = gameManager.SkillTree["money4"];
        UpdateCurrencyText(towerCost);
        towerObject = defaultTower;
        towerType = ElementTypes.Fire;
        towerLimit = GetNumberOfTilesWithSprite(wallTile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        //check if player has enough currency
        //if (currencyManager.CurrentCurrency >= towerCost)
        //{
        //    //remove currency required from player and increase tower cost
        //    currencyManager.CurrentCurrency -= (uint)Mathf.FloorToInt(towerCost);
        //    towerCost *= towerCostModifier;
        //    UpdateCurrencyText(towerCost);

        //    //spawn tower that follows player mouse until they click to drop it
        //    GameObject newTower = Instantiate(towerObject, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        //    newTower.GetComponent<TowerAI>().CreateTower(towerType);
        //}
        
    }

    //change text
    private void UpdateCurrencyText(BigInteger amount)
    {
        currencyText.text = "tower cost: ";

        if (amount < 1000000000)
        {
            currencyText.text += amount.ToString("F0");
        }
        else
        {
            currencyText.text += amount.ToString("0." + new string('0', 2) + "e0");
        }

        //currencyText.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position + new Vector3(0, -.75f, 0));
    }

    public GameObject PrefabToInstantiate
    {
        get { return towerObject; }
        set { towerObject = value; }
    }

    public ElementTypes TowerTypeToCreate
    {
        get { return towerType; }
        set { towerType = value; }
    }

    //i think i should be using a raycast that ignores the tower image on top of the tower button 
    //but instead im just making the image a button and using the tower button object's script in the button component.
    //i just need to make sure a buy cant happen twice at the same time otherwise ill have to use the raycast or make a seperate button sprite for all the towers
    //and swap the sprite of the tower button instead of using a second image on top of the tower button
    //haha past me, you fucking idiot, it does happen twice so my fix is to make only the tower image the button
    public void DirtyButton()
    {
        //check if player has enough currency and if the towerlimit hasnt been reached
        if (currencyManager.CurrentCurrency >= towerCost && currentNumTowers < towerLimit)
        {
            //remove currency required from player and increase tower cost
            currencyManager.CurrentCurrency -= towerCost;
            towerCost = new BigInteger((double)towerCost * towerCostModifier);
            UpdateCurrencyText(towerCost);

            //spawn tower that follows player mouse until they click to drop it
            GameObject newTower = Instantiate(towerObject, new UnityEngine.Vector3(transform.position.x, transform.position.y, transform.position.z), UnityEngine.Quaternion.identity);
            newTower.GetComponent<TowerAI>().CreateTower(towerType);
            newTower.GetComponent<MousePickUp>().gamemanager = gameManager;
            if (towerType != ElementTypes.Chaos && towerType != ElementTypes.Water)
            {
                newTower.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false;
            }

            currentNumTowers++;
        }
    }

    private int GetNumberOfTilesWithSprite(Sprite tileSprite)
    {
        int amount = 0;
        //loop through all tiles
        BoundsInt bounds = tilemap.cellBounds;
        foreach(Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = tilemap.GetTile<Tile>(pos);
            if (tile != null)
            {
                if (tile.sprite == tileSprite)
                {
                    amount++;
                }
            }
        }
        
        return amount;
    }
}
