# SReport
SReport is a log manager for [Selenium](https://github.com/SeleniumHQ/selenium) based tests and projects

### How to use

You should to use `SReportBuilder` to generate a log Report file, based on what type of logs you want and the desired log folder. You can do that this way:

```csharp
SReport = _logReport = SReportBuilder.Setup(yourIWebDriver, baseLocation)
                .WithAllLogsEnabled()
                .Build();
```

After that, you just can call the method `Log` whenever you want to genetare a log based on test or a context.

```csharp
_logReport.Log("your text name / context name");
```


### Customizations

If you are not looking fo all logs, just a screenshoot or browser console info, you can filter them after the `Setup` method, calling from the SReportBuilder. As Example:

```csharp
SReport = _logReport = SReportBuilder.Setup(Driver, baseLocation)
                .WithConsoleLogs()
                .WithGeneralInfo()
                .WithPageStateHtml()
                .WithScreenshoot()
                .Build();
```

### Logs explanation

* **Screenshoot**: 
Take a Screenshoot based in the Browser View, and saves it as .png file.

* **Browser Console**: 
Saves a text file containing browser console content.

* **General Info**:
Saves a text file containing the test name, generation date, OS and framework version.

* **Page State Html**:
Create a .htm file with the real state of the page. It can be also manipulated in a browser DevTools to be easy to correct errors based on element queries.

* **WebDriver Instance**:
Log Based on Selenium Web Driver Messages log type.

* **Selenium Client**:
Log Based on Selenium Server log type. 

* **Selenium Client**:
Log Based on Selenium Client log type. 

* **Profiling**:
Log Based on Selenium Profile log type. 


