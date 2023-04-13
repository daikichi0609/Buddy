using System;

static class WeightedRandomSelector
{
    private static Random ms_Random = new Random();

    static public int SelectIndex(int[] weights)
    {
        int totalWeight = 0;

        foreach (int w in weights)
            totalWeight += w;

        int rand = ms_Random.Next(totalWeight);
        int sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (rand < sum)
                return i;
        }

        return -1;
    }
}