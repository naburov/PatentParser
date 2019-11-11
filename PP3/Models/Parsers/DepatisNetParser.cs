using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PP3.Models.Parsers
{
    public class DepatisnetParser : Parser
    {
        public override bool TryParse(HtmlDocument doc, out Patent p)
        {
            p = null;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var enc1252 = Encoding.GetEncoding(1252);

            var CpcPattern = @"[A-Z]([0-9]){2}[A-Z]";
            var webSitePattern = @"\b[htps]{5}[:][\/]{2}\S+\b";
            var DatePattern = @"[0-9]{2}[.][0-9]{2}[.][0-9]{4}";

            try
            {
                var table = doc.DocumentNode.SelectNodes("//table[@class='tab_detail']/tbody/tr").ToList();


                //var name = (from tr in table
                //            where string.Join(' ', (from td in tr.ChildNodes select Regex.Replace(td.InnerText, "[^A-Za-z0-9]", "")).ToArray())
                //            .Contains("Titel")
                //            select tr.ChildNodes[7].InnerText).ToString();

                var name = (from list in (from tr in table select GetClearList(tr)).ToList()
                            where list.Contains("Titel")
                            select list).ToList()[0][3];

                var autors = (from list in (from tr in table select GetClearList(tr)).ToList()
                              where list.Contains("Erfinder")
                              select list).ToList()[0][3];

                var date = ((from dates in Regex.Matches(doc.DocumentNode
                           .SelectSingleNode("//table[@class='tab_detail']").InnerText, DatePattern)
                            select DateTime.Parse(dates.Value)).ToList()).Max();


                

                var CPC = string.Join(' ', from match in Regex.Matches(doc.Text, CpcPattern) select match.Value);

                var url = Regex.Matches(doc.Text, webSitePattern).First().Value;

                p = new Patent()
                {
                    Autors = autors,
                    Country = "DEU",
                    Name = name.ToString(),
                    PublicationDate = date,
                    Link = url,
                    CPC = CPC,
                };
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static List<string> GetClearList(HtmlNode tr)
        {
            var tds = (from t in tr.ChildNodes select t.InnerText).ToList();
            for (int i = 0; i < tds.Count; i++)
            {
                tds[i] = tds[i].Replace("&nbsp;", string.Empty);
                tds[i] = tds[i].Trim();
            }

            for (int i = 0; i < tds.Count; i++)
            {
                if (tds[i] == "")
                    tds.RemoveAt(i);
            }
            return tds;
        }

        public DepatisnetParser() : base()
        {

        }

        public override bool TryParse(string url, out Patent p)
        {
            throw new NotImplementedException();
        }
    }
}
