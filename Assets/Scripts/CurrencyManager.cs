using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO
//figure out how to multiply a number by a big number
//make an overload that lets you add and substract a big number by another big number
//do the same thing for multiplication and division
//figure out how to make it work with other scripts (probably just use a function to translate instead of using a property)

public class CurrencyManager : MonoBehaviour
{
    //currency variables
    //private BigNumber maxCurrency;
    //private BigNumber currentCurrency;

    private ulong maxCurrency;
    private ulong currentCurrency;
    private float currencyTick;
    private uint maxCurrencyModifier;

    private float currencyGainRate;
    private float percentGainPerSecond = .01f;

    // B A R
    public Image barBackground;
    public Image currencyBar;
    private float barFullSize;
    private float barHeight;
    public Text currencyText;

    //gamemanager
    public GameManager gameManager;

    private void Start()
    {
        //maxCurrency = new BigNumber();
        //maxCurrency.IncreaseBy(100);
        //currentCurrency = new BigNumber();

        maxCurrency = (uint)gameManager.SkillTree["money2"];
        currentCurrency = 0;
        currencyTick = 0;
        maxCurrencyModifier = 2;
        percentGainPerSecond = gameManager.SkillTree["money1"];

        SetGainRate(percentGainPerSecond);

        barFullSize = barBackground.rectTransform.rect.width;
        barHeight = barBackground.rectTransform.rect.height;
    }

    private void Update()
    {
        if (gameManager.IsPaused)
        {
            return;
        }

        //increase currentCurrency by gain rate
        //currentCurrency.IncreaseBy(Mathf.FloorToInt(currencyGainRate));

        ////check if current currency exceeds max currency
        //if (currentCurrency.IsGreaterThan(maxCurrency))
        //{
        //    maxCurrency.MultiplyBy(2);
        //    IncreaseMaxCurrency(maxCurrency);
        //}

        //increase currentCurrency by gain rate
        currencyTick += currencyGainRate;
        if (currencyTick > 1)
        {
            currentCurrency += (uint)Mathf.FloorToInt(currencyTick);
            currencyTick -= (uint)Mathf.FloorToInt(currencyTick);
        }

        //check if current currency exceeds max currency
        if (currentCurrency > maxCurrency)
        {
            IncreaseMaxCurrency(maxCurrency * maxCurrencyModifier);
        }

        //update currency bar
        UpdateCurrencyBar(currentCurrency, maxCurrency);
    }

    //property for the current currency amount
    public ulong CurrentCurrency
    {
        get { return currentCurrency; }
        set { currentCurrency = value; }
    }

    private void UpdateCurrencyBar(ulong amount, ulong cap)
    {
        //change bar
        currencyBar.rectTransform.sizeDelta = new Vector2(((float)amount / cap * barFullSize), barHeight);

        //change text
        if (amount < 1000000000)
        {
            currencyText.text = amount.ToString() + " er";
        }
        else
        {
            currencyText.text = amount.ToString("0." + new string('0', 2) + "e0") + " er";
        }
    }

    //increase max currency
    //public void IncreaseMaxCurrency(BigNumber increase)
    //{
    //    maxCurrency.IncreaseBy(increase);

    //    SetGainRate(percentGainPerSecond);
    //}

    //increase max currency
    public void IncreaseMaxCurrency(ulong increase)
    {
        maxCurrency += increase;

        SetGainRate(percentGainPerSecond);
    }

    //calculate currency gain rate
    public void SetGainRate(float percentGain)
    {
        currencyGainRate = maxCurrency * percentGain * Time.fixedDeltaTime;
    }
}
