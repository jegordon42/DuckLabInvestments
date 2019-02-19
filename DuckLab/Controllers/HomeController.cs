using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using DuckLab.Models;
using System.Data.Entity;

namespace DuckLab.Controllers
{
    public class HomeController : Controller
    {
        private ducklabdbEntities db = new ducklabdbEntities();
        public static string baseURL = "https://api.iextrading.com/1.0/";

        public ActionResult Index()
        {
            return View(db.Companies.Take(10).ToList());
        }

        public RedirectToRouteResult GetStocks()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL + "ref-data/symbols");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                string strResponse = response.Content.ReadAsStringAsync().Result;
                while(strResponse.IndexOf("symbol") != -1)
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

                    Company newCompany = new Company();
                    newCompany.companyName = companyName;
                    newCompany.companySymbol = symbol;
                    db.Companies.Add(newCompany);
                    db.Entry(newCompany).State = EntityState.Added;

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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}