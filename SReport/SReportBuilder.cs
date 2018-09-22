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

        public SReportBuilder WithPageStateHtml()
        {
            sReport.PageStateHtml = true;
            return this;
        }

        public SReportBuilder WithGeneralInfo()
        {
            sReport.GeneralInfo = true;
            return this;
        }

        public SReport Build() => 
            sReport;
    }
}
