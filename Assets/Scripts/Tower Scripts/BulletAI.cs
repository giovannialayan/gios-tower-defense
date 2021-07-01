using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAI : MonoBehaviour
{
    //speed of bullet
    private float bulletSpeed;

    //damge dealt by bullet
    private float bulletDamage;

    //types of bullet
    private ElementTypes[] types;

    //status effect to apply
    private int statusEffect = 0;

    // Start is called before the first frame update
    void Start()
    {
        bulletSpeed = 10 * Time.fixedDeltaTime;

        bulletDamage = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.parent.position, bulletSpeed);
    }

    public float BulletDamage
    {
        get { return bulletDamage; }
        set { bulletDamage = value; }
    }

    public ElementTypes[] BulletType
    {
        get { return types; }
        set { types = value; }
    }

    //deal damage to enemy, checks for and applies type multiplier to damage
    public float DamageToDeal(ElementTypes etype)
    {
        float finalDamage = bulletDamage;

        switch (etype)
        {
            case ElementTypes.Lightning:
                if (types[0] == ElementTypes.Earth || types[1] == ElementTypes.Earth)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Fire:
                if (types[0] == ElementTypes.Water || types[1] == ElementTypes.Water)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Whimsical:
                if (types[0] == ElementTypes.Nature || types[1] == ElementTypes.Nature)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Order:
                if (types[0] == ElementTypes.Chaos || types[1] == ElementTypes.Chaos)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Nature:
                if (types[0] == ElementTypes.Fire || types[1] == ElementTypes.Fire)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Earth:
                if (types[0] == ElementTypes.Whimsical || types[1] == ElementTypes.Whimsical)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Chaos:
                if (types[0] == ElementTypes.Order || types[1] == ElementTypes.Order)
                {
                    finalDamage *= 1.5f;
                }
                break;

            case ElementTypes.Water:
                if (types[0] == ElementTypes.Lightning || types[1] == ElementTypes.Lightning)
                {
                    finalDamage *= 1.5f;
                }
                break;

            default:
                break;
        }

        return finalDamage;
    }

    //apply status effect to enemy
    public int StatusEffect
    {
        get { return statusEffect; }
        set { statusEffect = value; }
    }
}
