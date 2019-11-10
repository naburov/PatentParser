using HtmlAgilityPack;
using ScrapySharp.Core;
using ScrapySharp.Html.Parsing;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    }
}
