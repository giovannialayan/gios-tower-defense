using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;

public class ElementShop : MonoBehaviour
{
    //currency manager reference
    public CurrencyManager currencyManager;

    //cost of elements
    private BigInteger elementCost;
    private float elementCostModifier;

    //prefabs for elements
    public GameObject fireItem;
    public GameObject lightningItem;
    public GameObject whimsicalItem;
    public GameObject natureItem;
    public GameObject earthItem;
    public GameObject waterItem;
    public GameObject orderItem;
    public GameObject chaosItem;

    //currency text
    public Text currencyText;

    //drop down variables
    private bool showDropDown = false;
    public GameObject shopMenu;
    public Text towerText;

    //list of elements in inventory
    private List<Transform> inventory;
    private UnityEngine.Vector3 inventoryZero;

    //gamemanager
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        elementCost = 5;
        elementCostModifier = 1.5f;
        UpdateCurrencyText(elementCost);
        inventoryZero = new UnityEngine.Vector3(3.265f, 10.32f, 0);
        inventory = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BuyFireItem()
    {
        BuyElement(ElementTypes.Fire, fireItem);
    }

    public void BuyLightningItem()
    {
        BuyElement(ElementTypes.Lightning, lightningItem);
    }

    public void BuyWhimsicalItem()
    {
        BuyElement(ElementTypes.Whimsical, whimsicalItem);
    }

    public void BuyNatureItem()
    {
        BuyElement(ElementTypes.Nature, natureItem);
    }

    public void BuyEarthItem()
    {
        BuyElement(ElementTypes.Earth, earthItem);
    }

    public void BuyWaterItem()
    {
        BuyElement(ElementTypes.Water, waterItem);
    }

    public void BuyOrderItem()
    {
        BuyElement(ElementTypes.Order, orderItem);
    }

    public void BuyChaosItem()
    {
        BuyElement(ElementTypes.Chaos, chaosItem);
    }

    //buy element and put it in inventory
    private void BuyElement(ElementTypes element, GameObject elementObject)
    {
        //check if player has enough currency
        if (currencyManager.CurrentCurrency >= elementCost && inventory.Count < 18)
        {
            //remove currency required from player and increase element cost
            currencyManager.CurrentCurrency -= elementCost;
            elementCost = new BigInteger((double)elementCost * elementCostModifier);
            UpdateCurrencyText(elementCost);

            //spawn element in the correct position in the inventory
            int invlen = inventory.Count;
            inventory.Add(Instantiate(elementObject, inventoryZero, UnityEngine.Quaternion.identity).transform);
            inventory[invlen].position = inventoryZero + new UnityEngine.Vector3(invlen * 1.094f, (invlen / 6) * -1.094f, 0);
            if (invlen >= 6)
            {
                inventory[invlen].position = new UnityEngine.Vector3(inventoryZero.x + (invlen % 6) * 1.094f, inventory[invlen].position.y, 0);
            }

            //Debug.Log(inventoryZero.x + "+ ((" + invlen + " - 1) % 6) * 1.094 = " + (inventoryZero.x + (invlen % 6) * 1.094f));
            //Debug.Log(invlen + " % 6 = " + invlen % 6);

            //change origin and element in ItemPickUp
            ItemPickUp elementItemPickUp = inventory[invlen].GetComponent<ItemPickUp>();
            elementItemPickUp.Origin = inventory[invlen].position;
            elementItemPickUp.Element = element;
            elementItemPickUp.Shop = gameObject.GetComponent<ElementShop>();
            elementItemPickUp.gamemanager = gameManager;

        }
    }

    //change text
    private void UpdateCurrencyText(BigInteger amount)
    {
        currencyText.text = "element cost: ";

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

    //shows or hides the drop down shop menu
    public void ToggleDropDown()
    {
        if (showDropDown)
        {
            shopMenu.SetActive(false);
            showDropDown = false;
            currencyText.color = new Color(1, 1, 1, 0);
        }
        else
        {
            shopMenu.SetActive(true);
            showDropDown = true;
            currencyText.color = new Color(1, 1, 1, 1);
            towerText.color = new Color(1, 1, 1, 0);
        }
    }

    //property so i can remove items from the inventory
    public void RemoveItem(Transform itemTrans)
    {
        inventory.Remove(itemTrans);

        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i].position = inventoryZero + new UnityEngine.Vector3(i * 1.094f, (i / 6) * -1.094f, 0);
            if (i >= 6)
            {
                inventory[i].position = new UnityEngine.Vector3(inventoryZero.x + (i % 6) * 1.094f, inventory[i].position.y, 0);
            }

            //change origin
            inventory[i].GetComponent<ItemPickUp>().Origin = inventory[i].position;
        }
    }
}
