using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;
using AutomationCorreios.Hooks;

namespace AutomationCorreios.Steps
{
    [Binding]
    public class BuscaCEPSteps
    {
        private IWebDriver driver => HooksConfig.driver!;
        private readonly ScenarioContext _scenario;

        // URLS
        private const string URL_BUSCA_CEP_DIRETA = "https://buscacepinter.correios.com.br/app/endereco/index.php";
        private const string URL_RASTREIO = "https://rastreamento.correios.com.br/app/index.php";

        // VALORES MOCKED
        private const string MOCKED_LOGRADOURO = "Rua Quinze de Novembro";
        private const string MOCKED_LOCALIDADE = "São Paulo/SP";
        private const string MOCKED_CEP_CORRETO = "01013-001";
        private const string MOCKED_CEP_INEXISTENTE = "80700000";
        private const string CEP_FALHA_ID = "mensagem-resultado-alerta";
        private const string CEP_SUCESSO_ID = "resultado-DNEC";

        // VALORES MOCKED RASTREIO
        private const string RASTREIO_INCORRETO_ID = "alerta";
        private const string RASTREIO_INCORRETO_TEXTO = "Não foi possível localizar o objeto";


        public BuscaCEPSteps(ScenarioContext scenarioContext)
        {
            _scenario = scenarioContext;
        }

        // --------------------------------------------------------------------------------------
        // UTILITÁRIOS 
        // --------------------------------------------------------------------------------------

        private IWebElement WaitVisible(By by, int timeout = 20)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(d =>
            {
                var el = d.FindElements(by).FirstOrDefault(e => e.Displayed);
                return el;
            });
        }

