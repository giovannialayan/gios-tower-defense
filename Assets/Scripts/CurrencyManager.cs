using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    //currency variables
    private long maxCurrency;
    private double currentCurrency;
    private double currencyGainRate;

    // B A R
    public Image barBackground;
    public Image currencyBar;
    private float barFullSize;
    private float barHeight;
    public Text currencyText;

    private void Start()
    {
        maxCurrency = 100;
        currentCurrency = 0;
        SetGainRate();

        barFullSize = barBackground.rectTransform.rect.width;
        barHeight = barBackground.rectTransform.rect.height;
    }

    private void Update()
    {
        //increase currentCurrency by gain rate
        currentCurrency += currencyGainRate;

        //check if current currency exceeds max currency
        if (currentCurrency >= maxCurrency)
        {
            IncreaseMaxCurrency(maxCurrency * 2);
        }

        //update currency bar
        UpdateCurrencyBar(currentCurrency, maxCurrency);
    }

    //property for the current currency amount
    public int CurrentCurrency
    {
        get { return (int)currentCurrency; }
        set { currentCurrency = value; }
    }

    private void UpdateCurrencyBar(double amount, long cap)
    {
        //change bar
        currencyBar.rectTransform.sizeDelta = new Vector2((float)(amount / cap * barFullSize), barHeight);

        //change text
        currencyText.text = ((int)amount).ToString();
    }

    //increase max currency
    public void IncreaseMaxCurrency(long increase)
    {
        maxCurrency += increase;

        SetGainRate();
    }

    //calculate currency gain rate
    public void SetGainRate()
    {
        currencyGainRate = maxCurrency * .02f * Time.fixedDeltaTime;
    }
}
