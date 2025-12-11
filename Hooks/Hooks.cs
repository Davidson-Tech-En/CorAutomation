using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;

namespace AutomationCorreios.Hooks
{
    [Binding]
    public class HooksConfig
    {
        public static IWebDriver driver { get; private set; }

        // -------------------------------------------------------------------------
        // Modo de execução: Alterne entre true/false para mudar o modo de teste.
        // true = Modo Visual (Para demonstração e avaliação)
        // false = Modo Headless (Para performance e CI/CD)
        // -------------------------------------------------------------------------
        private const bool VISUAL_MODE_ENABLED = false;

        [BeforeScenario]
        public void BeforeScenario()
        {
            ChromeOptions options = new ChromeOptions();

            if (!VISUAL_MODE_ENABLED)
            {
                // Configurações para modo HEADLESS (visa a performance)
                options.AddArgument("--headless");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-extensions");

                // NOTA: Em Headless, o Assert funciona, mas a prova visual é perdida.
            }
            else
            {
                // Configurações para modo VISUAL (demonstração)
                options.AddArgument("--start-maximized");
            }

            // Inicializar o Driver com as opções configuradas
            driver = new ChromeDriver(options);

            // Configuração de espera implícita
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            driver?.Quit();
        }
    }
}