using UnityEngine;
using UnityEngine.UI;

namespace SupermarketSim.Player
{
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int money = 300;
        [SerializeField] private Text moneyLabel;

        public int Money => money;

        public event System.Action<int> OnMoneyChanged;

        private void Awake()
        {
            EnsureMoneyLabel();
            RefreshUI();
        }

        private void Start()
        {
            EnsureMoneyLabel();
            RefreshUI();
        }

        public void BindLabel(Text label)
        {
            moneyLabel = label;
            RefreshUI();
        }

        public void EnsureMoneyLabel()
        {
            if (moneyLabel != null) return;

            var existing = GameObject.Find("MoneyText");
            if (existing != null)
            {
                moneyLabel = existing.GetComponent<Text>();
                if (moneyLabel != null) return;
            }

            var canvasObj = GameObject.Find("PlayerCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("PlayerCanvas");
                canvasObj.transform.SetParent(transform, false);
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            var moneyObj = new GameObject("MoneyText");
            moneyObj.transform.SetParent(canvasObj.transform, false);
            moneyLabel = moneyObj.AddComponent<Text>();
            moneyLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            moneyLabel.fontSize = 32;
            moneyLabel.fontStyle = FontStyle.Bold;
            moneyLabel.color = new Color(0.95f, 0.95f, 0.6f);
            moneyLabel.alignment = TextAnchor.UpperRight;

            var rect = moneyLabel.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -24f);
            rect.sizeDelta = new Vector2(420f, 48f);

            var outline = moneyObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        public void SetMoney(int value)
        {
            money = Mathf.Max(0, value);
            RefreshUI();
            OnMoneyChanged?.Invoke(money);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (money < amount) return false;
            money -= amount;
            RefreshUI();
            OnMoneyChanged?.Invoke(money);
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            money += amount;
            RefreshUI();
            OnMoneyChanged?.Invoke(money);
        }

        private void RefreshUI()
        {
            if (moneyLabel != null)
                moneyLabel.text = $"Баланс: {money} $";
        }
    }
}
