using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PP3.Models.Parsers
{

    public class PathftParser : Parser
    {
        public override bool TryParse(string url, out Patent p)
        {
            p = null;

            string CpcPattern = @"[A-Z]([0-9]){2}[A-Z]";
            string NamePatern = @"([\w]+[\s]{1,})+";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            var dateCount = 0;
            DateTime date = DateTime.Now;
            string CPC = "";

            var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            var query = (from cell in htmlDoc.DocumentNode.SelectNodes("//td") select cell.InnerText.Trim()).ToList<string>();

            var name = (from cell in htmlDoc.DocumentNode.SelectNodes("//font") select cell.InnerText.Trim()).ToList<string>()[3];
                        

            try
            {
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
                p = new Patent()
                {
                    Autors = autor,
                    Country = "US",
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

        public override bool TryParse(HtmlDocument doc, out Patent p)
        {
            throw new NotImplementedException();
        }

        public PathftParser() : base()
        {

        }
    }
}
