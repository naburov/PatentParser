using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public abstract Patent TryParse(string url);
        public Parser()
        {

        }
    }
    public class PathftParser : Parser
    {
        public override Patent TryParse(string url)
        {
            string CpcPattern = @"[A-Z]([0-9]){2}[A-Z]";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            var dateCount = 0;
            DateTime date = DateTime.Now;
            string CPC = "";

            var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            var query = (from cell in htmlDoc.DocumentNode.SelectNodes("//td") select cell.InnerText).ToList<string>();

            var name = (from cell in htmlDoc.DocumentNode.SelectNodes("//font") select cell.InnerText).ToList<string>()
                .Where(x => x.Length > 4).First();

            foreach (var cell in query)
            {
                if (Regex.IsMatch(cell, CpcPattern))
                    CPC += cell;
            }
            string autor = query[8].Replace(',', ' ').Replace("&nbsp", "").Replace('\n', ' ').Replace(';', ' ').Replace("et al.", "").Trim();
            foreach (var cell in query)
            {
                bool ok = DateTime.TryParse(cell, out date);
                if (ok && dateCount > 1) break;
                if (ok && dateCount < 2) dateCount++;
            }
            CPC = CPC.Replace("&nbsp", " ");
            var p = new Patent()
            {
                Autors = autor,
                Country = "US",
                Name = name,
                PublicationDate = date,
                Link = url,
                CPC = CPC,
            };
            return p;
        }

        public PathftParser() : base()
        {

        }
    }
    public class FipsParser : Parser
    {
        public override Patent TryParse(string url)
        {
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
            var p = new Patent()
            {
                Autors = autors,
                Country = "RU",
                Name = name,
                PublicationDate = date,
                Link = url,
                CPC = CPC,
            };
            return p;
        }

        public FipsParser() : base()
        {

        }
    }
    public class EapatisParser
    {
        public Patent TryParse(string url)
        {
            HtmlWeb web = new HtmlWeb();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var enc1251 = Encoding.GetEncoding(1251);
            web.OverrideEncoding = enc1251;
            //var doc = new HtmlDocument();
            var doc = web.Load(url);

            var props = Parser.CreateDictionaryFromUrl(doc);

            var CPC = props["Индексы Международной патентной классификации"];
            var date = DateTime.Parse(props["Дата подачи заявки"]);
            var name = props["Название изобретения"];
            var autor = props["Сведения о заявителе(ях)"];
            var link = url;

            var p = new Patent()
            {
                Autors = autor,
                Country = "EAPATIS",
                Name = name,
                PublicationDate = date,
                Link = url,
                CPC = CPC,
            };
            return p;
        }
        public EapatisParser() : base()
        {

        }
    }

   
}
