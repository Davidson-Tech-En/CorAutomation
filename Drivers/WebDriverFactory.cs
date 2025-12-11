using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CorAutomation.Drivers
{
    public static class WebDriverFactory
    {
        public static IWebDriver Create()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            return new ChromeDriver(options);
        }
    }
}
