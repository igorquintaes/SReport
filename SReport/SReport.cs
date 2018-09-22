using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SReportLog
{
    public class SReport
    {
        private readonly IWebDriver _driver;

        public readonly string LogsPath;
        public bool Screenshoot{get; internal set;}
        public bool BrowserConsole { get; internal set; }
        public bool GeneralInfo { get; internal set; }
        public bool PageStateHtml { get; internal set; }
        public bool SeleniumClient { get; internal set; }
        public bool WebDriverInstance { get; internal set; }
        public bool Profiling { get; internal set; }
        public bool ServerMessages { get; internal set; }

        internal SReport(IWebDriver driver, string logsPath)
        {
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);

            LogsPath = logsPath;
            _driver = driver;
        }

        public void Log(string testName)
        {
            if (_driver == null)
                return;

            var tesLogFolder = Path.Combine(LogsPath, testName);

            if (!Directory.Exists(tesLogFolder))
                Directory.CreateDirectory(tesLogFolder);
            else
            {
                var files = (new DirectoryInfo(tesLogFolder)).GetFiles();
                foreach (var file in files)
                    file.Delete();
            }

            if (GeneralInfo)
            {
                using (var stream = File.CreateText(Path.Combine(tesLogFolder, $"{nameof(GeneralInfo)}.txt")))
                {
                    stream.WriteLine($"Date: {DateTime.Now.ToLongDateString()} - {DateTime.Now.ToLongTimeString()}");
                    stream.WriteLine($"Test name: {testName} {Environment.NewLine}");
                    stream.WriteLine($"{RuntimeInformation.OSDescription}");
                    stream.WriteLine($"{RuntimeInformation.FrameworkDescription}");
                }
            }

            if (Screenshoot)
            {
                var screenshot = (_driver as ITakesScreenshot).GetScreenshot();
                screenshot.SaveAsFile(Path.Combine(tesLogFolder, $"{nameof(Screenshoot)}.png"), ScreenshotImageFormat.Png);
            }

            if (BrowserConsole)
            {
                SeleniumLog(tesLogFolder, LogType.Browser);
            }

            if (SeleniumClient)
            {
                SeleniumLog(tesLogFolder, LogType.Client);
            }

            if (WebDriverInstance)
            {
                SeleniumLog(tesLogFolder, LogType.Driver);
            }

            if (Profiling)
            {
                SeleniumLog(tesLogFolder, LogType.Profiler);
            }

            if (ServerMessages)
            {
                SeleniumLog(tesLogFolder, LogType.Server);
            }

            if (PageStateHtml)
            {
                var styles = _driver.FindElements(By.TagName("link"))
                    .Where(x => !string.IsNullOrWhiteSpace(x.GetAttribute("href")) && x.GetAttribute("rel").ToLower() == "stylesheet")
                    .Select(x => x.GetAttribute("href"));

                using (var stream = File.CreateText(Path.Combine(tesLogFolder, $"{nameof(PageStateHtml)}.htm")))
                {
                    stream.WriteLine(_driver.PageSource);
                    foreach(var style in styles)
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            try
                            {
                                stream.WriteLine($@"<style type=""text/css"">{webClient.DownloadString(style)}</style>");
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void SeleniumLog(string folder, string logType)
        {
            var logs = _driver.Manage().Logs.GetLog(logType).OrderBy(x => x.Timestamp);
            if (logs.Any())
            {
                using (var stream = File.CreateText(Path.Combine(folder, $"{nameof(logType)}.txt")))
                {
                    foreach (var log in logs)
                    {
                        stream.WriteLine($"DateTime: {log.Timestamp.ToLongTimeString()}");
                        stream.WriteLine($"Level: {log.Level}");
                        stream.WriteLine($"Message: {log.Message} {Environment.NewLine}");
                    }
                }
            }
        }
    }
}
