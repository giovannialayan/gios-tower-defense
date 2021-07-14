using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject skillTextBox;

    public MainMenu mainmenu;

    //skill tree variables
    public string skillName = "";
    public int skillCostIncrement = 1; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        skillTextBox.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillTextBox.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tag == "skill tree")
        {
            mainmenu.BuySkillTree(skillName, skillCostIncrement);
        }
    }
}
