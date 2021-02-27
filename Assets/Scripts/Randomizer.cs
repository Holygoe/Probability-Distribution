using UnityEngine;

namespace ProbabilityDistribution
{
    public static class Randomizer
    {
        public static void MakeStep(ref DistributionCounter counter)
        {
            var roll = Random.value * counter.Size;

            for (var i = 0; i < counter.Size; i++)
            {
                if (roll < 1)
                {
                    counter.AddStep(i);
                    return;
                }

                roll--;
            }
            
            counter.AddStep(counter.Size - 1);
        }
        
        public static void MakeCrazyStep(ref DistributionCounter counter)
        {
            var probability = 1;

            for (var i = 0; i < counter.Size; i++)
            {
                var roll = Random.value * counter.Size;
                
                if (roll < probability)
                {
                    counter.AddStep(i);
                    return;
                }

                probability++;
            }
            
            counter.AddStep(counter.Size - 1);
        }
    }
}
