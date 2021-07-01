using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnderConstruction : MonoBehaviour
{
    //under construction object
    public GameObject underConstruction;
    private Image background;
    private Text text;
    public bool constructionOn;
    private float countDown = 2;

    // Start is called before the first frame update
    void Start()
    {
        background = underConstruction.transform.GetChild(0).GetComponent<Image>();
        text = underConstruction.transform.GetChild(1).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //pops up under construction text and then makes it fade away
        if (constructionOn)
        {
            underConstruction.SetActive(true);
            Color ic = background.color;
            Color tc = text.color;
            if (ic.a > 0)
            {
                ic.a = countDown;
                tc.a = countDown;
            }
            
            background.color = ic;
            text.color = tc;
            countDown -= Time.deltaTime;

            if (countDown <= 0)
            {
                constructionOn = false;
                countDown = 2;
                background.color = new Color(0, 0, 0, 1);
                text.color = new Color(1, 1, 1, 1);
                underConstruction.SetActive(false);
            }
        }
    }
}
