using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class CollectedFishUI : MonoBehaviour
    {
        public static CollectedFishUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI collectedText;
        [SerializeField] private string labelPrefix = "Collected Fishes: ";

        private int _count;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateText();
        }

        public void Add(int amount = 1)
        {
            _count += amount;
            UpdateText();
        }

        public void Set(int count)
        {
            _count = count;
            UpdateText();
        }

        private void UpdateText()
        {
            if (collectedText != null)
            {
                collectedText.text = $"{labelPrefix}{_count}";
            }
        }
    }
}
