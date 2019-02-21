using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DuckLab.Models;
using EntityState = System.Data.Entity.EntityState;
using System.Net.Http;
using System.Net.Http.Headers;


namespace DuckLab.Controllers
{
    public class CompaniesController : Controller
    {
        private ducklabdbEntities db = new ducklabdbEntities();
        public static string baseURL = "https://api.iextrading.com/1.0/";

        public ActionResult Index()
        {
            return View();
            
        }

        public PartialViewResult _Companies(string name = "", string symbol = "", int lastPage = 0,  string page = "")
        {
            int numPage;
            if (page == "" || page == "Search")
                numPage = 1;
            else if (page == "<")
                numPage = lastPage - 1;
            else if (page == ">")
                numPage = lastPage + 1;
            else
                numPage = Convert.ToInt32(page);

            ViewBag.page = numPage;
            ViewBag.name = name;
            ViewBag.symbol = symbol;
            ViewBag.pageCount = db.Companies.Where(x => x.companyName.StartsWith(name) && x.companySymbol.StartsWith(symbol)).Count();
            if(ViewBag.pageCount % 25 == 0)
                ViewBag.pageCount = ViewBag.pageCount / 25;
            else
                ViewBag.pageCount = (ViewBag.pageCount / 25) + 1;

            numPage--;
            return PartialView(db.Companies.Where(x => x.companyName.StartsWith(name) && x.companySymbol.StartsWith(symbol)).OrderBy(x => x.companyName).Skip(numPage * 25).Take(25).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        public RedirectToRouteResult GetCompanies()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL + "ref-data/symbols");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                string strResponse = response.Content.ReadAsStringAsync().Result;
                while (strResponse.IndexOf("symbol") != -1)
                {
                    string symbol = "";
                    int index = strResponse.IndexOf("symbol") + 9;
                    for (; strResponse[index] != '"'; index++)
                    {
                        symbol = symbol + strResponse[index];
                    }

                    string companyName = "";
                    index = strResponse.IndexOf("name") + 7;
                    for (; strResponse[index] != '"'; index++)
                    {
                        companyName = companyName + strResponse[index];
                    }

                    if(!db.Companies.Where(x => x.companySymbol == symbol).Any())
                    {
                        Company newCompany = new Company();
                        newCompany.companyName = companyName;
                        newCompany.companySymbol = symbol;
                        db.Companies.Add(newCompany);
                        db.Entry(newCompany).State = EntityState.Added;
                    }

                    strResponse = strResponse.Substring(index);
                }
                db.SaveChanges();
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            client.Dispose();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
