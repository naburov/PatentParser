using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PP3.Models;

using System.Diagnostics;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Text;

namespace PP3.Controllers
{
    public class PatentsController : Controller
    {
        private readonly PatentDbContext _context;

        public PatentsController(PatentDbContext context)
        {
            _context = context;
        }

        private bool PatentExists(int id)
        {
            return _context.Patents.Any(e => e.ID == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddUrl(string url)
        {
            if (url.Contains("patft"))
            {
                Patent p = ParsePathft(url);
                _context.Patents.Add(p);
                await _context.SaveChangesAsync();
            }
            if (url.Contains("fips"))
            {
                Patent p = ParseFips(url);
                _context.Patents.Add(p);
                await _context.SaveChangesAsync();
            }
            if (url.Contains("eapo"))
            {
                Patent p = ParseEapatis(url);
                _context.Patents.Add(p);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");

        }

        private static Patent ParsePathft(string url)
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
        private static Patent ParseFips(string url)
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
        private static Patent ParseEapatis(string url)
        {
            HtmlWeb web = new HtmlWeb();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var enc1251 = Encoding.GetEncoding(1251);
            web.OverrideEncoding = enc1251;
            //var doc = new HtmlDocument();
            var doc = web.Load(url);

            var props = CreateDictionaryFromUrl(doc);

            var CPC = props["Индексы Международной патентной классификации"];
            var date = DateTime.Parse(props["Дата подачи заявки"]);
            var name = props["Название изобретения"];
            var autor = props["Сведения о заявителе(ях)"];
            var link = url;

            var p = new Patent()
            {
                Autors = autor,
                Country = "RU",
                Name = name,
                PublicationDate = date,
                Link = url,
                CPC = CPC,
            };
            return p;

        }
        private static Dictionary<string, string> CreateDictionaryFromUrl(HtmlDocument doc)
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
        

        #region Useless
        // GET: Patents
        public async Task<IActionResult> Index()
        {
            return View(await _context.Patents.ToListAsync());
        }

        // GET: Patents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patent = await _context.Patents
                .FirstOrDefaultAsync(m => m.ID == id);
            if (patent == null)
            {
                return NotFound();
            }

            return View(patent);
        }

        // GET: Patents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PublicationDate,CPC,Link,Country,Autors")] Patent patent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patent);
        }

        // GET: Patents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patent = await _context.Patents.FindAsync(id);
            if (patent == null)
            {
                return NotFound();
            }
            return View(patent);
        }

        // POST: Patents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PublicationDate,CPC,Link,Country,Autors")] Patent patent)
        {
            if (id != patent.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatentExists(patent.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patent);
        }

        // GET: Patents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patent = await _context.Patents
                .FirstOrDefaultAsync(m => m.ID == id);
            if (patent == null)
            {
                return NotFound();
            }

            return View(patent);
        }

        // POST: Patents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patent = await _context.Patents.FindAsync(id);
            _context.Patents.Remove(patent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion

    }
}
