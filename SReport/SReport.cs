using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;

namespace SReport
{
    public class SReport
    {
        private readonly IWebDriver _driver;

        internal SLog sLogs;

        public readonly string LogsPath;

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
            
            if (sLogs.HasFlag(SLog.Screenshoot))
            {
                var screenshot = (_driver as ITakesScreenshot).GetScreenshot();
                screenshot.SaveAsFile(Path.Combine(tesLogFolder, $"{nameof(SLog.Screenshoot)}.png"), ScreenshotImageFormat.Png);
            }

            if (sLogs.HasFlag(SLog.BrowserConsole))
            {
                SeleniumLog(tesLogFolder, LogType.Browser);
            }

            if (sLogs.HasFlag(SLog.SeleniumClient))
            {
                SeleniumLog(tesLogFolder, LogType.Client);
            }

            if (sLogs.HasFlag(SLog.WebDriverInstance))
            {
                SeleniumLog(tesLogFolder, LogType.Driver);
            }

            if (sLogs.HasFlag(SLog.Profiling))
            {
                SeleniumLog(tesLogFolder, LogType.Profiler);
            }

            if (sLogs.HasFlag(SLog.ServerMessages))
            {
                SeleniumLog(tesLogFolder, LogType.Server);
            }

            if (sLogs.HasFlag(SLog.PageHtml))
            {
                var styles = _driver.FindElements(By.TagName("link"))
                    .Where(x => !string.IsNullOrWhiteSpace(x.GetAttribute("href")) && x.GetAttribute("rel").ToLower() == "stylesheet")
                    .Select(x => x.GetAttribute("href"));

                using (var stream = File.CreateText(Path.Combine(tesLogFolder, $"{nameof(SLog.PageHtml)}.htm")))
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
