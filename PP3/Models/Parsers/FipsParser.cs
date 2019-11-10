using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PP3.Models.Parsers
{

    public class FipsParser : Parser
    {
        public override bool TryParse(string url, out Patent p)
        {
            p = null;

            HtmlWeb web = new HtmlWeb();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var enc1251 = Encoding.GetEncoding(1251);
            web.OverrideEncoding = enc1251;
            //var doc = new HtmlDocument();
            var doc = web.Load(url);


            //string path = @"D:\Программы (все)\ИЗ №2691214.html";
            string CpcPattern = @"[A-Z]([0-9]){2}[A-Z]";
            string DatePattern = @"([0-9]){2}[.]([0-9]){2}[.]([0-9]){4}";
            //doc.Load(path);

            try
            {
                var node = doc.DocumentNode.SelectSingleNode("//head/title");
                var query = (from cell in doc.DocumentNode.SelectNodes("//span") select cell.InnerText).ToList<string>();

                string CPC = "";

                foreach (var cell in query)
                {
                    if (Regex.IsMatch(cell, CpcPattern))
                        CPC += cell;
                }

                var name = doc.DocumentNode.SelectSingleNode("//p[@id='B542']/b").InnerText;
                query = (from cell in doc.DocumentNode.SelectNodes("//a") select cell.InnerText).ToList<string>();

                DateTime date = DateTime.Now;
                foreach (var cell in query)
                {
                    if (Regex.IsMatch(cell, DatePattern))
                        DateTime.TryParse(cell, out date);
                }

                var autors = (from cell in doc.DocumentNode.SelectNodes("//td[@id='bibl']/p/b")
                              select cell.InnerText).FirstOrDefault<string>().Replace(",", ", ").Replace("\n", "");

                var link = url;
                p = new Patent()
                {
                    Autors = autors,
                    Country = "RU",
                    Name = name,
                    PublicationDate = date,
                    Link = url,
                    CPC = CPC,
                };
                return true;

            }catch(Exception e)
            {
                return false;
            }
        }

        public FipsParser() : base()
        {

        }
    }
}
