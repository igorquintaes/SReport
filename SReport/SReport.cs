using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SReportLog
{
    public class SReport
    {
        private readonly IWebDriver _driver;

        internal SLog sLogs;

        public readonly string LogsPath;
        public string LogTemplatePath => Path.Combine(LogsPath, "sReport.htm");

        internal SReport(IWebDriver driver, string logsPath, string templatePath)
        {
            LogsPath = logsPath;
            _driver = driver;

            if (Directory.Exists(logsPath))
                Directory.Delete(logsPath, true);

            do
            {
                Thread.Sleep(100);
            } while (Directory.Exists(logsPath));

            Directory.CreateDirectory(logsPath);

           var template = (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath)
                    ? null
                    : File.ReadAllText(templatePath))
                ?? Resources.sreport_template;

            using (var stream = File.CreateText(LogTemplatePath))
            {
                stream.Write(template);
            }
        }

        public void Log(string testCompleteName, bool adjustTestNamespace = true)
        {
            if (_driver == null)
                return;

            var testNameWithoutNamespaceString = testCompleteName;
            var testNamespaceString = testCompleteName;

            if (adjustTestNamespace)
            {
                int index = testCompleteName.LastIndexOf(".");
                if (index > 0)
                {
                    testNamespaceString = testCompleteName.Substring(0, index);
                    testNameWithoutNamespaceString = testCompleteName.Replace($"{testNamespaceString}.", "");
                }
            }

            var testLogFolder = Path.Combine(LogsPath, testCompleteName);
            Directory.CreateDirectory(testLogFolder);

            var template = new HtmlDocument();
            template.Load(LogTemplatePath);
            var logList = template.DocumentNode.SelectSingleNode("//*[@id='test-list']");
            logList.AppendChild(HtmlNode.CreateNode($"<li>{testNameWithoutNamespaceString}</li>"));

            var testNamespace = template.DocumentNode.SelectSingleNode("//*[@id='test-namespace']");
            testNamespace.InnerHtml = testNamespaceString;

            var testLastName = template.DocumentNode.SelectSingleNode("//*[@id='test-last-name']");
            testLastName.InnerHtml = testNameWithoutNamespaceString;

            var testDateTime = template.DocumentNode.SelectSingleNode("//*[@id='test-date-time']");
            testDateTime.InnerHtml = $"Date: {DateTime.Now.ToLongDateString()} - {DateTime.Now.ToLongTimeString()}";


            if (sLogs.HasFlag(SLog.Screenshoot))
            {
                var screenshot = (_driver as ITakesScreenshot).GetScreenshot();
                screenshot.SaveAsFile(Path.Combine(testLogFolder, $"{nameof(SLog.Screenshoot)}.png"), ScreenshotImageFormat.Png);

                LogTestItem(testCompleteName, $"{nameof(SLog.Screenshoot)}.png", template);
            }

            if (sLogs.HasFlag(SLog.BrowserConsole))
            {
                SeleniumLog(testLogFolder, LogType.Browser, testCompleteName, template);
            }

            if (sLogs.HasFlag(SLog.SeleniumClient))
            {
                SeleniumLog(testLogFolder, LogType.Client, testCompleteName, template);
            }

            if (sLogs.HasFlag(SLog.WebDriverInstance))
            {
                SeleniumLog(testLogFolder, LogType.Driver, testCompleteName, template);
            }

            if (sLogs.HasFlag(SLog.Profiling))
            {
                SeleniumLog(testLogFolder, LogType.Profiler, testCompleteName, template);
            }

            if (sLogs.HasFlag(SLog.ServerMessages))
            {
                SeleniumLog(testLogFolder, LogType.Server, testCompleteName, template);
            }

            if (sLogs.HasFlag(SLog.PageHtml))
            {
                var styles = _driver.FindElements(By.TagName("link"))
                    .Where(x => !string.IsNullOrWhiteSpace(x.GetAttribute("href")) && x.GetAttribute("rel").ToLower() == "stylesheet")
                    .Select(x => x.GetAttribute("href"));
                    
                using (var stream = File.CreateText(Path.Combine(testLogFolder, $"{nameof(SLog.PageHtml)}.htm")))
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

                LogTestItem(testCompleteName, $"{nameof(SLog.PageHtml)}.htm", template);
            }

            template.Save(LogTemplatePath);
        }

        private void SeleniumLog(string folder, string logType, string testName, HtmlDocument template)
        {
            var logs = _driver.Manage().Logs.GetLog(logType).OrderBy(x => x.Timestamp);
            if (logs.Any())
            {
                using (var stream = File.CreateText(Path.Combine(folder, $"{logType}.txt")))
                {
                    foreach (var log in logs)
                    {
                        stream.WriteLine($"DateTime: {log.Timestamp.ToLongTimeString()}");
                        stream.WriteLine($"Level: {log.Level}");
                        stream.WriteLine($"Message: {log.Message} {Environment.NewLine}");
                    }
                }

                LogTestItem(testName, $"{logType}.txt", template);
            }
        }

        private void LogTestItem(string testName, string logName, HtmlDocument template)
        {
            var logsNode = template.DocumentNode.SelectSingleNode("//*[@id='test-log-list']");
            logsNode.AppendChild(HtmlNode.CreateNode($@"<li><a target=""_blank"" href=""{testName}/{logName}"">{logName}</a></li>"));
        }
    }
}
