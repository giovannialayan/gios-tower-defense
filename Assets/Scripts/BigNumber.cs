using System;
using System.Collections;

public class BigNumber
{
    //TODO
    //i think i fixed this but if there are problems in the future look at this first
    //the BigNumber input overload for IncreaseBy is not sustainable, figure out a way that cant reach intmax unlike the current version


    //big number array and length
    private int[] bigNumber;
    private int numberLen;

    //constructor for BigNumber
    public BigNumber()
    {
        bigNumber = new int[1];
        bigNumber[0] = 0;
        numberLen = bigNumber.Length;
    }

    //return array as a string
    public string Number
    {
        get
        {
            return string.Join("", bigNumber);
        }
    }

    //property for bigNumber
    public int[] NumberArray
    {
        get { return bigNumber; }
    }

    //property for numberLen
    public int NumberLength
    {
        get { return numberLen; }
    }

    //increase bigNumber by amount and update the array if it goes to the next place
    public void IncreaseBy(int amount)
    {
        bigNumber[numberLen - 1] += amount;

        int extra = 0;

        for(int i = numberLen - 1; i >= 0; i--)
        {
            //if the current number is greater than 10, remove the ten and add it as 1 to the next place
            bigNumber[i] += extra;
            
            if(bigNumber[i] >= 10)
            {
                extra = bigNumber[i] - bigNumber[i] % 10;

                //extend array if we reach 0 but there is still extra to add to the next place
                if (i == 0)
                {
                    int[] temp = new int[numberLen + 1];
                    Array.Copy(bigNumber, 0, temp, 1, numberLen + 1);

                    bigNumber = temp;
                    numberLen = bigNumber.Length;

                    i++;
                }
            }
            else
            {
                extra = 0;
            }
        }
    }

    //overload for IncreaseBy that allows addition by other BigNumbers
    public void IncreaseBy(BigNumber otherNumber)
    {
        for(int i = numberLen - 1; i >= 0; i--)
        {
            this.IncreaseBy(otherNumber.NumberArray[i] * (int)Math.Pow(10, numberLen - i - 1));
        }
    }

    //decrease bigNumber by amount and update the array if it goes into the next place
    public void DecreaseBy(int amount)
    {
        bigNumber[numberLen - 1] -= amount;

        int extra = 0;

        for (int i = numberLen - 1; i >= 0; i--)
        {
            bigNumber[i] += extra;

            if (bigNumber[i] <= -10)
            {
                extra = bigNumber[i] + bigNumber[i] % 10;
            }
            else
            {
                extra = 0;
            }
        }
    }

    //multiply this number by the amount
    /// <summary>
    /// multiply by a single digit only
    /// </summary>
    /// <param name="amount">single digit multiplier</param>
    /// <returns></returns>
    public void MultiplyBy(int amount)
    {
        //multiply each digit by amount, IF YOU WANT TO MAKE THIS ABLE TO USE MULTIPLE DIGITS LATER USE THE GRID METHOD: https://en.wikipedia.org/wiki/Multiplication_algorithm
        for (int i = 0; i < numberLen; i++)
        {
            bigNumber[i] *= amount;
        }

        BigNumber multipliedNumber = new BigNumber();

        for (int i = numberLen - 1; i >= 0; i--)
        {
            //the multiplier at the end of this line is the place of the number it is adding
            //ie: the second to last digit of bigNumber is in the tens place so if the number is 123 it is multiplied by 10^(3 - 1 - 1)
            //because i starts at 2, it has iterated once so i is now 1, 3 - 1 -1 = 1, the digit in the tens place is multiplied by 10
            multipliedNumber.IncreaseBy(bigNumber[i] * (int)Math.Pow(10, numberLen - i - 1));
        }

        bigNumber = multipliedNumber.NumberArray;
        numberLen = bigNumber.Length;
    }
    
    //multiply this number by the other BigNumber
    public void DivideBy(BigNumber amount)
    {
        //google division algorithm
    }

    //check if this number is greater than the other number
    public bool IsGreaterThan(BigNumber otherNumber)
    {
        return true;
    }

    //check if this number is less than the other number
    public bool IsLessThan(BigNumber otherNumber)
    {
        return true;
    }

    //check if this number is equal to the other number, if it is neither greater than nor less than the other number it must be equal to it
    public bool IsEqualTo(BigNumber otherNumber)
    {
        return !this.IsGreaterThan(otherNumber) && !this.IsLessThan(otherNumber);
    }
}