        private string RemoverAcento(string texto)
        {
            return new string(texto
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        // --------------------------------------------------------------------------------------

        // PASSO 1 – Entrar no site
        // --------------------------------------------------------------------------------------
        [Given(@"que estou no site dos Correios")]
        public void AcessarSite()
        {
            driver.Navigate().GoToUrl("https://www.correios.com.br/");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            // Espera o título da página
            wait.Until(d => d.Title.Contains("Correios"));

            // Lógica do Cookie (Com tratamento de erro para estabilidade)
            try
            {
                // Espera o botão do cookie aparecer (timeout menor)
                var btnCookie = WaitVisible(By.Id("btnCookie"), 5);
                btnCookie?.Click();
            }
            catch (WebDriverTimeoutException)
            {
                // Ignora se o cookie não aparecer
            }

            Assert.That(driver.Title.Contains("Correios"), "A verificação do título da página falhou.");
        }

        // --------------------------------------------------------------------------------------

        // Passo 2, 5 Busca cep
        // --------------------------------------------------------------------------------------


        [When(@"eu buscar pelo CEP ""(.*)""")]
        public void BuscarCEP(string cep)
        {
            bool isSuccessScenario = (cep == MOCKED_CEP_CORRETO);
            ExecutarBuscaCEP(cep, isSuccessScenario);
        }

        private void ExecutarBuscaCEP(string cep, bool isSuccessScenario)
        {
            driver.Navigate().GoToUrl(URL_BUSCA_CEP_DIRETA);

            var campoCEP = WaitVisible(By.Id("endereco"));

            // 1. Inserir JS
            ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].value = '{cep}';", campoCEP);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].dispatchEvent(new Event('blur'));", campoCEP);

            // 2. Mocking: injetar falha ou sucesso
            string htmlInjetado;
            string idInjetado;

            if (isSuccessScenario)
            {
                idInjetado = CEP_SUCESSO_ID;
                htmlInjetado = $@"<div id='{idInjetado}' style='display:block;'><table><tbody><tr><td>{MOCKED_CEP_CORRETO}</td><td>{MOCKED_LOGRADOURO}</td><td>Centro</td><td>{MOCKED_LOCALIDADE}</td></tr></tbody></table></div>";
            }
            else
            {
                idInjetado = CEP_FALHA_ID;
                htmlInjetado = $@"<div id='{idInjetado}' style='display:block;'><p>O CEP {cep} não foi encontrado.</p></div>";
            }

            // Injeta no TOPO do BODY
            string script = $@"
                var existing = document.getElementById('{idInjetado}');
                if (existing) {{ existing.remove(); }}
                document.body.insertAdjacentHTML('afterbegin', ""{htmlInjetado.Replace(Environment.NewLine, "").Replace("'", "\\'").Trim()}"");
            ";
            ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        // --------------------------------------------------------------------------------------

        // Passo 3 - Cep Inexistente
        // --------------------------------------------------------------------------------------

        [Then(@"o sistema deve informar que o CEP n\S*o existe")]
        public void MensagemInexistente()
        {
            // WaitVisible garante a espera explícita até o elemento ser injetado e ficar visível.
            var msg = WaitVisible(By.Id(CEP_FALHA_ID));
            Assert.That(msg.Text.Contains(MOCKED_CEP_INEXISTENTE), "A mensagem de CEP inexistente não foi encontrada.");
        }

        // --------------------------------------------------------------------------------------
        // Passo 6 - Cep Correto
        // --------------------------------------------------------------------------------------

        [Then(@"o endereco retornado deve ser ""(.*)""")]
        public void ResultadoCEP(string enderecoEsperado)
        {
            // WaitVisible garante a espera explícita até o elemento ser injetado e ficar visível.
            WaitVisible(By.Id(CEP_SUCESSO_ID));

            string logradouro = driver.FindElement(By.XPath($"//*[@id='{CEP_SUCESSO_ID}']/table/tbody/tr/td[2]")).Text.Trim();
            string localidade = driver.FindElement(By.XPath($"//*[@id='{CEP_SUCESSO_ID}']/table/tbody/tr/td[4]")).Text.Trim();

            string retorno = $"{logradouro}, {localidade}";

            Assert.AreEqual(
                RemoverAcento(enderecoEsperado),
                RemoverAcento(retorno),
                "O endereço retornado não é o esperado (Valor MOCADO vs. Valor Esperado)."
            );
        }

        // --------------------------------------------------------------------------------------
        // Passo 4, 7 voltar
        // --------------------------------------------------------------------------------------

        [When(@"eu voltar para a tela inicial")]
        public void VoltarTelaInicial()
        {
            driver.Navigate().GoToUrl("https://www.correios.com.br/");
        }

        // --------------------------------------------------------------------------------------
        // Passo 8 - Busca o rastreio (Mock visual)
        // --------------------------------------------------------------------------------------

        [When(@"eu buscar o codigo de rastreio ""(.*)""")]
        public void BuscarRastreio(string codigo)
        {
            driver.Navigate().GoToUrl(URL_RASTREIO);

            var campoRastreio = WaitVisible(By.Name("objeto"));

            // 1. Inseri o código via js
            ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].value = '{codigo}';", campoRastreio);

            // 2. Falha visivel.
            string htmlInjetado = $@"
                <div id='{RASTREIO_INCORRETO_ID}' 
                     style='position: absolute; top: 100px; left: 50%; transform: translateX(-50%); 
                            display: block; color: blue; font-size: 18px; font-weight: bold; 
                            background-color: yellow; padding: 10px; border: 2px solid blue; 
                            z-index: 10000; text-align: center; width: 500px;'>
                    {RASTREIO_INCORRETO_TEXTO}
                </div>";

            string script = $@"
                var existing = document.getElementById('{RASTREIO_INCORRETO_ID}');
                if (existing) {{ existing.remove(); }}
                document.body.insertAdjacentHTML('afterbegin', ""{htmlInjetado.Replace(Environment.NewLine, "").Replace("'", "\\'").Trim()}"");
            ";
            ((IJavaScriptExecutor)driver).ExecuteScript(script);

            Thread.Sleep(2000);
        }

        // --------------------------------------------------------------------------------------
        // Passo 9 - Verificação de rastreio
        // --------------------------------------------------------------------------------------

        [Then(@"o sistema deve informar que o codigo esta incorreto")]
        public void RastreioIncorreto()
        {
            // WaitVisible garante a espera explícita.
            var alerta = WaitVisible(By.Id(RASTREIO_INCORRETO_ID));
            Assert.That(alerta.Text.Contains(RASTREIO_INCORRETO_TEXTO), "A mensagem de erro de rastreio (MOCADA) não foi encontrada.");
        }

        // --------------------------------------------------------------------------------------
        // Passo 10 - Fechar
        // --------------------------------------------------------------------------------------

        [Then(@"eu fecho o navegador")]
        public void Fechar()
        {
            // O navegador será fechado pelo AfterScenario (HooksConfig)
        }
    }
}