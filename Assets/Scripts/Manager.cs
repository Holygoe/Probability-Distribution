using UnityEngine;
using UnityEngine.UI;

namespace ProbabilityDistribution
{
    public class Manager : MonoBehaviour
    {
        public bool isCrazy;
        public Text[] cells;

        private DistributionCounter _counter;
        private string[] _distribution;

        private void Start()
        {
            _counter = new DistributionCounter(cells.Length);
            _distribution = new string[cells.Length];
            
            InvokeRepeating(nameof(UpdateText), 1, 1);
        }

        private void Update()
        {
            const int rate = 100;
            
            for (var i = 0; i < rate; i++)
            {
                if (isCrazy)
                {
                    Randomizer.MakeCrazyStep(ref _counter);
                }
                else
                {
                    Randomizer.MakeStep(ref _counter);
                }
            }
        }

        private void UpdateText()
        {
            _counter.GetDistribution(_distribution);
            
            for (var i = 0; i < cells.Length; i++)
            {
                cells[i].text = _distribution[i];
            }
        }
    }
}
