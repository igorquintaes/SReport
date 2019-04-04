using OpenQA.Selenium;
using System;
using System.Linq;

namespace SReportLib
{
    public class SReportBuilder
    {
        private readonly SReport sReport;

        private SReportBuilder(IWebDriver driver, string logsPath) => 
            sReport = new SReport(driver, logsPath);

        public static SReportBuilder Setup(IWebDriver driver, string logsPath) => 
            new SReportBuilder(driver, logsPath);

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
