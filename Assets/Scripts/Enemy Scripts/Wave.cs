using System;
using System.Collections;
using UnityEngine;

public enum EnemyClass
{
    Cur,
    Knight,
    Monk,
    Paladin,
    Assassin,
    Succubus,
    Armorer,
    Warlock
}

public class Wave
{
    //number of enemies
    public int enemies { get; set; }

    //health of enemies
    public float health { get; set; }

    //attack of enemies
    public float attack { get; set; }

    //speed of enemies
    public float speed { get; set; }

    //curreny given on kill
    public float worth { get; set; }

    //enemy types
    public ElementTypes[] elementTypes { get; set; }

    //enemy type spread, used to randomly determine which type an enemy should be when it is spawned
    public float typeSpread { get; set; }

    public int waveNumber { get; set; }

    //enemy class
    public EnemyClass enemyClass { get; set; }

    /// <summary>
    /// creates a wave which contains information about the number of enemies, their attack, their speed, and their currency value
    /// </summary>
    /// <param name="enemies">number of enemies the wave should spawn</param>
    /// <param name="health">amount of health the enemies will have</param>
    /// <param name="attack">attack value of the enemies</param>
    /// <param name="speed">speed value of the enemies</param>
    /// <param name="worth">currency value of the enemies</param>
    /// <param name="elementTypes">array of all types present in this wave</param>
    /// <param name="enemyClass">class that enemy should belong to</param>
    public Wave(int enemies, float health, float attack, float speed, float worth, ElementTypes[] elementTypes, EnemyClass enemyClass, int waveNumber)
    {
        this.enemies = enemies;
        this.health = health;
        this.attack = attack;
        this.speed = speed;
        this.worth = worth;
        this.elementTypes = elementTypes;
        typeSpread = elementTypes.Length / (float)enemies;
        this.enemyClass = enemyClass;
        this.waveNumber = waveNumber;
    }

    //determine what type an enemy will be
    public ElementTypes ChooseType(ElementTypes[] types, float spread)
    {
        //i dont know if this works
        float rng = UnityEngine.Random.Range(0, spread * types.Length);

        for (int i = 0; i < types.Length; i++)
        {
            if (rng <= spread * (i + 1))
            {
                return types[i];
            }
        }

        return ElementTypes.Error;
    }

    //overload that uses the default types spread
    public ElementTypes ChooseType()
    {
        //i dont know if this works
        float rng = UnityEngine.Random.Range(0, typeSpread * elementTypes.Length);

        for (int i = 0; i < elementTypes.Length; i++)
        {
            if (rng <= typeSpread * (i + 1))
            {
                return elementTypes[i];
            }
        }

        return ElementTypes.Error;
    }

    //return a string with all wave information
    public string GetWaveInfo()
    {
        string elementString = "";
        string colorTag1 = "<color=#ffffffff>";

        foreach (ElementTypes element in elementTypes)
        {
            switch ((int)element)
            {
                case 0:
                    colorTag1 = "<color=#333333ff>";
                    break;
                case 1:
                    colorTag1 = "<color=#aaaaaaff>";
                    break;
                case 2:
                    colorTag1 = "<color=#ff0000ff>";
                    break;
                case 3:
                    colorTag1 = "<color=#0000ffff>";
                    break;
                case 4:
                    colorTag1 = "<color=#cc6619ff>";
                    break;
                case 5:
                    colorTag1 = "<color=#cccc00ff>";
                    break;
                case 6:
                    colorTag1 = "<color=#b200ccff>";
                    break;
                case 7:
                    colorTag1 = "<color=#00ff00ff>";
                    break;
                default:
                    colorTag1 = "<color=white>";
                    break;
            }
            elementString += colorTag1 + element + "</color>, ";
        }

        elementString = elementString.Substring(0, elementString.Length - 2);

        return string.Format("wave #{0}\n{1} enemies\n{2} attack\n{3} health\n{4} speed\n{5} money\n{6} class\nthe elements within are\n{7}", waveNumber, enemies, attack, health, speed, worth, enemyClass, elementString);
    }
}



