using OpenQA.Selenium.Firefox;

namespace source.Retrieve_Requests
{
    public class SeleniumedTorBrowser
    {

        /// <summary>
        /// Manages options for Tor Browser driver.
        /// </summary>
        private readonly FirefoxOptions firefoxOptions;

        /// <summary>
        /// Exposes Tor Browser driver as a service.
        /// </summary>
        private readonly FirefoxDriverService firefoxDriverService;

        /// <summary>
        /// Driver for Tor Browser.
        /// </summary>
        /// <value><c>FirefoxDriver</c></value>
        public FirefoxDriver FirefoxDriver { get; set; }
        /// <summary>
        /// Path for tor browser profile.
        /// </summary>
        private readonly string profilePath;

        /// <summary>
        /// Path for tor browser binary.
        /// </summary>
        private readonly string binaryPath;

        /// <summary>
        /// Path for gecko driver. Link between Selenium tests and Firefox browser.
        /// </summary>
        private readonly string geckoDriverPath;

        /// <summary>
        /// Constructs Selenium with Tor Browser.
        /// </summary>
        public SeleniumedTorBrowser()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(currentDirectory);
            profilePath = currentDirectory + "/resources/tor-browser/Browser/TorBrowser/Data/Browser/profile.default/";
            binaryPath = currentDirectory + "/resources/tor-browser/Browser/firefox";
            geckoDriverPath = currentDirectory + "/resources/tor-browser/geckodriver";


            firefoxDriverService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
            firefoxDriverService.FirefoxBinaryPath = binaryPath;
            firefoxDriverService.BrowserCommunicationPort = 2828;

            firefoxOptions = new()
            {
                LogLevel = FirefoxDriverLogLevel.Fatal
            };
            firefoxOptions.AddArguments("-profile", profilePath);
            firefoxOptions.SetPreference("marionette.debugging.clicktostart", false);
            firefoxOptions.SetPreference("torbrowser.settings.quickstart.enabled", true);

            FirefoxDriver = new FirefoxDriver(firefoxDriverService, firefoxOptions, TimeSpan.FromSeconds(180));
        }
    }
}
