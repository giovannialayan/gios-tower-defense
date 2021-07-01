using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseManager : MonoBehaviour
{
    //living variables
    private float health;
    private float maxHealth;
    private bool isAlive = true;

    //health bar
    public Image healthBar;
    public Image barBackground;
    public Text healthText;
    private float barFullSize;
    private float barHeight;

    // Start is called before the first frame update
    void Start()
    {
        maxHealth = 10;
        health = 10;
        isAlive = true;

        //bar setup
        GameObject healthBarParent = GameObject.FindGameObjectWithTag("healthBar");
        barBackground = healthBarParent.transform.GetChild(0).GetComponent<Image>();
        healthBar = healthBarParent.transform.GetChild(1).GetComponent<Image>();
        healthText = healthBarParent.transform.GetChild(2).GetComponent<Text>();
        barFullSize = barBackground.rectTransform.rect.width;
        barHeight = barBackground.rectTransform.rect.height;

        UpdateHealthBar(health, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //handle decreasing health
    public void DecreaseHealth(float amount)
    {
        health -= amount;
        health = Mathf.Floor(health * 10) / 10;
        
        UpdateHealthBar(health, maxHealth);

        if (health <= 0)
        {
            isAlive = false;
        }
    }

    public void HealHealth(float amount)
    {
        health += amount;
        health = Mathf.Floor(health * 10) / 10;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UpdateHealthBar(health, maxHealth);
    }

    //update health bar
    private void UpdateHealthBar(float amount, float cap)
    {
        //change bar
        healthBar.rectTransform.sizeDelta = new Vector2(amount / cap * barFullSize, barHeight);

        //change text
        healthText.text = amount.ToString("F1");
    }

    public bool IsAtMaxHealth()
    {
        return health == maxHealth;
    }

    public bool IsAlive
    {
        get { return isAlive; }
    }
    public float MaxHealth
    {
        get { return maxHealth; }
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        health += amount;

        UpdateHealthBar(health, maxHealth);
    }
}
