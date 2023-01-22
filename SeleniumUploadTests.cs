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

    [Test]
    public void CanUploadAFileOnChromeFromBuildDirectory()
    {
        _driver = new ChromeDriver();
        CanUploadAFile();
    }

    [Test]
    public void CanUploadAFileOnEdgeFromBuildDirectory()
    {
        _driver = new EdgeDriver();
        CanUploadAFile();
    }

    [Test]
    public void CanUploadAFileOnFirefoxFromBuildDirectory()
    {
        _driver = new FirefoxDriver();
        CanUploadAFile();
    }

    [Test]
    public void CanUploadAFileOnSafariFromBuildDirectory()
    {
        Assume.That(OperatingSystem.IsMacOS(), Is.True,
            () => "Safari is only available locally on MacOS");
        _driver = new SafariDriver();
        CanUploadAFile();
    }


    private void CanUploadAFile()
    {
        // Navigate to Url
        _driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/upload");

        string _buildDirectoryFilePath =
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources",
                "tick.png");

        // The important bit: FileInputElement.SendKeys({full.path.to.file})
        _driver.FindElement(By.Id("file-upload"))
            .SendKeys(_buildDirectoryFilePath);
        _driver.FindElement(By.Id("file-submit"))
            .Submit();

        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));


        // SafariDriver leaves surrounding whitespace so .Trim() for compatibility
        string loadedFiles =
            wait.Until((d) => d.FindElement(By.Id("uploaded-files"))).Text.Trim();

        // Not ideal locator but is unique and I can't edit the source to add a better one
        string successMessage =
            _driver.FindElement(By.CssSelector("#content h3")).Text;

        // Using NUnit Asserts
        Assert.Multiple(() =>
        {
            Assert.That(successMessage, Is.EqualTo("File Uploaded!"));
            Assert.That(loadedFiles, Is.EqualTo("tick.png"));
        });

        // Or using  my preferred FluentAssertions
        using (new AssertionScope())
        { 
            successMessage.Should().Be("File Uploaded!");
            loadedFiles.Should().Be("tick.png");
        }

    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Quit();
    }

}