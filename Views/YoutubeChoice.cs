using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumScraper.Views {
    static class YoutubeChoice {
        public static void Print() {
            Console.Clear();
            Header.Print();
            Console.WriteLine("You've chosen: YouTube");
            Console.WriteLine("Enter a search term or press ENTER to quit: ");
        }
    }
}
