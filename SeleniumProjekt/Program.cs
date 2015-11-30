using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumProjekt
{
    class Program
    {
        private const string Url = "http://localhost/3semesterprojekt/public/index.php";
        private const string Placering = "C:/temp/Selenium/";
        static void Main(string[] args)
        {
            using (IWebDriver webDriver = new ChromeDriver())
            {
                webDriver.Navigate().GoToUrl(Url);
                webDriver.Manage().Window.Size = new Size(420, 665);
                webDriver.FindElement(By.XPath("/html/body/div/form/button")).Click();

                Random random = new Random();
                TagSceenshot(webDriver, Placering + "screenshot-" + random.Next(1000, 9999) + ".png");
                webDriver.FindElement(By.XPath("/html/body/div/nav/div/div[1]/button")).Click();
                Thread.Sleep(1000   );
                TagSceenshot(webDriver, Placering + "screenshot-" + random.Next(1000, 9999) + ".png");

                webDriver.Navigate().GoToUrl(Url + "/statistik/linjegraf/dato/2015");
                TagSceenshot(webDriver, Placering + "screenshot-" + random.Next(1000, 9999) + ".png");

                webDriver.Navigate().GoToUrl(Url + "/statistik/linjegraf/temperatur");
                TagSceenshot(webDriver, Placering + "screenshot-" + random.Next(1000, 9999) + ".png");

                webDriver.Navigate().GoToUrl(Url + "/historik");
                TagSceenshot(webDriver, Placering + "screenshot-" + random.Next(1000, 9999) + ".png");
            }
        }
        public static void TagSceenshot(IWebDriver driver, string placering)
        {
            ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;
            Screenshot screenshot = screenshotDriver.GetScreenshot();
            screenshot.SaveAsFile(placering, ImageFormat.Png);
        }
    }
}
