using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Teste.Service
{
    public class ServiceBase
    {
        public IWebDriver driver;

        public void StartMethod()
        {
            OpenWebDriver();
        }

        private void OpenWebDriver()
        {

            using (ChromeDriverService driverService = ChromeDriverService.CreateDefaultService("/home/bruno/Área de Trabalho/Teste/Driver"))
            {
                var co = new ChromeOptions();
                co.AddArgument("--incognito");

                using (var driver = new ChromeDriver(driverService, co, TimeSpan.FromSeconds(60)))
                {
                    Navagation(driver);
                }
            }

        }

        private void Navagation(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("http://comprasnet.gov.br/acesso.asp?url=/livre/pregao/ata0.asp");

            driver.SwitchTo().Frame("main2");
            // driver.SwitchTo().Frame("main2");

            driver.FindElement(By.Id("btorgao")).Click();

            Thread.Sleep(1000);

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            var option = driver.FindElement(By.Id("lstOrgao"));
            var selectOption = new SelectElement(option);

            Thread.Sleep(5000);

            selectOption.SelectByValue("20114");

            Thread.Sleep(2000);

            driver.FindElement(By.Id("btok")).Click();

            Thread.Sleep(2000);

            var itens = driver.FindElements(By.Id("chkUASG")).ToList();

            int index = 0;

            foreach (var item in itens)
            {
                if (index > 4)
                {
                    break;
                }

                item.Click();

                index++;
            }

            driver.FindElement(By.Id("btSeleciona2")).Click();

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.SwitchTo().Frame("main2");

            driver.FindElement(By.Name("ok")).Click();

            Thread.Sleep(2000);

            var linhas = driver.FindElements(By.ClassName("tex3"));
            var tag = driver.FindElement(By.TagName("html"));
            var html = tag.GetAttribute("innerHTML");

            foreach (var linha in linhas)
            {

            }

        }
    }
}