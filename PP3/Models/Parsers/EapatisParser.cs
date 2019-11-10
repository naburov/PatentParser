using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP3.Models.Parsers
{
    public class EapatisParser : Parser
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

            var props = Parser.CreateDictionaryFromUrl(doc);

            try
            {
                DateTime date;
                var CPC = props["Индексы Международной патентной классификации"];
                try
                {
                    date = DateTime.Parse(props["Дата подачи заявки"]);
                }
                catch (Exception e)
                {
                    date = DateTime.Parse(props["Дата подачи евразийской заявки"]);
                }
                var name = props["Название изобретения"];
                var autor = props["Сведения о заявителе(ях)"];
                var link = url;

                p = new Patent()
                {
                    Autors = autor,
                    Country = "EAPATIS",
                    Name = name,
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

        public EapatisParser() : base()
        {

        }
    }
}
