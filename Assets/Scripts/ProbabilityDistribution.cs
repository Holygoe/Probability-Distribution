using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProbabilityDistribution
{
    public class ProbabilityDistribution : MonoBehaviour
    {
        /// <summary>
        /// Текстовые поля со значением веса (или не веса), указанного для каждого элемента.
        /// </summary>
        public Text[] items;
        
        /// <summary>
        /// Текстовые поля для результатов распределения вероятностей по алгоритму Сергея (типа как для настольной игры).
        /// </summary>
        public Text[] tabletopProb;

        /// <summary>
        /// Текстовые поля для результатов, рассчитаных по обычному алгоритму, если считать, что указаны были веса.
        /// </summary>
        public Text[] weightProb;

        /// <summary>
        /// Здесь я задал 10 предметов с их "вероятностями" выпадения по аналогии с тем, как указал Сергей. Их сумма
        /// ровна 165. Ну этот так :-) для наглядности.  
        /// </summary>
        private static readonly float[] PROBS =
        {
            20, 30, 50
        };

        /// <summary>
        /// Сумма всех "вероятностей" или "весов", как хотите. 
        /// </summary>
        private readonly float _totalProb = PROBS.Sum(prob => prob);
        
        /// <summary>
        /// Счётчик выпадений для алгоритма Сергея.
        /// </summary>
        private DistributionCounter _tabletopCounter = new DistributionCounter(PROBS.Length);
        
        /// <summary>
        /// Счётчик выпадений для обычного алгоритма с весами.
        /// </summary>
        private DistributionCounter _weightCounter = new DistributionCounter(PROBS.Length);

        /// <summary>
        /// Будем обновлять результаты распределения раз в секунду.
        /// </summary>
        private void Start()
        {
            InvokeRepeating(nameof(UpdateText), 1, 1);

            for (var i = 0; i < PROBS.Length; i++)
            {
                items[i].text = PROBS[i].ToString();
            }
        }

        /// <summary>
        /// Каждый FixedUpdate, т.е. 60 раз в секунду, по 100 раз случайно получаем предмет и заносим результат в счётчики. 
        /// </summary>
        private void FixedUpdate()
        {
            const int rate = 100;
            
            for (var i = 0; i < rate; i++)
            {
                // Делаем случайный выбор по схеме с настольной игрой.
                Randomizer.AddRandomItemViaTabletopWay(ref _tabletopCounter, PROBS);
                
                // Делаем случайный выбор по схеме с весами.
                Randomizer.AddRandomItemViaWeightWay(ref _weightCounter, PROBS, _totalProb);
            }
        }

        /// <summary>
        /// Заполняем результаты распределения.
        /// </summary>
        private void UpdateText()
        {
            UpdateDistribution(tabletopProb, _tabletopCounter);
            UpdateDistribution(weightProb, _weightCounter);
        }

        /// <summary>
        /// Обновить значения распределений в текстовых полях <see cref="texts"/> согласно счётчику <see cref="counter"/>.
        /// </summary>
        private static void UpdateDistribution(Text[] texts, DistributionCounter counter)
        {
            var distribution = counter.GetDistribution();

            for (var i = 0; i < counter.Size; i++)
            {
                texts[i].text = distribution[i];
            }
        }
    }
    
    public struct DistributionCounter
    {
        /// <summary>
        /// Сколько всего было добавлено предметов.
        /// </summary>
        private int _totalCount;
        
        /// <summary>
        /// Сколько было добавлено тех или иных предметов.
        /// </summary>
        private readonly int[] _itemCounts;
        
        /// <summary>
        /// Размер счетчика.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Создаём новый счетчик на <see cref="size"/> предметов.
        /// </summary>
        public DistributionCounter(int size)
        {
            Size = size;
            _itemCounts = new int[size];
            
            _totalCount = 0;
        }

        /// <summary>
        /// Добавить новый предмет с номером <see cref="itemIndex"/>.
        /// </summary>
        public void AddItem(int itemIndex)
        {
            _totalCount++;
            _itemCounts[itemIndex]++;
        }

        /// <summary>
        /// Получить текущее распределение вероятностей для предметов.
        /// </summary>
        public string[] GetDistribution()
        {
            var distribution = new string[Size];
            
            if (_totalCount == 0) return distribution;

            for (var i = 0; i < Size; i++)
            {
                distribution[i] = (int)((float) _itemCounts[i] / _totalCount * 10000) / 100f + "%";
            }

            return distribution;
        }
    }
    
    /// <summary>
    /// Класс содержит два разных способа сулчайного получения предмета.
    /// </summary>
    public static class Randomizer
    {
        /// <summary>
        /// Это сосбоб случайного получения предмета сравни настольной игре.
        /// </summary>
        /// <param name="counter">Счетчик, куда будет занесён результат.</param>
        /// <param name="items">Предметы в мешочке с их вероятностями.</param>
        public static void AddRandomItemViaTabletopWay(ref DistributionCounter counter, IEnumerable<float> items)
        {
            // Будем тут хранить те предметы, которые еще не достали из мешочка.
            var probs = items.Select((value, index) => (index, value)).ToList();
            
            // Сумма вероятностей всех предыдущих попыток. Вначале она равна 0.
            var totalPreviousProbs = 0;

            // Можем попытаться достать лишь то количество раз, сколько предметов в мешочке.
            for (var i = 0; i < counter.Size; i++)
            {
                // Бросаем две кости на d10, получам значения от 0 до 99
                var roll = Random.Range(0, 100);
                
                // Достаём новый предмет из мешочка.
                var probIndex = Random.Range(0, probs.Count);
                var prob = probs[probIndex];
                probs.Remove(prob);

                // Если бросок костей меньше, чем вероятность текущей вещи плюс сумма вероятностей всех предыдущих вещей,
                // то это то, что нам нужно.
                if (roll <= prob.value + totalPreviousProbs)
                {
                    counter.AddItem(prob.index);
                    
                    return;
                }
                
                // Если бросок кубика нас не устроил, то переходим к следующему
                totalPreviousProbs += (int) prob.value;
            }
        }
        
        /// <summary>
        /// Это обычный способ распределения по весам. Обратите внимание, какой он простой и не требует такого
        /// количества вычислений. Random.value вызывается всего один раз вызывается, нет никаких LINQ и все такое.
        /// </summary>
        /// <param name="counter">Счетчик, куда будет занесён результат.</param>
        /// <param name="weights">Реестр предметов с их весами.</param>
        /// <param name="totalWeight">Сумма всех весов, чтобы не считать каждый раз.</param>
        public static void AddRandomItemViaWeightWay(ref DistributionCounter counter, float[] weights, float totalWeight)
        {
            // Делаем бросок кости и умножаем его на общую массу.
            var roll = Random.value * totalWeight;

            // Перебираем все предметы по очереди.
            for (var i = 0; i < counter.Size; i++)
            {
                // Если бросок кубика меньше веса предмета, то добавляем его в счетчик и выходим.
                if (roll < weights[i])
                {
                    counter.AddItem(i);
                    
                    return;
                }

                // Перед тем как перейти к следующему предмету, уменьшаем значение значение броска кубика.
                roll -= weights[i];
            }
        
            // Добавляем с счечик последний предмет, если ничего не выпало.
            counter.AddItem(counter.Size - 1);
        }
    }
}
