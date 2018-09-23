# SReport
SReport is a log manager for Selenium based tests and projects

### How to use

You should to use `SReportBuilder` to generate a log Report file, based on what type of logs you want and the desired log folder. You can do that this way:

```
SReport = _logReport = SReportBuilder.Setup(yourIWebDriver, baseLocation)
                .WithAllLogsEnabled()
                .Build();
```

After that, you just can call the method `Log` whenever you want to genetare a log based on test or a context.

```
_logReport.Log("your text name / context name");
```


### Customizations

* If you are not looking fo all logs, just a screenshoot or browser console info, you can filter them after the `Setup` method, calling from the SReportBuilder. As Example:

```
SReport = _logReport = SReportBuilder.Setup(Driver, baseLocation)
                .WithConsoleLogs()
                .WithGeneralInfo()
                .WithPageStateHtml()
                .WithScreenshoot()
                .Build();
```

