using System;

public static class RandomUtility
{
    private static System.Random random;
    public static int getRamdom()
    {
        int _max = 1500000000;

        if (random == null)
        {
            random = new System.Random((int)DateTime.Now.Ticks);
        }

        return random.Next(0, _max);
    }

    public static bool IsSucceedProbability(float probability)
    {
        if (probability >= 1)
        {
            return true;
        }

        if (probability <= 0)
        {
            return false;
        }

        float random = UnityEngine.Random.Range(0f, 1f);

        if (random <= probability)
        {
            return true;
        }

        return false;
    }
}