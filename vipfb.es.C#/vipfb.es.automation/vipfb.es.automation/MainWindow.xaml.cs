using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static IWebElement img;
        static Bitmap image, cropImg, image1, cropImg1;
        static BitmapImage bmpImg, bmpImg1;
        static string clientKey;
        static string resultText, resultText1, idText;

        static int timeWait;
        int getIDFromPage = 0;
        int count = 0, total, pass = 0;
        Boolean gotResult = false;
        public static int xval, yval;

        private readonly BackgroundWorker automationJob = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (usePageID.IsChecked == true)
            //{
            //    getID();
            //}

            //processCaptcha();
            //ExampleImageToText();
            //fillAndSubmitPage1();
            //fillAndSubmitPage2();
            //driver = new FirefoxDriver();
            clientTokenBox.Dispatcher.Invoke(new Action(() => clientKey = clientTokenBox.Text));
            automationJob.RunWorkerAsync();
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            processCaptcha();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //driver = new FirefoxDriver();
            options = new FirefoxOptions();
            options.AddAdditionalCapability(CapabilityType.Version, "latest", true);
            options.AddAdditionalCapability(CapabilityType.Platform, "WIN10", true);
            options.AddAdditionalCapability("key", "key", true);
            options.AddAdditionalCapability("secret", "secret", true);
            options.AddAdditionalCapability("name", this.Name, true);

            ////
            inputTokenBox.TextWrapping = TextWrapping.NoWrap;
            inputTokenBox.AcceptsReturn = true;

            ///// Run Background Woker
            automationJob.DoWork += automationDoWork;
            automationJob.RunWorkerCompleted += automationJobRunWokerCompleted;
        }

        private void automationJobRunWokerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void automationDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var item in tokenList.Items)
            {
                string token = (String)item;
                count++;
                currentToken.Dispatcher.Invoke(new Action(() => currentToken.Content = count.ToString() 
                                                                 + "/" + total.ToString()));
                

                try
                {
                    driver = new FirefoxDriver();
                    if (getIDFromPage == 1)
                    {
                        getID();
                    }
                    gotResult = false;
                    processCaptcha();
                    ExampleImageToText();
                    fillAndSubmitPage1(token);
                    while (!gotResult) ;
                    IWebElement texth5 = driver.FindElement(By.XPath("//h5[.='Select our services']"));
                    Console.WriteLine("passed");
                    if (texth5.Text != "")
                    {
                        fillAndSubmitPage2();
                    }
                    getResponse();
                }
                catch (Exception ex)
                {
                    driver.Close();
                    Console.WriteLine(ex);
                }
                
            }
            
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            automationJob.CancelAsync();
        }




        private void getResponse()
        {
            
            if (pass == 1)
            {
                return;
            }
           
            {
                Console.WriteLine("Waiting");
                timeWaitBox.Dispatcher.Invoke(new Action(() => timeWait = Convert.ToInt32(timeWaitBox.Text)));
                Thread.Sleep(timeWait * 1000);

                try
                {
                    IWebElement response = driver.FindElement(By.TagName("font"));

                    responseBox.Dispatcher.Invoke(new Action(() => responseBox.Text = response.GetAttribute("InnerHTML")));


                    driver.Navigate().GoToUrl("https://viplikestar.com/next.php");
                    Thread.Sleep(1000);
                    
                }
                catch (Exception ex)
                {
                    responseBox.Dispatcher.Invoke(new Action(() => responseBox.Text = responseBox.Text = "Fail"));

                }
            }
            driver.Close();


        }
        private void fillAndSubmitPage2()
        {
            driver.Navigate().GoToUrl("https://vipfb.es/Request");
            Thread.Sleep(1000);

            pass = 0;
            try
            {
                IWebElement countdown = driver.FindElement(By.Id("countdown"));
                Console.WriteLine(countdown.Text);
                if (countdown.Text.Contains("Please wait"))
                {
                    pass = 1;
                    driver.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                pass = 0;
            }

            ITakesScreenshot screenShotDriver = driver as ITakesScreenshot;
            Screenshot ss = screenShotDriver.GetScreenshot();
            ss.SaveAsFile("captcha1.png", OpenQA.Selenium.ScreenshotImageFormat.Png);

            img = driver.FindElement(By.TagName("img"));
            image1 = new Bitmap("captcha1.png");
            //MessageBox.Show(img.Location.ToString());
            x1.Dispatcher.Invoke(new Action(() => xval = Convert.ToInt16(x1.Text)));
            y1.Dispatcher.Invoke(new Action(() => yval = Convert.ToInt16(y1.Text)));
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(xval,
                 yval,
                img.Size.Width + 50, img.Size.Height + 50);
            string path = Directory.GetCurrentDirectory();

            try
            {
                cropImg1 = image1.Clone(rect, image.PixelFormat);
                cropImg1.Save("Crop2.png", ImageFormat.Png);

                var stream = File.OpenRead("Crop2.png");
                bmpImg1 = new BitmapImage();
                bmpImg1.BeginInit();
                bmpImg1.StreamSource = stream;
                bmpImg1.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg1.EndInit();
                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke((Action)(() => captchaImg1.Source = bmpImg1));
                if (File.Exists(@"Crop3.png"))
                {
                    File.Delete(@"Crop3.png");
                }
                stream.Dispose();
            }
            catch (Exception ex)
            {
                //cropImg1.Save("Crop3.png", ImageFormat.Png);

                //var stream = File.OpenRead("Crop3.png");
                //bmpImg1 = new BitmapImage();
                //bmpImg1.BeginInit();
                //bmpImg1.StreamSource = stream;
                //bmpImg1.CacheOption = BitmapCacheOption.OnLoad;
                //bmpImg1.EndInit();
                //captchaImg1.Dispatcher.BeginInvoke(new Action(() => captchaImg1.Source = bmpImg1));
                //if (File.Exists(@"Crop2.png"))
                //{
                //    File.Delete(@"Crop2.png");
                //}
                //stream.Dispose();
            }


            //MessageBox.Show(path);



            image1.Dispose();
            cropImg1.Dispose();
            //bmpImg.Freeze();


            gotResult = false;
            ExampleImageToText1();
            while (!gotResult) ;
            ///////////// Find 2 input ////////////
            ReadOnlyCollection<IWebElement> inputs = driver.FindElements(By.TagName("input"));

            inputs[0].SendKeys(resultText1);
            inputs[1].Clear();
            inputs[1].SendKeys(idText);

            IWebElement button = driver.FindElement(By.XPath("//button[@type='submit']"));
            button.Click();
        }
        private void fillAndSubmitPage1(string token)
        {
            IWebElement captchaField = driver.FindElement(By.Name("cpth"));
            captchaField.SendKeys(resultText);

            IWebElement tokenID = driver.FindElement(By.Id("wtoken"));
            tokenID.SendKeys(token);

            Thread.Sleep(3000);

            IWebElement submitBtn = driver.FindElement(By.XPath("//button[.=' Submit ']"));
            //MessageBox.Show(submitBtn.GetAttribute("InnerHTML"));
            submitBtn.Click();
        }

        private void UsePageID_Unchecked(object sender, RoutedEventArgs e)
        {
            getIDFromPage = 0;
        }

        private void ExampleImageToText1()
        {
            DebugHelper.VerboseMode = true;
            var api = new ImageToText
            {
                ClientKey = clientKey,
                FilePath = "Crop2.png"
            };

            if (!api.CreateTask())
                DebugHelper.Out("API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            else if (!api.WaitForResult())
                DebugHelper.Out("Could not solve the captcha.", DebugHelper.Type.Error);
            else
            {
                DebugHelper.Out("Result: " + api.GetTaskSolution().Text, DebugHelper.Type.Success);
                resultBox1.Dispatcher.BeginInvoke(new Action(() => resultBox1.Text = api.GetTaskSolution().Text));
                resultText1 = api.GetTaskSolution().Text;
                gotResult = true;
            }

        }

        private void TestBtn2_Click(object sender, RoutedEventArgs e)
        {
            driver.Navigate().GoToUrl("https://vipfb.es/Request");
            Thread.Sleep(1000);

            ITakesScreenshot screenShotDriver = driver as ITakesScreenshot;
            Screenshot ss = screenShotDriver.GetScreenshot();
            ss.SaveAsFile("captcha1.png", OpenQA.Selenium.ScreenshotImageFormat.Png);

            img = driver.FindElement(By.TagName("img"));

            image1 = new Bitmap("captcha1.png");
            x1.Dispatcher.Invoke(new Action(() => xval = Convert.ToInt16(x1.Text)));
            y1.Dispatcher.Invoke(new Action(() => yval = Convert.ToInt16(y1.Text)));

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(xval,
                 yval,
                img.Size.Width + 50, img.Size.Height);
            string path = Directory.GetCurrentDirectory();

            try
            {
                cropImg1 = image1.Clone(rect, image.PixelFormat);
                cropImg1.Save("Crop2.png", ImageFormat.Png);

                var stream = File.OpenRead("Crop2.png");
                bmpImg1 = new BitmapImage();
                bmpImg1.BeginInit();
                bmpImg1.StreamSource = stream;
                bmpImg1.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg1.EndInit();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke((Action)(() => captchaImg1.Source = bmpImg1));
                if (File.Exists(@"Crop3.png"))
                {
                    File.Delete(@"Crop3.png");
                }
                stream.Dispose();
            }
            catch (Exception ex)
            {
                //cropImg1.Save("Crop3.png", ImageFormat.Png);

                //var stream = File.OpenRead("Crop3.png");
                //bmpImg1 = new BitmapImage();
                //bmpImg1.BeginInit();
                //bmpImg1.StreamSource = stream;
                //bmpImg1.CacheOption = BitmapCacheOption.OnLoad;
                //bmpImg1.EndInit();
                //captchaImg1.Dispatcher.BeginInvoke(new Action(() => captchaImg1.Source = bmpImg1));
                //if (File.Exists(@"Crop2.png"))
                //{
                //    File.Delete(@"Crop2.png");
                //}
                //stream.Dispose();
            }


            //MessageBox.Show(path);



            image1.Dispose();
            cropImg1.Dispose();
            //bmpImg.Freeze();


            ///////////// Find 2 input ////////////
            ReadOnlyCollection<IWebElement> inputs = driver.FindElements(By.TagName("input"));

            inputs[0].SendKeys(resultBox1.Text);
            inputs[1].Clear();
            inputs[1].SendKeys(idText);
            Thread.Sleep(2000);
            IWebElement button = driver.FindElement(By.XPath("//button[@type='submit']"));
            button.Click();
            
        }

        private void UsePageID_Checked(object sender, RoutedEventArgs e)
        {
            getIDFromPage = 1;
        }

        private void ExampleImageToText()
        {

            DebugHelper.VerboseMode = true;
            var api = new ImageToText
            {
                ClientKey = clientKey,
                FilePath = "Crop.png"
            };

            if (!api.CreateTask())
                DebugHelper.Out("API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            else if (!api.WaitForResult())
                DebugHelper.Out("Could not solve the captcha.", DebugHelper.Type.Error);
            else
            {   
                DebugHelper.Out("Result: " + api.GetTaskSolution().Text, DebugHelper.Type.Success);
                resultBox.Dispatcher.BeginInvoke(new Action(() => resultBox.Text = api.GetTaskSolution().Text));
                resultText = api.GetTaskSolution().Text;
                gotResult = true;
            }
               
        }
        private void getID()
        {
            driver.Navigate().GoToUrl("https://viplikestar.com/id.php");
            //MessageBox.Show(driver.PageSource);

            IWebElement id = driver.FindElement(By.TagName("body"));
            string idStr = id.GetAttribute("innerHTML");
            //MessageBox.Show(idStr);
            idBox.Dispatcher.Invoke(new Action(() => idBox.Text = idStr));
            idText = idStr;
        }

        private void InputBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(inputTokenBox.LineCount);
            total = inputTokenBox.LineCount;
            string[] lines = Regex.Split(inputTokenBox.Text, "\r\n");
            for (int i = 0; i < inputTokenBox.LineCount; i++)
            {
                tokenList.Items.Add(lines[i]);
            }
            
        }

        private void processCaptcha()
        {
            driver.Navigate().GoToUrl("https://vipfb.es/");
            //Resize to 1920 x 1080
            //System.Drawing.Size windowSize = new System.Drawing.Size(1550, 838);
            //driver.Manage().Window.Size = windowSize;
            driver.Manage().Window.Maximize();
            Console.WriteLine("Size: " + driver.Manage().Window.Size.Height + " " + driver.Manage().Window.Size.Width);

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

            // Get x, y in invoke
            x.Dispatcher.Invoke(new Action(() => xval = Convert.ToInt16(x.Text)));
            y.Dispatcher.Invoke(new Action(() => yval = Convert.ToInt16(y.Text)));

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X + xval,
                p.Y - yval,
                img.Size.Width + 50, img.Size.Height + 100);
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

                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke((Action)(() => captchaImg.Source = bmpImg));

                if (File.Exists(@"Crop1.png"))
                {
                    File.Delete(@"Crop1.png");
                }
                stream.Dispose();
            }
            catch (Exception ex)
            {
                //cropImg.Save("Crop1.png", ImageFormat.Png);

                //var stream = File.OpenRead("Crop1.png");
                //bmpImg = new BitmapImage();
                //bmpImg.BeginInit();
                //bmpImg.StreamSource = stream;
                //bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                //bmpImg.EndInit();
                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke((Action)(() => captchaImg.Source = bmpImg));
                //if (File.Exists(@"Crop.png"))
                //{
                //    File.Delete(@"Crop.png");
                //}
                //stream.Dispose();
            }


            //MessageBox.Show(path);



            image.Dispose();
            cropImg.Dispose();
            //bmpImg.Freeze();
        }
    }
}
