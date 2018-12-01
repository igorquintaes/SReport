using System;

namespace SReport
{
    [Flags]
    public enum SLog
    {
        Screenshoot        = 0b0000001,
        BrowserConsole     = 0b0000010,
        PageHtml           = 0b0000100,
        SeleniumClient     = 0b0001000,
        WebDriverInstance  = 0b0010000,
        Profiling          = 0b0100000,
        ServerMessages     = 0b1000000
    }
}
