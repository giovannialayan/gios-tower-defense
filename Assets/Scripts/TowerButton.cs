using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    public GameObject towerObject;

    public Text currencyText;

    private float towerCost;

    public CurrencyManager currencyManager;

    // Start is called before the first frame update
    void Start()
    {
        towerCost = 10;
        UpdateCurrencyText(towerCost.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        //check if player has enough currency
        if (currencyManager.CurrentCurrency >= towerCost)
        {
            //remove currency required from player and increase tower cost
            currencyManager.CurrentCurrency -= Mathf.FloorToInt(towerCost);
            towerCost *= 2f;
            UpdateCurrencyText(towerCost.ToString());

            //spawn tower that follows player mouse until they click to drop it
            Instantiate(towerObject, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        }
        
    }

    //update currency text
    private void UpdateCurrencyText(string text)
    {
        currencyText.text = text;
        currencyText.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position + new Vector3(0, -.75f, 0));
    }
}
