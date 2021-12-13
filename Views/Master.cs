using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumScraper.Views {
    static class Master {
        public static void PrintGeneral(string option) {
            Console.Clear();
            Header.Print();
            Console.WriteLine("You've chosen: " + option);
            Console.Write("Enter a search term or press ENTER to quit: ");
        }
    }
}
