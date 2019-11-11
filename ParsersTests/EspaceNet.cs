using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PP3.Models;
using PP3.Models.Parsers;
using System;

namespace ParsersTests
{
    [TestClass]
    public class EspaceNetParsers
    {
        static Parser EspaceNetParser = new EspaceNetParser();
        static Parser DepatisNetParser = new DepatisnetParser();

        [TestMethod]
        public void TryParseEapatis()
        {
            Patent p = new Patent();

            var doc = new HtmlDocument();
            doc.Load("D:\\Программы (все)\\HTML-страницы\\Espacenet - Bibliographic data.html");

            string url = "https://worldwide.espacenet.com/publicationDetails/biblio?CC=KR&NR=20190084850A&KC=A&FT=D&ND=3&date=20190717&DB=&locale=en_EP#";
            string name = "HIGH BANDWIDTH MEMORY SILICON PHOTONIC THROUGH SILICON VIA ARCHITECTURE FOR LOOKUP COMPUTING ARTIFICIAL INTELLEGENCE ACCELERATOR";
            string autors = " GU PENG; MALLADI KRISHNA;";
            DateTime date = DateTime.Parse("2019-07-17");

            bool ok = EspaceNetParser.TryParse(doc, out p);
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void TryParseDepatisNet()
        {
            Patent p = new Patent();

            var doc = new HtmlDocument();
            doc.Load("D:\\Программы (все)\\HTML-страницы\\DEPATISnet _ Bibliographische Daten.html");

            bool ok = DepatisNetParser.TryParse(doc, out p);

            Assert.IsTrue(ok);
        }


    }
}
