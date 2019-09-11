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
        public string path;
        public string url;

        public void StartMethod()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();


            var threads = int.Parse($"{config["threads"]}");
            path = $"{config["path"]}";

            DeleteFiles(path);

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

        }

        public void DeleteFiles(string directory)
        {
            var files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
        public void OpenWebDriver(object processNumber)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();

            path = $"{config["path"]}";
            url = $"{config["urlChrome"]}";
            var threads = int.Parse($"{config["threads"]}");

            int intNumber = int.Parse(processNumber.ToString());

            using (ChromeDriverService driverService = ChromeDriverService.CreateDefaultService("/home/bruno/Área de Trabalho/Teste/Driver"))
            {
                var co = new ChromeOptions();
                co.AddArgument("--incognito");

                using (var driver = new ChromeDriver(driverService, co, TimeSpan.FromSeconds(60)))
                {
                    try
                    {
                        Navagation(driver, path, url, intNumber, threads);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        driver.Quit();
                    }
                }
            }

        }

        private void Navagation(IWebDriver driver, string path, string url, int processNumber, int threads)
        {
            try
            {
                ExecuteContinue(delegate { GoToHomePage(driver, url); }, out bool isPage);
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

        private void GoToHomePage(IWebDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
            }
            catch (Exception e)
            {
                throw new Exception(" - Message: " + e.Message);
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
            catch (Exception e)
            {
                throw new Exception(" # Message: " + e.Message);
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

                        CreateFile(html, name, path);

                        driver.FindElement(By.Id("btnVoltar")).Click();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateFile(string value, string name, string directory)
        {
            string path = $"{directory}/{name}.txt";

            using (StreamWriter sw = File.CreateText(path))
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