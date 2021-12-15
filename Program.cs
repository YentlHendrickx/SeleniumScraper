// Debug modee logs options and disables creation of CSV file)
//#define debug
// Debug options, if enabled will disable the headless mode (for inspecting the web page)
//#define debugOptions

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumScraper.Views;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Threading;

namespace SeleniumScraper {
    class Program {
        static void Main(string[] args) {
            //// Var setup
            bool continueProgram = true;
            bool noValidResults = false;

            // Dictionaries for user input and urls
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("y", "YouTube");
            options.Add("i", "Indeed");
            options.Add("r", "Reddit");
            options.Add("", "Enter");

            Dictionary<string, string> urls = new Dictionary<string, string>();
            urls.Add("y", "https://www.youtube.com/results?search_query=");
            urls.Add("i", "https://be.indeed.com/jobs?q=");
            urls.Add("r", "https://www.reddit.com/r/");

            // Create driver and add headless option
            ChromeOptions chOptions = new ChromeOptions();

#if debugOptions
            chOptions.AddArgument("--disable-notifications");
#else
            chOptions.AddArgument("--headless");
            chOptions.AddArgument("--disable-notifications");
            chOptions.AddArgument("--silent");
            chOptions.AddArgument("--log-level=3");
#endif
            var chromeDriverService = ChromeDriverService.CreateDefaultService("./");
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;
            IWebDriver driver = new ChromeDriver(chromeDriverService, chOptions);


            // Program will continously run unless user exits himself
            while (continueProgram) {
                noValidResults = false;
                
                IntroPage.Print();
                string userIn = Console.ReadLine().Trim().ToLower();

                if (options.ContainsKey(userIn)) {

                    if (userIn == "") {
                        continueProgram = false;
                    } else {
                        Master.PrintGeneral(options[userIn]);
                        string searchTermIn = Console.ReadLine().Trim();

                        // String builder for svc file
                        System.Text.StringBuilder svcBuilder = new StringBuilder();

                        if (searchTermIn != "") {
                            Console.Write("Starting the scraping process!\n");

                            string lookupUrl = urls[userIn] + searchTermIn;

                            // For indeed we want only from the past 3 days
                            if (options[userIn] == "Indeed") {
                                lookupUrl += "&fromage=3";
                            } else if (options[userIn] == "Reddit") {
                                // For reddit we want the top posts of the past month
                                lookupUrl += "/top/?t=month";
                            } else if (options[userIn] == "YouTube") {
                                lookupUrl += "&sp=CAI%253D";
                            }
                            Console.WriteLine(lookupUrl);
                            // Go to url
                            driver.Navigate().GoToUrl(lookupUrl);
                            Console.Clear();

                            // Format
                            string outputString = "\n----------\n";

                            #region Scraping
                            #region YouTube

                            if (options[userIn] == "YouTube") {
                                // For checking the amount of valid results
                                int youtubeResults = 0;
                                int i = 1;
                                // Loop over first 5 vids
                                while (youtubeResults < 5 && i < 10) {
                                    try {
                                        // Get title
                                        string title = "(//*[@id='video-title']/yt-formatted-string)[" + i + "]";
                                        var videotitle = driver.FindElement(By.XPath(title)).Text;

                                        // Get views
                                        string views = "(//*[@id=\"metadata-line\"]/span[1])[" + i + "]";
                                        var vidViews = driver.FindElement(By.XPath(views)).Text;

                                        // Get channel
                                        string channel = "/html/body/ytd-app/div/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div/ytd-section-list-renderer/" +
                                            "div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer[" + i + "]/div[1]/div/div[2]/ytd-channel-name/div/div/yt-formatted-string/a";
                                        var vidChannel = driver.FindElement(By.XPath(channel)).Text;

                                        // Get url
                                        string url = "/html/body/ytd-app/div/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div/ytd-section-list-renderer/" +
                                            "div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer[" + i + "]/div[1]/ytd-thumbnail/a";
                                        var vidUrl = driver.FindElement(By.XPath(url));
                                        string hrefUrl = vidUrl.GetAttribute("href");

                                        // Output for console window
                                        outputString += videotitle + '\n' + vidViews + '\n' + vidChannel + '\n' + hrefUrl;
                                        outputString += "\n----------\n";

                                        // Append data for CSV saving
                                        svcBuilder.Append("\"" + videotitle + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + vidViews + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + vidChannel + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + hrefUrl + "\"");
                                        svcBuilder.Append("\n");

                                        youtubeResults++;
                                    } catch {
#if !debug
                                        Console.WriteLine("Error while fething from YouTube!");
#endif
                                    }

                                    if (youtubeResults == 0) {
                                        outputString = "No results found for " + options[userIn] + " scrape with option: " + searchTermIn;
                                        noValidResults = true;
                                    }
                                    i++;
                                }

                                #endregion
                                #region Indeed
                            } else if (options[userIn] == "Indeed") {
                                int indeedResults = 0;
                                int i = 1;
                                while (indeedResults < 16 && i < 20) {
                                    // Scrape from indeed site

                                    try {
                                        // Get title
                                        string titleFind = "//*[@id=\"mosaic-provider-jobcards\"]/a[" + i + "]/div[1]/div/div[1]/div/table[1]/tbody/tr/td/div[1]/h2/span";
                                        var title = driver.FindElement(By.XPath(titleFind)).Text;

                                        string companyFind = "//*[@id=\"mosaic-provider-jobcards\"]/a[" + i + "]/div[1]/div/div[1]/div/table[1]/tbody/tr/td/div[2]/pre/span";
                                        var companyName = driver.FindElement(By.XPath(companyFind)).Text;

                                        string locationFind = "//*[@id=\"mosaic-provider-jobcards\"]/a[" + i + "]/div[1]/div/div[1]/div/table[1]/tbody/tr/td/div[2]/pre/div";
                                        var location = driver.FindElement(By.XPath(locationFind)).GetAttribute("innerHTML");

                                        if (location.IndexOf('<') != -1) {
                                            location = location.Substring(0, location.IndexOf('<'));
                                        }

                                        string urlFind = "//*[@id=\"mosaic-provider-jobcards\"]/a[" + i + "]";
                                        var url = driver.FindElement(By.XPath(urlFind)).GetAttribute("href");

                                        // Output for console window
                                        outputString += title + '\n' + companyName + '\n' + location + '\n' + url;
                                        outputString += "\n----------\n";

                                        // Append data for CSV saving
                                        svcBuilder.Append("\"" + title + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + companyName + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + location + "\"");
                                        svcBuilder.Append(";");
                                        svcBuilder.Append("\"" + url + "\"");
                                        svcBuilder.Append("\n");

                                        indeedResults++;
                                    } catch {
#if debug
                                        Console.WriteLine("Error while fetching from Indeed!");
#endif
                                    }

                                    if (indeedResults == 0) {
                                        outputString = "No results found for " + options[userIn] + " scrape with option: " + searchTermIn;
                                        noValidResults = true;
                                    }
                                    i++;
                                }

                                #endregion
                                #region Reddit
                            } else if (options[userIn] == "Reddit") {
                                int resultCount = 0;
                                int i = 1;

                                // Reddit scraping is done for top 5 most popular post of that month on the subreddit
                                // Limit attempts to 10
                                while (resultCount < 5 && i < 10) {
                                    try {
                                        // ID of posts are saved in the same position everytime
                                        string postId = "//*[@id=\"SHORTCUT_FOCUSABLE_DIV\"]/div[2]/div/div/div/div[2]/div[4]/div[1]/div[4]/div[" + i + "]/div/div";
                                        string id = driver.FindElement(By.XPath(postId)).GetAttribute("id");

                                        // Filter out empty tags
                                        if (id != "") {
                                            // Filter out advertisements (regular posts never exceed an ID greater than 10
                                            if (id.Length > 10) {
                                                Console.WriteLine("Advertisement");
                                                throw new InvalidDataException("Advertisement");
                                            }

                                            // Log ID for debugging
#if debug
                                            Console.WriteLine(id);
#endif

                                            // SOME POSTS HAVE A DIFFERENT STRUCTURE
                                            // AS IF TO MAKE MY JOB HARDER!
                                            // So far I've identified 3 different post structures
                                            // These different structures save the title of the post under a different tag
                                            string titleFind = "";
                                            string titleResult = "";
                                            string dateFind = "";
                                            string dateResult = "";
                                            string upvoteFind = "";
                                            string upvoteResult = "";
                                            string urlFind = "";
                                            string urlResult = "";

                                            // Get date, this is consistent
                                            try {
                                                dateFind = "//*[@id=\"" + id + "\"]/div[3]/div[1]/div/div[1]/a";
                                                dateResult = driver.FindElement(By.XPath(dateFind)).Text;
                                            } catch {
                                                //Console.WriteLine("Date Fetch Failed");
                                            }

                                            // Upvotes are also consistent and use the post ID in their own ID
                                            // Get upvotes
                                            try {
                                                upvoteFind = "//*[@id=\"vote-arrows-" + id + "\"]/div";
                                                upvoteResult = driver.FindElement(By.XPath(upvoteFind)).Text;
                                            } catch {
                                                // Invalid
                                                //Console.WriteLine("Upvote Fetch Failed");
                                            }

                                            // URL are also consistent and also use the post ID
                                            try {
                                                urlFind = "//*[@id=\"" + id + "\"]/div[3]/div[1]/div/div[1]/a";
                                                urlResult = driver.FindElement(By.XPath(urlFind)).GetAttribute("href");
                                            } catch {
                                                // Invalid
                                                //Console.WriteLine("URL FECTH FAILED");
                                            }

                                            try {
                                                titleFind = "//*[@id=\"" + id + "\"]/div[3]/div[2]/div[2]/a/div/h3";
                                                titleResult = driver.FindElement(By.XPath(titleFind)).Text;

                                                //Console.WriteLine("First Post structure");
                                            } catch {
                                                try {
                                                    titleFind = "//*[@id=\"" + id + "\"]/div[2]/article/div[1]/div[2]/div[2]/a/div/h3";
                                                    titleResult = driver.FindElement(By.XPath(titleFind)).Text;

                                                    //Console.WriteLine("Second Post structure");
                                                } catch {
                                                    try {
                                                        titleFind = "//*[@id=\"" + id + "\"]/div[3]/div[2]/div[1]/a/div/h3";
                                                        titleResult = driver.FindElement(By.XPath(titleFind)).Text;

                                                        //Console.WriteLine("Third Post structure");
                                                    } catch {
                                                        // Invalid Title
                                                    }
                                                }
                                            }

                                            // If we managed to get a title the rest of the values should also be valid
                                            if (titleResult != "") {
                                                // Output for console window
                                                outputString += titleResult + '\n' + upvoteResult + '\n' + dateResult + '\n' + urlResult;
                                                outputString += "\n----------\n";

                                                // Append data for CSV saving
                                                svcBuilder.Append("\"" + titleResult + "\"");
                                                svcBuilder.Append(";");
                                                svcBuilder.Append("\"" + upvoteResult + "\"");
                                                svcBuilder.Append(";");
                                                svcBuilder.Append("\"" + dateResult + "\"");
                                                svcBuilder.Append(";");
                                                svcBuilder.Append("\"" + urlResult + "\"");
                                                svcBuilder.Append("\n");

                                                // Got a new valid result
                                                resultCount++;
                                            }
                                        }
                                    } catch {
#if debug
                                        Console.WriteLine("Encountered Error! Continuing with next post...");
                                        Console.WriteLine("");
#endif
                                    }

                                    i++;
                                }

                                // No results have been found, must be invalid link
                                if (resultCount == 0) {
                                    outputString = "No results found for " + options[userIn] + " scrape with option: " + searchTermIn;
                                    noValidResults = true;
                                }

                            }
                            #endregion
                            #endregion

                            Console.WriteLine(outputString);

                            Console.WriteLine("");
                            Console.WriteLine("Press any key to continue!");
                            Console.ReadKey();
                            Console.WriteLine("");

                            #region SaveAsSVC

                            bool validAnswer = true;
#if !debug
                            // Only give users the option to save their data if the data actually has any value
                            if (!noValidResults) {
                                // Give user option to save their data
                                Console.Write("Do you want to save this data? y/n: ");
                                validAnswer = false;
                            } else {
                                // By setting valid answer to true, data saving will be skipped
                                validAnswer = true;
                                Console.Clear();
                            }
#endif

                            while (!validAnswer) {
                                string answer = Console.ReadLine().Trim().ToLower();

                                if (answer == "n" || answer == "y") {
                                    if (answer == "y") {
                                        Console.WriteLine("");
                                        Console.WriteLine("SAVING...");
                                        Console.WriteLine("");

                                        try {
                                            // Create and write to SVC file
                                            string date = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss");
                                            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                                            path += "\\output\\";
                                            string fileName = date + " " + options[userIn] + " scrape " + searchTermIn + ".csv";
                                            string filePath = path + fileName;

                                            File.WriteAllText(filePath, svcBuilder.ToString());
                                        } catch {
                                            Console.WriteLine("");
                                            Console.WriteLine("Error while saving... please try again later.");
                                            Console.WriteLine("");
                                        }
                                    }

                                    validAnswer = true;

                                    Console.WriteLine();
                                    Console.WriteLine("Press any key to continue!");
                                    Console.ReadKey();
                                    Console.Clear();

                                } else {
                                    // Invalid input
                                    Console.WriteLine("");
                                    Console.WriteLine("Only 'n' or 'y' are allowed! Please try again.");
                                    Console.Write("Do you want to save this data? y/n: ");
                                }
                            }
                            #endregion
                        } else {
                            // Invalid input
                            Console.WriteLine("");
                            Console.WriteLine("Please choose a valid option! ");
                            Console.WriteLine("");
                        }

                    }
                }
            }
            // Close driver and exit
            driver.Close();
            Environment.Exit(0);
        }
    }
}