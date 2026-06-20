using UnityEngine;

namespace SupermarketSim.Architecture
{
    /// <summary>
    /// Контекст игровой сцены. Наследуется от ApplicationContext.
    /// </summary>
    public class GameContext : ApplicationContext
    {
        protected override void Awake()
        {
            base.Awake();
            // Специфичная инициализация для игры
        }

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            // Здесь будет биндинг сервисов, специфичных для игрового процесса
        }
    }
}
