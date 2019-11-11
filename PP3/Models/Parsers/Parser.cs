using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace PP3.Models.Parsers
{
    public abstract class Parser
    {
        public static Dictionary<string, string> CreateDictionaryFromUrl(HtmlDocument doc)
        {
            var res = new Dictionary<string, string>();
            var dict = doc.DocumentNode.SelectNodes("//tr");

            foreach (var pair in dict)
            {
                var count = pair.ChildNodes.Count;
                if (count == 2)
                {
                    var Key = pair.ChildNodes[0].FirstChild.InnerText;
                    Key = Regex.Replace(Key, "[(][0-9]+[)]", "").Trim();
                    var Value = pair.ChildNodes[1].FirstChild.InnerText;
                    Value = Regex.Replace(Value, "[(][0-9]+[)]", "").Trim();
                    res.Add(Key, Value);
                }
            }
            return res;
        }

        public static HtmlDocument GetPage(string url)
        {
            var doc = new HtmlDocument();

            using (var driver = new ChromeDriver())
            {
                driver.Navigate().GoToUrl(url);
                var pageSource = driver.PageSource;
                doc.LoadHtml(pageSource);
            }
            return doc;
        }

        public abstract bool TryParse(string url, out Patent patent);

        public Parser()
        {

        }

        public abstract bool TryParse(HtmlDocument doc, out Patent p);
    }

    

   
}
