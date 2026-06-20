using UnityEngine;

namespace SupermarketSim.Architecture
{
    /// <summary>
    /// Контекст главного меню. Наследуется от ApplicationContext.
    /// </summary>
    public class MenuContext : ApplicationContext
    {
        protected override void Awake()
        {
            base.Awake();
            if (GetComponent<MainMenuUI>() == null)
                gameObject.AddComponent<MainMenuUI>();
        }

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            // Здесь будет биндинг сервисов, специфичных для меню
        }
    }
}
