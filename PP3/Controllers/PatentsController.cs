using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using PP3.Models;

using System.Diagnostics;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Text;
using OfficeOpenXml;
using System.IO;
using PP3.Models.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PP3.Controllers
{
    public class PatentsController : Controller
    {
        private readonly PatentDbContext _context;
        private IHostingEnvironment _appEnvironment;
        private Dictionary<string, Parser> _parsers;

        public PatentsController(PatentDbContext context, IHostingEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;

            _parsers = new Dictionary<string, Parser>();
            _parsers.Add("uspto", new PathftParser());
            _parsers.Add("fips", new FipsParser());
            _parsers.Add("eapo", new EapatisParser());
            _parsers.Add("espacenet", new EspaceNetParser());
            _parsers.Add("depatisnet", new DepatisnetParser());
        }

        private bool PatentExists(int id)
        {
            return _context.Patents.Any(e => e.ID == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddUrl(string url)
        {
            Patent patent = null;
            bool ok;
            await Task.Run(() =>
            {
                var websiteDomains = url.Split('.');
                string parserName = "";

                foreach (var key in _parsers.Keys)
                {
                    if (websiteDomains.Contains(key))
                        parserName = key;
                }

                try
                {
                    ok = _parsers[parserName].TryParse(url, out patent);
                }
                catch (Exception e)
                {
                    ok = false;
                }

                if (ok)
                {
                    _context.Patents.Add(patent);
                    _context.SaveChanges();
                }
            });

            //if (url.Contains("patft"))
            //{
            //    await Task.Run(() =>
            //    {
            //        Patent p = ParsePathft(url);
            //        _context.Patents.Add(p);
            //        _context.SaveChanges();
            //    });
            //}
            //if (url.Contains("fips"))
            //{
            //    await Task.Run(() =>
            //    {
            //        Patent p = ParseFips(url);
            //        _context.Patents.Add(p);
            //        _context.SaveChanges();
            //    });
            //}
            //if (url.Contains("eapo"))
            //{
            //    Patent p = ParseEapatis(url);
            //    _context.Patents.Add(p);
            //    await _context.SaveChangesAsync();
            //}
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
            try
            {
                var CPC = props["Индексы Международной патентной классификации"];
                var date = DateTime.Parse(props["Дата подачи евразийской заявки"]);
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
            catch
            {
                return null;
            }
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




        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult Export()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {

                var ws = pck.Workbook.Worksheets.Add("Документы");

                var export = _context.Patents;

                List<string> names = export.First().GetType().GetProperties().Select(p => p.Name).ToList();
                int asciicode = (int)'A';
                for (int i = 1; i <= names.Count(); i++)
                {
                    ws.Cells[Convert.ToChar(asciicode++).ToString() + "1"].Value = names[i - 1];
                }

                ws.Cells["A2"].LoadFromCollection(export);
                ws.Cells.AutoFitColumns();

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var enc = Encoding.GetEncoding(437);

                byte[] arr = pck.GetAsByteArray();
                return File(arr, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Patents.xlsx");

                //pck.SaveAs(Response.OutputStream);
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=Документы.xlsx");
            }
        }

        [HttpPost("UploadHtml")]
        public async Task<IActionResult> UploadHtml(IFormFile uploadedFile)
        {
            var filePaths = new List<string>();
            await Task.Run(() =>
            {
                string path = "/Files/" + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    uploadedFile.CopyTo(fileStream);
                }

                Patent patent  = new Patent();
                var doc = new HtmlDocument();
                doc.Load(_appEnvironment.WebRootPath + path);
                var title = doc.DocumentNode.SelectSingleNode("//title").InnerText.Split(' ');
                if (title.Contains("Espacenet"))
                {
                    var ok = _parsers["espacenet"].TryParse(doc, out patent);
                    if (ok)
                    {
                        _context.Patents.Add(patent);
                        _context.SaveChanges();
                    }
                }
                else if (title.Contains("DEPATISnet"))
                {
                    var ok = _parsers["depatisnet"].TryParse(doc, out patent);
                    if (ok)
                    {
                        _context.Patents.Add(patent);
                        _context.SaveChanges();
                    }
                }

            });

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return RedirectToAction("Index", "Home");
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
