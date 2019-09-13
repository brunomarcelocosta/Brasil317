using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Teste.Service
{
    public class ServiceBase
    {
        public IWebDriver driver;
        public static int threads = 0;

        public void StartMethod(string value)
        {

            Console.WriteLine("Start Metodo...");

            threads = int.Parse(value);

            var processNumber = 0;
            Thread[] array = new Thread[threads];

            var result = Parallel.For(0, threads, (i, state) =>
            {
                processNumber++;
                object obj = processNumber;
                OpenWebDriver(processNumber);

                if (i == threads)
                {
                    state.Break();
                }

            });

            var path = @"./home/HTMLs";

            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Console.WriteLine($"file:{file}");
            }

        }


        public void OpenWebDriver(object processNumber)
        {
            int intNumber = int.Parse(processNumber.ToString());

            try
            {
                // ChromeDriverService driverService = ChromeDriverService.CreateDefaultService("/opt/selenium");

                Console.WriteLine("Pegou webdriver...");

                var co = new ChromeOptions();
                co.AddArgument("--incognito");
                co.AddArgument("--no-sandbox");
                co.AddArgument("--headless");

                var driver = new ChromeDriver("/opt/selenium", co);

                Console.WriteLine("Open webdriver...");

                try
                {
                    Navagation(driver, intNumber, threads);
                }
                catch
                {

                }
                finally
                {
                    driver.Quit();
                    driver.Dispose();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

        }

        private void Navagation(IWebDriver driver, int processNumber, int threads)
        {
            try
            {
                ExecuteContinue(delegate { GoToHomePage(driver); }, out bool isPage);
                if (!isPage)
                {
                    Console.WriteLine("Timeout");
                    return;
                }

                ExecuteContinue(delegate { SelectFilter(driver); }, out bool isFilter);
                if (!isFilter)
                {
                    Console.WriteLine("Timeout");
                    return;
                }

                ExecuteContinue(delegate { SelectItem(driver, processNumber, threads); }, out bool isItem);
                if (!isItem)
                {
                    Console.WriteLine("Timeout");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

        }

        private void GoToHomePage(IWebDriver driver)
        {
            try
            {
                Console.WriteLine("GOING TO HOMEPAGE...");

                driver.Navigate().GoToUrl("http://comprasnet.gov.br/acesso.asp?url=/livre/pregao/ata0.asp");

                Console.WriteLine("AT HOMEPAGE...");

                driver.Navigate().Refresh();
            }
            catch
            {

            }

        }

        private void SelectFilter(IWebDriver driver)
        {
            try
            {
                driver.Navigate().Refresh();

                driver.SwitchTo().Frame("main2");

                while (true)
                {
                    driver.FindElement(By.Id("btorgao")).Click();

                    try
                    {
                        driver.SwitchTo().Window(driver.WindowHandles[1]);
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        driver.Navigate().Refresh();
                    }
                }

                var option = driver.FindElement(By.Id("lstOrgao"));
                var selectOption = new SelectElement(option);

                selectOption.SelectByValue("20114");

                driver.FindElement(By.Id("btok")).Click();

                Thread.Sleep(1000);

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

                Thread.Sleep(1000);

            }
            catch
            {

            }
        }

        private void SelectItem(IWebDriver driver, int processNumber, int threads)
        {
            try
            {
                double count = (driver.FindElements(By.TagName("a")).Count) / threads;
                var value = Convert.ToInt16(Math.Round(count));

                int index_end = value * processNumber;
                int index_init = processNumber == 1 ? 0 : (index_end - value) + 1;

                for (int i = index_init; i < index_end; i++)
                {

                    driver.SwitchTo().Window(driver.WindowHandles[0]);
                    driver.SwitchTo().Frame("main2");

                    var linhas = driver.FindElements(By.TagName("a"));

                    var name = linhas[i].GetAttribute("name");
                    var _name = name.Substring(name.Length - 2);

                    if (_name.Equals("-1"))
                    {
                        driver.FindElement(By.Id("btnVoltar")).Click();
                        driver.FindElement(By.Id("btnVoltar")).Click();

                        i--;
                        continue;

                    }
                    else
                    {
                        linhas[i].Click();

                        var html = driver.PageSource;

                        CreateFile(html, name);

                        driver.FindElement(By.Id("btnVoltar")).Click();

                    }
                }
            }
            catch
            {

            }
        }

        public void CreateFile(string value, string name)
        {
            var directory = @"./home/HTMLs";

            string file = $"{directory}/{name}.html";

            Console.WriteLine("Recorded the file...");

            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(value);
            }

        }

        public void ExecuteContinue(Action action, out bool result)
        {
            bool continueTask = false;
            result = false;
            var _timeout = 0;
            var _timeoutMax = 1000 * 300;

            while (!continueTask)
            {
                Thread.Sleep(1000);
                _timeout += 1000;

                if (_timeout >= _timeoutMax)
                {
                    result = false;
                    continueTask = true;
                }
                else
                {

                    try
                    {
                        action();
                        continueTask = true;
                        result = true;
                    }
                    catch { }
                }
            }
        }
    }
}