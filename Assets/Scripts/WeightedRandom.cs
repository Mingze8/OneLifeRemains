using System.Collections.Generic;

public class WeightedRandom
{
    // Generic method to handle weighted random selection
    public static T SelectRandom<T>(List<T> items, System.Func<T, int> weightSelector)
    {
        int totalWeight = 0;
        foreach (var item in items)
        {
            totalWeight += weightSelector(item);
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        foreach (var item in items)
        {
            accumulatedWeight += weightSelector(item);
            if (randomValue < accumulatedWeight)
            {
                return item;
            }
        }

        return default(T);  // Default, should never hit here
    }
}
