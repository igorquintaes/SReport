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

            var tesLogtFolder = Path.Combine(LogsPath, testName);

            if (!Directory.Exists(tesLogtFolder))
                Directory.CreateDirectory(tesLogtFolder);
            else
            {
                var files = (new DirectoryInfo(tesLogtFolder)).GetFiles();
                foreach (var file in files)
                    file.Delete();
            }

            if (GeneralInfo)
            {
                using (var stream = File.CreateText(Path.Combine(tesLogtFolder, $"{nameof(GeneralInfo)}.txt")))
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
                screenshot.SaveAsFile(Path.Combine(tesLogtFolder, $"{nameof(Screenshoot)}.png"), ScreenshotImageFormat.Png);
            }

            if (BrowserConsole)
            {
                var browserLogs = _driver.Manage().Logs.GetLog(LogType.Browser).OrderBy(x => x.Timestamp);
                if (browserLogs.Any())
                {
                    using (var stream = File.CreateText(Path.Combine(tesLogtFolder, $"{nameof(BrowserConsole)}.txt")))
                    {
                        foreach (var browserLog in browserLogs)
                        {
                            stream.WriteLine($"DateTime: {browserLog.Timestamp.ToLongTimeString()}");
                            stream.WriteLine($"Level: {browserLog.Level}");
                            stream.WriteLine($"Message: {browserLog.Message} {Environment.NewLine}");
                        }
                    }
                }
            }

            if (PageStateHtml)
            {
                var styles = _driver.FindElements(By.TagName("link"))
                    .Where(x => !string.IsNullOrWhiteSpace(x.GetAttribute("href")) && x.GetAttribute("rel").ToLower() == "stylesheet")
                    .Select(x => x.GetAttribute("href"));

                using (var stream = File.CreateText(Path.Combine(tesLogtFolder, $"{nameof(PageStateHtml)}.htm")))
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
    }
}
