using HtmlAgilityPack;
using ScrapySharp.Core;
using ScrapySharp.Html.Parsing;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PP3.Models.Parsers
{
    public class EspaceNetParser : Parser
    {
        public override bool TryParse(string filepath, out Patent p)
        {
            p = null;

            return true;
        }

        public override bool TryParse(HtmlDocument doc, out Patent p)
        {
            p = null;
            string CpcPattern = @"[A-Z]([0-9]){2}[A-Z]";
            string DatePattern = @"[0-9]{4}[-][0-9]{2}[-][0-9]{2}";
            string webSitePattern = @"\b[htps]{5}[:][\/]{2}\S+\b";

            try
            {
                var name = doc.DocumentNode.SelectSingleNode("//div[@id='pagebody']/h3").InnerText.Trim();
                var autors = doc.DocumentNode.SelectSingleNode("//span[@id='inventors']").InnerText.Trim();

                var CPC = string.Join(' ', (from a in doc.DocumentNode.SelectNodes("//a")
                                            where Regex.IsMatch(a.InnerText, CpcPattern)
                                            select a.InnerText).ToArray());

                var url = Regex.Matches(doc.Text, webSitePattern).First().Value;
                var date = Regex.Matches(doc.DocumentNode.SelectSingleNode("//div[@id='pagebody']/h1").InnerText.Trim(), DatePattern)
                    .First().Value;

                p = new Patent()
                {
                    Autors = autors,
                    Country = "EU",
                    Name = name,
                    PublicationDate = DateTime.Parse(date),
                    Link = url,
                    CPC = CPC,
                };
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }
    }
}
