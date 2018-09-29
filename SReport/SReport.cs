using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;

namespace SReportLog
{
    public class SReport
    {
        private readonly IWebDriver _driver;
        private string _templatePath;

        internal SLog sLogs;

        public readonly string LogsPath;
        public string LogTemplatePath => Path.Combine(LogsPath, "sReport.htm");

        internal SReport(IWebDriver driver, string logsPath, string templatePath)
        {
            LogsPath = logsPath;
            _driver = driver;

            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);
            else
            {
                var files = (new DirectoryInfo(logsPath)).GetFiles();
                foreach (var file in files)
                    file.Delete();

                var dirs = (new DirectoryInfo(logsPath)).GetDirectories();
                foreach (var dir in dirs)
                    dir.Delete();
            }

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
            template.Save($"{LogTemplatePath}l");
                
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
            }

            if (sLogs.HasFlag(SLog.BrowserConsole))
            {
                SeleniumLog(testLogFolder, LogType.Browser);
            }

            if (sLogs.HasFlag(SLog.SeleniumClient))
            {
                SeleniumLog(testLogFolder, LogType.Client);
            }

            if (sLogs.HasFlag(SLog.WebDriverInstance))
            {
                SeleniumLog(testLogFolder, LogType.Driver);
            }

            if (sLogs.HasFlag(SLog.Profiling))
            {
                SeleniumLog(testLogFolder, LogType.Profiler);
            }

            if (sLogs.HasFlag(SLog.ServerMessages))
            {
                SeleniumLog(testLogFolder, LogType.Server);
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
            }

            template.Save(LogTemplatePath);
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
