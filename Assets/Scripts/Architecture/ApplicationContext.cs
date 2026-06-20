using UnityEngine;

namespace SupermarketSim.Architecture
{
    /// <summary>
    /// Базовый контекст приложения. 
    /// Должен содержать глобальные зависимости, которые шарятся между всеми сценами.
    /// </summary>
    public class ApplicationContext : MonoBehaviour
    {
        protected virtual void Awake()
        {
            // Инициализация глобальных зависимостей (BInject)
            SetupDependencies();
        }

        protected virtual void SetupDependencies()
        {
            // Здесь будет биндинг глобальных сервисов
        }
    }
}
