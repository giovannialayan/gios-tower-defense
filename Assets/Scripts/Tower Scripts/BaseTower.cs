using System;

public enum ElementTypes
{
    Chaos = 0,
    Order = 1,
    Fire = 2,
    Water = 3,
    Earth = 4,
    Lightning = 5,
    Whimsical = 6,
    Nature = 7,
    Error = 8
}

public class BaseTower
{
    //range is the radius of the circle around the tower that it can shoot in
    protected float range;

    //attack is the damage a single bullet deals
    protected float attack;

    //attack speed is the number of seconds between bullet shots
    protected float attackSpeed;

    //element of the tower
    protected ElementTypes element;
    protected ElementTypes immutableElement;

    //water tower enhancement
    protected bool watered;

	public BaseTower(float attack, float attackSpeed, float range, ElementTypes element)
	{
        this.attack = attack;
        this.attackSpeed = attackSpeed;
        this.range = range;
        this.element = element;
        immutableElement = element;
        watered = false;
	}

    //attack property
    public float Attack
    {
        get { return watered ? attack *= 1.1f: attack; }
        set { attack = value; }
    }

    //attack speed property
    public float AttackSpeed
    {
        get { return watered ? attackSpeed *= .9f: attackSpeed; }
        set { attackSpeed = value; }
    }

    //range property
    public float Range
    {
        get { return watered ? range *= 1.1f : range; }
        set { range = value; }
    }

    //element property
    public ElementTypes Element
    {
        get { return element; }
        set { element = value; }
    }

    //property for immutable element
    public ElementTypes ImmutableElement
    {
        get { return immutableElement; }
    }

    //property for watered
    public bool Watered
    {
        get { return watered; }
        set { watered = value; }
    }
}
