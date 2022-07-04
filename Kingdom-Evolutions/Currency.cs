using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Currency 
{
    public string value;

    float amount = 0f;
    string multiplier = "";

    List<string> multipliers = new List<string>{ "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp" };

    public Currency(string value)
    {
        if (value == "")
        {
            Set("0");
            return;
        }
            
        this.value = value;

        ToFloat();
    }

    public void ToFloat()
    {
        multipliers = new List<string> { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp" };

        multiplier = FindMultiplier(value);
        Set(value);
    }

    public override string ToString()
    {
        return (Mathf.Round(amount * 100f) / 100f).ToString(CultureInfo.InvariantCulture) + multiplier;
    }

    public float GetFloat()
    {
        return amount;
    }

    public string FindMultiplier(string findIn)
    {
        string mul = Regex.Replace(findIn, @"[^A-Za-z]+", string.Empty);

        return mul;
    }

    public string FindFloat(string findIn)
    {
        string fl = Regex.Match(findIn, @"([-+]?[0-9]*\.?[0-9]+)").Groups[1].Value;
        return fl;
    }

    public string GetMultiplier()
    {
        return multiplier;
    }

    public int GetMultiplierIndex()
    {
        return (multiplier == "") ? 0 : multipliers.IndexOf(multiplier);
    }

    public void Set(string amount)
    {
        float x = 0f;
        string mul = "";
        
        int otherMulIndex = 0;

        if (char.IsLetter(amount, amount.Length - 1))
        {
            //Find the multiplier in string
            mul = FindMultiplier(amount);
            otherMulIndex = multipliers.IndexOf(mul);

            //Find the number in string
            string num = FindFloat(amount);
            x = float.Parse(num, CultureInfo.InvariantCulture);
        }
        else
        {
            x = float.Parse(amount, CultureInfo.InvariantCulture);
            mul = FindMultiplier(amount);
        }

        this.amount = x;
        multiplier = mul;

        ReCalculate();
    }

    public void Add(string amount)
    {
        float x = 0f;

        int myMulIndex = GetMultiplierIndex();
        int otherMulIndex = 0;

        if(char.IsLetter(amount, amount.Length - 1))
        {
            //Find the multiplier in string
            string mul = FindMultiplier(amount);
            otherMulIndex = multipliers.IndexOf(mul);

            //Find the number in string
            string num = FindFloat(amount);
            x = float.Parse(num, CultureInfo.InvariantCulture);
        }
        else
        {
            x = float.Parse(amount, CultureInfo.InvariantCulture);
        }

        int pow = otherMulIndex - myMulIndex;

        if (myMulIndex > otherMulIndex)//Adding a smaller number 
        {
            x = x * Mathf.Pow(10, pow * 3);
            this.amount += x;
        }
        else if(myMulIndex < otherMulIndex)//Adding bigger number
        {
            this.amount = this.amount / Mathf.Pow(10, pow * 3);
            this.amount = x + this.amount;
            IncreaseMultiplier(pow);
        }
        else
        {
            this.amount += x;
        }

        ReCalculate();
    }

    public void Add(float amount)
    {
        this.amount += amount;
        ReCalculate();
    }

    public void Remove(string amount)
    {
        float x = 0f;

        int myMulIndex = (multiplier == "") ? 0 : multipliers.IndexOf(multiplier);
        int otherMulIndex = 0;

        if (char.IsLetter(amount, amount.Length - 1))
        {
            string mul = FindMultiplier(amount);
            otherMulIndex = multipliers.IndexOf(mul);

            //Find the number in string
            string num = FindFloat(amount);
            x = float.Parse(num, CultureInfo.InvariantCulture);
        }
        else
        {
            x = float.Parse(amount, CultureInfo.InvariantCulture);
        }

        int pow = otherMulIndex - myMulIndex;

        if (myMulIndex > otherMulIndex)//Removing a smaller number 
        {
            x = x * Mathf.Pow(10, pow * 3);
            this.amount -= x;
        }
        else
        {
            this.amount -= x;
        }

        ReCalculate();
    }

    public void Multiply(float byAmount)
    {
        amount = amount * byAmount;
        ReCalculate();
    }

    public void Divide(Currency b)
    {
        amount = amount / b.GetFloat() / Mathf.Pow(10, (GetMultiplierIndex() - b.GetMultiplierIndex()) * 3);
        ReCalculate();
    }

    public static Currency operator +(Currency a, Currency b)
    {
        a.Add(b.ToString());

        return a;
    }

    public static Currency operator +(Currency a, float b)
    {
        a.Add(b);
        return a;
    }

    public static Currency operator -(Currency a, Currency b)
    {
        a.Remove(b.ToString());
        return a;
    }

    public static Currency operator *(Currency a, float b)
    {
        Currency c = new Currency(a.ToString());
        c.Multiply(b);

        return c;
    }

    public static Currency operator /(Currency a, Currency b)
    {
        Currency c = new Currency(a.ToString());
        c.Divide(b);

        return c;
    }

    public static float operator %(Currency a, Currency b)
    {
        float af = a.amount;
        float bf = b.amount;

        int pow = (a.GetMultiplierIndex() - b.GetMultiplierIndex()) * 3;

        return af / bf * Mathf.Pow(10, pow);
    }

    public static bool operator ==(Currency a, float b)
    {
        return (a.GetFloat() == b);
    }

    public static bool operator !=(Currency a, float b)
    {
        return (a.GetFloat() != b);
    }

    public static bool operator >(Currency a, Currency b)
    {
        if (a.GetMultiplierIndex() > b.GetMultiplierIndex())
        {
            return true;
        }
        else if(a.GetMultiplierIndex() < b.GetMultiplierIndex())
        {
            return false;
        }
        else
        {
            if (a.GetFloat() > b.GetFloat())
                return true;
            else
                return false;
        }
    }

    public static bool operator <(Currency a, Currency b)
    {
        if (a.GetMultiplierIndex() < b.GetMultiplierIndex())
        {
            return true;
        }
        else if (a.GetMultiplierIndex() > b.GetMultiplierIndex())
        {
            return false;
        }
        else
        {
            if (a.GetFloat() < b.GetFloat())
                return true;
            else
                return false;
        }
    }

    public static bool operator >=(Currency a, Currency b)
    {
        if (a.GetMultiplierIndex() > b.GetMultiplierIndex())
        {
            return true;
        }
        else if (a.GetMultiplierIndex() < b.GetMultiplierIndex())
        {
            return false;
        }
        else
        {
            if (a.GetFloat() >= b.GetFloat())
                return true;
            else
                return false;
        }
    }

    public static bool operator <=(Currency a, Currency b)
    {
        if (a.GetMultiplierIndex() < b.GetMultiplierIndex())
        {
            return true;
        }
        else if (a.GetMultiplierIndex() > b.GetMultiplierIndex())
        {
            return false;
        }
        else
        {
            if (a.GetFloat() <= b.GetFloat())
                return true;
            else
                return false;
        }
    }

    public static bool operator >(Currency a, float b)
    {
        return a.GetFloat() > b;
    }

    public static bool operator <(Currency a, float b)
    {
        return a.GetFloat() < b;
    }

    public static bool operator >= (Currency a, float b)
    {
        return a.GetFloat() >= b;
    }

    public static bool operator <= (Currency a, float b)
    {
        return a.GetFloat() <= b;
    }

    void ReCalculate()
    {
        if (this.amount >= 1000f)
        {
            if (IncreaseMultiplier(1))
            {
                this.amount /= 1000f;
            }
        }

        if(this.amount < 1f)
        {
            if (DecreaseMultiplier(1))
            {
                this.amount = this.amount * 1000f;
            }
            else 
            {
                this.amount = 0f;
            }
        }

        if(multiplier == "")
        {
            this.amount = Mathf.Round(this.amount);
        }
    }

    bool IncreaseMultiplier(int times)
    {

        int done = 0;

        for(int t = 0; t < times; t++)
        {
            for (int i = 0; i < multipliers.Count; i++)
            {
                if (multipliers[i] == multiplier)
                {
                    if (multipliers[i] != multipliers[multipliers.Count - 1])
                    {
                        multiplier = multipliers[i + 1];
                        done++;
                        break;
                    }
                }
            }
        }

        if (done == times)
            return true;

        return false;
    }

    bool DecreaseMultiplier(int times)
    {

        int done = 0;

        for (int t = 0; t < times; t++)
        {
            for (int i = 0; i < multipliers.Count; i++)
            {
                if (multipliers[i] == multiplier)
                {
                    if (multipliers[i] != multipliers[0])
                    {
                        multiplier = multipliers[i - 1];
                        done++;
                        break;
                    }
                }
            }
        }

        if (done == times)
            return true;

        return false;
    }
    
}
