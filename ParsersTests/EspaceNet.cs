using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PP3.Models;
using PP3.Models.Parsers;
using System;
using System.Text.RegularExpressions;

namespace ParsersTests
{
    [TestClass]
    public class ParserTesting
    {
        static Parser EspaceNetParser = new EspaceNetParser();
        static Parser DepatisNetParser = new DepatisnetParser();
        static Parser PathftParser = new PathftParser();

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
        public void TryParseGc()
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

        [TestMethod]
        public void TryPathft_GetNormalTitle()
        {
            Patent p = new Patent();

            var url = "http://patft.uspto.gov/netacgi/nph-Parser?Sect1=PTO2&Sect2=HITOFF&p=1&u=%2Fnetahtml%2FPTO%2Fsearch-bool.html&r=17&f=G&l=50&co1=AND&d=PTXT&s1=AI&OS=AI&RS=AI";
            bool ok = PathftParser.TryParse(url, out p);
            ok = !int.TryParse(p.Name, out int number) && ok;

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void TryPathft_GetNormalTitle2()
        {
            Patent p = new Patent();

            var url = "http://patft.uspto.gov/netacgi/nph-Parser?Sect1=PTO2&Sect2=HITOFF&p=1&u=%2Fnetahtml%2FPTO%2Fsearch-bool.html&r=31&f=G&l=50&co1=AND&d=PTXT&s1=AI&OS=AI&RS=AI";
            bool ok = PathftParser.TryParse(url, out p);
            ok = !int.TryParse(p.Name, out int number) && ok;

            Assert.IsTrue(ok);
        }


    }
}
