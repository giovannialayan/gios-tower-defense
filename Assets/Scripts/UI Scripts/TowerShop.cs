using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerShop : MonoBehaviour
{
    public Image selectedTower;
    private Sprite thisTower;

    private bool showDropDown = false;
    public GameObject shopMenu;
    public Text elementText;
    public Text currencyText;

    //tower prefabs
    public GameObject towerButton;
    public GameObject fireTowerObj;
    public GameObject lightningTowerObj;
    public GameObject whimsicalTowerObj;
    public GameObject waterTowerObj;
    public GameObject natureTowerObj;
    public GameObject orderTowerObj;
    public GameObject chaosTowerObj;
    public GameObject earthTowerObj;

    // Start is called before the first frame update
    void Start()
    {
        thisTower = transform.GetComponent<Image>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //changes the selected tower to lightning
    public void SelectLightningTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = lightningTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Lightning;
    }

    //changes the selected tower to lightning
    public void SelectFireTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = fireTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Fire;
    }

    //changes the selected tower to lightning
    public void SelectWhimsicalTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = whimsicalTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Whimsical;
    }

    //changes the selected tower to lightning
    public void SelectNatureTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = natureTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Nature;
    }

    //changes the selected tower to lightning
    public void SelectEarthTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = earthTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Earth;
    }

    //changes the selected tower to lightning
    public void SelectChaosTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = chaosTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Chaos;
    }

    //changes the selected tower to lightning
    public void SelectOrderTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = orderTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Order;
    }

    //changes the selected tower to lightning
    public void SelectWaterTower()
    {
        selectedTower.sprite = thisTower;
        towerButton.GetComponent<TowerButton>().PrefabToInstantiate = waterTowerObj;
        towerButton.GetComponent<TowerButton>().TowerTypeToCreate = ElementTypes.Water;
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
            elementText.color = new Color(1, 1, 1, 0);
        }
    }
}
