using System.Globalization;

namespace ProbabilityDistribution
{
    public struct DistributionCounter
    {
        private int _step;
        private readonly int[] _cells;
        
        public int Size { get; }

        public DistributionCounter(int size)
        {
            Size = size;
            _cells = new int[size];
            
            _step = 0;
        }

        public void AddStep(int cellIndex)
        {
            _step++;
            _cells[cellIndex]++;
        }

        public void GetDistribution(string[] distribution)
        {
            for (var i = 0; i < Size; i++)
            {
                distribution[i] = (int)((float) _cells[i] / _step * 10000) / 100f + "%";
            }
        }
    }
}
