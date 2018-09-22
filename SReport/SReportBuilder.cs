using OpenQA.Selenium;

namespace SReportLog
{
    public class SReportBuilder
    {
        private readonly SReport sReport;

        private SReportBuilder(IWebDriver driver, string logsPath) => 
            sReport = new SReport(driver, logsPath);

        public static SReportBuilder Setup(IWebDriver driver, string logsPath) => 
            new SReportBuilder(driver, logsPath);

        public SReportBuilder WithScreenshoot()
        {
            sReport.Screenshoot = true;
            return this;
        }

        public SReportBuilder WithConsoleLogs()
        {
            sReport.BrowserConsole = true;
            return this;
        }

        public SReportBuilder WithProfilingLogs()
        {
            sReport.Profiling = true;
            return this;
        }

        public SReportBuilder WithServerMessagesLogs()
        {
            sReport.ServerMessages = true;
            return this;
        }

        public SReportBuilder WithSeleniumClientLogs()
        {
            sReport.SeleniumClient = true;
            return this;
        }

        public SReportBuilder WithWebDriverInstanceLogs()
        {
            sReport.WebDriverInstance = true;
            return this;
        }

        public SReportBuilder WithPageStateHtmlLogs()
        {
            sReport.PageStateHtml = true;
            return this;
        }

        public SReportBuilder WithGeneralInfoLogs()
        {
            sReport.GeneralInfo = true;
            return this;
        }

        public SReportBuilder WithAllLogsEnabled()
        {
            sReport.BrowserConsole = true;
            sReport.GeneralInfo = true;
            sReport.PageStateHtml = true;
            sReport.Profiling = true;
            sReport.Screenshoot = true;
            sReport.SeleniumClient = true;
            sReport.ServerMessages = true;
            sReport.WebDriverInstance = true;
            return this;
        }

        public SReport Build() => 
            sReport;
    }
}
