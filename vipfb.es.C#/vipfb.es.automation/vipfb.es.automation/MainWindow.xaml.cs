using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Anticaptcha_example.Api;
using Anticaptcha_example.Helper;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace vipfb.es.automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Global variables
        private IWebDriver driver;
        private FirefoxOptions options;
        private IWebElement img;
        Actions actions;
        Bitmap image, cropImg;
        BitmapImage bmpImg;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            getID();
            processCaptcha();
            ExampleImageToText();
            fillAndSubmitPage1();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            driver = new FirefoxDriver();
            options = new FirefoxOptions();
            options.AddAdditionalCapability(CapabilityType.Version, "latest", true);
            options.AddAdditionalCapability(CapabilityType.Platform, "WIN10", true);
            options.AddAdditionalCapability("key", "key", true);
            options.AddAdditionalCapability("secret", "secret", true);
            options.AddAdditionalCapability("name", this.Name, true);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            driver.Close();
        }





        private void fillAndSubmitPage1()
        {
            IWebElement captchaField = driver.FindElement(By.Name("cpth"));
            captchaField.SendKeys(resultBox.Text);

            IWebElement tokenID = driver.FindElement(By.Id("wtoken"));
            tokenID.SendKeys(idBox.Text);

            IWebElement submitBtn = driver.FindElement(By.XPath("//button[.=' Submit ']"));
            MessageBox.Show(submitBtn.GetAttribute("InnerHTML"));
            submitBtn.Click();
        }
        private void ExampleImageToText()
        {
            DebugHelper.VerboseMode = true;
            var api = new ImageToText
            {
                ClientKey = clientTokenBox.Text,
                FilePath = "Crop.png"
            };

            if (!api.CreateTask())
                DebugHelper.Out("API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            else if (!api.WaitForResult())
                DebugHelper.Out("Could not solve the captcha.", DebugHelper.Type.Error);
            else
            {   
                DebugHelper.Out("Result: " + api.GetTaskSolution().Text, DebugHelper.Type.Success);
                resultBox.Text = api.GetTaskSolution().Text;
            }
               
        }
        private void getID()
        {
            driver.Navigate().GoToUrl("https://viplikestar.com/id.php");
            //MessageBox.Show(driver.PageSource);

            IWebElement id = driver.FindElement(By.TagName("body"));
            string idStr = id.GetAttribute("innerHTML");
            //MessageBox.Show(idStr);
            idBox.Text = idStr;
        }

        private void processCaptcha()
        {
            driver.Navigate().GoToUrl("https://vipfb.es/");
            driver.Manage().Window.Maximize();

            img = driver.FindElement(By.TagName("img"));

            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'})", img);

            Thread.Sleep(2000);

            ITakesScreenshot screenShotDriver = driver as ITakesScreenshot;
            Screenshot ss = screenShotDriver.GetScreenshot();
            ss.SaveAsFile("captcha.png", OpenQA.Selenium.ScreenshotImageFormat.Png);

            image = new Bitmap("captcha.png");
            System.Drawing.Point p = img.Location;
            //MessageBox.Show(img.Location.ToString());
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X + Convert.ToInt16(x.Text),
                p.Y - Convert.ToInt16(y.Text),
                img.Size.Width, img.Size.Height);
            string path = Directory.GetCurrentDirectory();
            try
            {
                cropImg = image.Clone(rect, image.PixelFormat);
                cropImg.Save("Crop.png", ImageFormat.Png);

                var stream = File.OpenRead("Crop.png");
                bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.StreamSource = stream;
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.EndInit();
                captchaImg.Source = bmpImg;
                if (File.Exists(@"Crop1.png"))
                {
                    File.Delete(@"Crop1.png");
                }
                stream.Dispose();
            }
            catch (Exception ex)
            {
                cropImg.Save("Crop1.png", ImageFormat.Png);

                var stream = File.OpenRead("Crop1.png");
                bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.StreamSource = stream;
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.EndInit();
                captchaImg.Source = bmpImg;
                if (File.Exists(@"Crop.png"))
                {
                    File.Delete(@"Crop.png");
                }
                stream.Dispose();
            }


            //MessageBox.Show(path);



            image.Dispose();
            cropImg.Dispose();
            //bmpImg.Freeze();
        }
    }
}
