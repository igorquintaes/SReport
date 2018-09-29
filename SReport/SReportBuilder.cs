using OpenQA.Selenium;
using System;
using System.Linq;

namespace SReportLog
{
    public class SReportBuilder
    {
        private readonly SReport sReport;

        private SReportBuilder(IWebDriver driver, string logsPath, string template) => 
            sReport = new SReport(driver, logsPath, template);

        public static SReportBuilder Setup(IWebDriver driver, string logsPath, string template = null) => 
            new SReportBuilder(driver, logsPath, template);

        public SReportBuilder AddLog(params SLog[] logs)
        {
            foreach(var log in logs)
            {
                sReport.sLogs |= log;
            }

            return this;
        }

        public SReportBuilder AddAllLogs() =>
            AddLog(Enum.GetValues(typeof(SLog)).Cast<SLog>().ToArray());

        public SReport Build() => 
            sReport;
    }
}
