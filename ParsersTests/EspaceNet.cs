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

        [TestMethod]
        public void TryParseEapatis()
        {
            Patent p = new Patent();

            string url = "https://worldwide.espacenet.com/publicationDetails/biblio?CC=KR&NR=20190084850A&KC=A&FT=D&ND=3&date=20190717&DB=&locale=en_EP#";
            string name = "HIGH BANDWIDTH MEMORY SILICON PHOTONIC THROUGH SILICON VIA ARCHITECTURE FOR LOOKUP COMPUTING ARTIFICIAL INTELLEGENCE ACCELERATOR";
            string autors = " GU PENG; MALLADI KRISHNA;";
            DateTime date = DateTime.Parse("2019-07-17");

            bool ok = EspaceNetParser.TryParse(url, out p);

            ok = ok && p.Name == name && p.Autors == autors;
            Assert.IsTrue(ok);
        }
    }
}
