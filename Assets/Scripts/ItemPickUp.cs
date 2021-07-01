using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private bool itemPlaced;

    private float snapVal = 1f;

    private Vector3 origin;

    private ElementTypes element;

    public Sprite icon;

    private ElementShop shop;

    //gamemanager
    public GameManager gamemanager { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        itemPlaced = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!itemPlaced && !gamemanager.IsPaused)
        {
            //put the object on the mouse
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            transform.position = Camera.main.ScreenToWorldPoint(mousePos);

            //snap the object to the nearest grid point
            transform.position = GetSnapPosition(transform.position, snapVal);
        }
    }

    //snap transform to grid
    private Vector3 GetSnapPosition(Vector2 originalPos, float snap = 1f)
    {
        return new Vector3(
            GetSnapValue(originalPos.x, snap),
            GetSnapValue(originalPos.y, snap),
            10
        );
    }

    //change value according to snap
    private float GetSnapValue(float value, float snap = .5f)
    {
        return (!Mathf.Approximately(snap, 0f)) ? Mathf.RoundToInt(value / snap) * snap : value;
    }

    //get all colliders in the scene and check if any of them contain any of the points given
    private GameObject GetStructureUnderHere(Bounds boundCheck)
    {
        GameObject[] structures = GameObject.FindGameObjectsWithTag("structure");

        foreach (GameObject structure in structures)
        {
            if (structure.GetComponent<BoxCollider2D>().bounds.Intersects(boundCheck) && structure.name != "base" && structure.name != "spawner")
            {
                return structure;
            }
        }

        return null;
    }

    private void OnMouseDown()
    {
        if (gamemanager.IsPaused)
        {
            return;
        }

        GameObject possibleTower = GetStructureUnderHere(new Bounds(transform.position, new Vector3(.5f, .5f, 1)));
        if (itemPlaced)
        {
            itemPlaced = false;
        }
        else if(possibleTower != null)
        {
            //get tower icon object
            GameObject iconObj = possibleTower.transform.GetChild(2).gameObject;

            //put icon on tower
            if (!iconObj.activeSelf)
            {
                iconObj.SetActive(true);
            }

            iconObj.GetComponent<SpriteRenderer>().sprite = icon;

            //add new element type to secondary element
            possibleTower.GetComponent<TowerAI>().SecondaryType = element;

            //destroy item object
            Destroy(gameObject);
            shop.RemoveItem(gameObject.transform);

            //Debug.Log("placed on tower at " + possibleTower.name);
        }
        else
        {
            transform.position = origin;
            itemPlaced = true;
        }
    }

    //property for the original position in inventory
    public Vector3 Origin
    {
        get { return origin; }
        set { origin = value; }
    }

    //property for element to be added
    public ElementTypes Element
    {
        get { return element; }
        set { element = value; }
    }

    //property for shop
    public ElementShop Shop
    {
        get { return shop; }
        set { shop = value; }
    }
}
