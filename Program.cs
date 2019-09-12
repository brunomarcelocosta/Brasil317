using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace console
{
    class Program
    {

        static void Main(string[] args)
        {
            var value = Environment.GetEnvironmentVariable("CHROME_THD");
            if (value == null)
            {
                Environment.SetEnvironmentVariable("CHROME_THD", "10");
                value = Environment.GetEnvironmentVariable("CHROME_THD");

            }

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();

            var path = $"{config["path"]}";

            DeleteFiles(path);

            new Teste.Service.ServiceBase().StartMethod(value);
        }

        public static void DeleteFiles(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
