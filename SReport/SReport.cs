using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;

namespace SReportLib
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

            if (sLogs.HasFlag(SLog.PageHtml))
            {
                using (var webClient = new System.Net.WebClient())
                using (var stream = File.CreateText(Path.Combine(tesLogFolder, $"{nameof(SLog.PageHtml)}.htm")))
                {
                    var template = new HtmlDocument();
                    template.LoadHtml(_driver.PageSource);
                    var nodes = template.DocumentNode.SelectNodes("//link[not(@href='')][@rel='stylesheet']");
                    if (nodes?.Any() != true)
                        return;

                    foreach (var node in nodes)
                    {
                        var attr = node.GetAttributeValue("href", "");
                        if (attr.StartsWith("data:text/css"))
                            continue;

                        var isUri = !Uri.TryCreate(attr, UriKind.RelativeOrAbsolute, out var uri);
                        if (!isUri)
                        {
                            Uri.TryCreate(_driver.Url, UriKind.RelativeOrAbsolute, out var newUri);
                            attr = $@"{newUri.Authority}/{uri}";
                        }

                        if (!attr.StartsWith("http://") || !attr.StartsWith("https://"))
                        {
                            attr = $@"http://{attr}";
                        }

                        var cssValue = webClient.DownloadString(attr);
                        var newElement = template.CreateElement("style");
                        newElement.InnerHtml = cssValue;
                        newElement.Attributes.Add("type", "text/css");

                        node.ParentNode.ReplaceChild(newElement, node);
                    }

                    stream.WriteLine(template.DocumentNode.InnerHtml);
                }
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
        }

        private void SeleniumLog(string folder, string logType)
        {
            if (!_driver.Manage().Logs.AvailableLogTypes.Any(x => x == logType))
                return;

            var logs = _driver.Manage().Logs.GetLog(logType)?.OrderBy(x => x.Timestamp);
            if (logs?.Any() == true)
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
