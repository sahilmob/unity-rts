using RTS.Environment;
using RTS.EventBus;
using RTS.Events;
using TMPro;
using UnityEngine;

namespace RTS.Player
{
    public class Supplies : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mineralsText;
        [SerializeField] private TextMeshProUGUI gasText;
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private SupplySO GasSO;
        [SerializeField] private SupplySO MineralsSO;

        public static int Minerals { get; private set; }
        public static int Gas { get; private set; }
        public static int Population { get; private set; }
        public static int PopulationLimit { get; private set; }

        void Awake()
        {
            Bus<SupplyEvent>.onEvent += HandleUpdateSupplies;
        }

        void OnDestroy()
        {
            Bus<SupplyEvent>.onEvent -= HandleUpdateSupplies;
        }

        private void HandleUpdateSupplies(SupplyEvent evt)
        {
            if (evt.SupplySO.Equals(GasSO))
            {
                Gas += evt.Amount;
                gasText.SetText(Gas.ToString());
            }
            else if (evt.SupplySO.Equals(MineralsSO))
            {
                Minerals += evt.Amount;
                mineralsText.SetText(Minerals.ToString());
            }
        }
    }
}