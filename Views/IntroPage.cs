using System;
using System.Collections.Generic;

namespace SeleniumScraper.Views {
    static class IntroPage {
        public static void Print() {
            Header.Print();
            Console.WriteLine("Type Y to scrape YouTube.");
            Console.WriteLine("Type I to scrape indeed");
            Console.WriteLine("Type R to scrape Reddit.");
            Console.WriteLine("Press ENTER to quit.");
            Console.WriteLine(" ");
            Console.Write("Enter choice: ");
        }
    }
}
