using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;

namespace FileUploadTests;

public class SeleniumUploadTests
{
    private IWebDriver? _driver;
    private string _buildDirectoryFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "tick.png");
    private string _documentsFilePath;


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var documentsPath = OperatingSystem.IsMacOS() ?
            // to avoid my confusion as MacOs actually returns /Users/username not /Users/username/Documents
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents") :
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        _documentsFilePath = Path.Combine(documentsPath,"tick.png");

        // using the copy below creates a file that is not world-readable and will not be uploaded by SafariDriver
        // Manually allow Everyone read access to use Safari and don't overwrite it again
        // Other WebDriver implementations do not require the file to be "world-readable"
        if (!File.Exists(_documentsFilePath))
        File.Copy(_buildDirectoryFilePath, _documentsFilePath);
    }

    [Test]
    public void CanUploadAFileOnChromeFromDocuments()
    {
        _driver = new ChromeDriver();
        CanUploadAFile(false);
    }

    [Test]
    public void CanUploadAFileOnEdgeFromDocuments()
    {
        _driver = new EdgeDriver();
        CanUploadAFile(false);
    }

    [Test]
    public void CanUploadAFileOnFirefoxFromDocuments()
    {
        _driver = new FirefoxDriver();
        CanUploadAFile(false);
    }

    [Test]
    public void CanUploadAFileOnSafariFromDocuments()
    {
        Assume.That(OperatingSystem.IsMacOS(), Is.True, () => "Safari is only available locally on MacOS");
        _driver = new SafariDriver();
        CanUploadAFile(false);
    }

    [Test]
    public void CanUploadAFileOnChromeFromBuildDirectory()
    {
        _driver = new ChromeDriver();
        CanUploadAFile(true);
    }

    [Test]
    public void CanUploadAFileOnEdgeFromBuildDirectory()
    {
        _driver = new EdgeDriver();
        CanUploadAFile(true);
    }

    [Test]
    public void CanUploadAFileOnFirefoxFromBuildDirectory()
    {
        _driver = new FirefoxDriver();
        CanUploadAFile(true);
    }

    [Test]
    public void CanUploadAFileOnSafariFromBuildDirectory()
    {
        Assume.That(OperatingSystem.IsMacOS(), Is.True, () => "Safari is only available locally on MacOS");
        _driver = new SafariDriver();
        CanUploadAFile(true);
    }


    private void CanUploadAFile(bool uploadFromBuildDirectory)
    {
        // Navigate to Url
        _driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/upload");

        string uploadFilePath = uploadFromBuildDirectory ?
            _buildDirectoryFilePath :
            _documentsFilePath;

        _driver.FindElement(By.Id("file-upload")).SendKeys(uploadFilePath);
        _driver.FindElement(By.Id("file-submit")).Submit();

        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        string loadedFiles = wait.Until((d) => d.FindElement(By.Id("uploaded-files"))).Text;

        var successMessage = wait.Until((d) => d.FindElement(By.CssSelector("#content h3"))).Text;

        // NUnit
        Assert.Multiple(() =>
        {
            Assert.That(successMessage, Is.EqualTo("File Uploaded!"));
            // SafariDriver returns whitespace around the filename so we must use "Does.Contain" constraint
            Assert.That(loadedFiles, Does.Contain("tick.png"));
        });

        // FluentAssertions
        using (new AssertionScope())
        { 
            successMessage.Should().Be("File Uploaded!");
            loadedFiles.Should().Contain("tick.png");

        }

    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Quit();
    }

}