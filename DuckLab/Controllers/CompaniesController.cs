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
            ViewBag.name = name;
            ViewBag.symbol = symbol;
            ViewBag.pageCount = db.Companies.Where(x => x.companyName.StartsWith(name) && x.companySymbol.StartsWith(symbol)).Count();
            if(ViewBag.pageCount % 25 == 0)
                ViewBag.pageCount = ViewBag.pageCount / 25;
            else
                ViewBag.pageCount = (ViewBag.pageCount / 25) + 1;

            int numPage;
            if (page == "" || page == "Search")
                numPage = 1;
            else if (page == "<")
                numPage = lastPage - 1;
            else if (page == "<<")
                numPage = 1;
            else if (page == ">>")
                numPage = ViewBag.pageCount;
            else if (page == ">")
                numPage = lastPage + 1;
            else
                numPage = Convert.ToInt32(page);
            ViewBag.page = numPage;
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

        public RedirectToRouteResult GetStockPrices()
        {
            List<int> allCompanyIds = db.Companies.Select(x => x.companyID).ToList();

            foreach (int companyId in allCompanyIds)
                GetStockPriceByCompanyID(companyId);

            return RedirectToAction("Index");
        }

        public RedirectToRouteResult GetStockPriceByCompanyID(int companyId)
        {
            Company company = db.Companies.Find(companyId);
            if(company.CompanyStocks.Where(x => x.stockTime.Value.Date == DateTime.Now.Date).Any())
                return RedirectToAction("Index");
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL + "stock/" + company.companySymbol + "/quote");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                string strResponse = response.Content.ReadAsStringAsync().Result;
                
                string strOpenPrice = "";
                int index = strResponse.IndexOf(",\"openTime\"") - 1;
                for (; Char.IsDigit(strResponse[index]) || strResponse[index] == '.'; index--)
                {
                    strOpenPrice = strResponse[index] + strOpenPrice;
                }
                decimal openPrice;
                if (!Decimal.TryParse(strOpenPrice, out openPrice))
                    openPrice = -1;


                string strClosePrice = "";
                index = strResponse.IndexOf(",\"closeTime\"") - 1;
                for (; Char.IsDigit(strResponse[index]) || strResponse[index] == '.'; index--)
                {
                    strClosePrice = strResponse[index] + strClosePrice;
                }
                decimal closePrice;
                if (!Decimal.TryParse(strOpenPrice, out closePrice))
                    closePrice = -1;

                CompanyStock stockOpenPrice = new CompanyStock();
                stockOpenPrice.companyId = companyId;
                stockOpenPrice.stockTime = DateTime.Now;
                stockOpenPrice.stockPrice = openPrice;
                stockOpenPrice.isOpen = true;
                db.CompanyStocks.Add(stockOpenPrice);
                db.Entry(stockOpenPrice).State = EntityState.Added;

                CompanyStock stockClosePrice = new CompanyStock();
                stockClosePrice.companyId = companyId;
                stockClosePrice.stockTime = DateTime.Now;
                stockClosePrice.stockPrice = closePrice;
                stockClosePrice.isOpen = false;
                db.CompanyStocks.Add(stockClosePrice);
                db.Entry(stockClosePrice).State = EntityState.Added;

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
