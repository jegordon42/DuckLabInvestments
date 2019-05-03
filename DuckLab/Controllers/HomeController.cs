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
        
        public ActionResult Index()
        {
            if (Session["userId"] == null)
                return RedirectToAction("Login", "Users");

            ViewBag.name = db.Users.Find(Convert.ToInt32(Session["userId"])).firstName;
            ViewBag.userId = db.Users.Find(Convert.ToInt32(Session["userId"])).userID;
            return View();
        }

        public ActionResult Training()
        {
            if (Session["userId"] == null)
                return RedirectToAction("Login", "Users");
            return View();
        }


        public JsonResult GetGameInfo(int userId)
        {
            User user = db.Users.Find(userId);
            IEnumerable<Game> games = db.GameUsers.Where(x => x.userId == userId).Select(x => x.Game);

            string gameString = "{\"games\":[";
            foreach(Game game in games)
            {
                if(game == games.Last())
                    gameString += "\"" + game.gameName + "\"]";
                else
                    gameString += "\"" + game.gameName + "\",";
            }

            gameString += "\"balances\":[";
            foreach (Game game in games)
            {
                decimal balance = user.GameUsers.Where(x => x.gameId == game.gameId).First().availableBalance ?? 0;
                foreach (UserStock stock in db.UserStocks.Where(x => x.gameId == game.gameId && x.userId == userId))
                {
                    decimal stockPrice = stock.Company.CompanyStocks.OrderByDescending(x => x.stockTime).FirstOrDefault().stockPrice * stock.quantityPurchased ?? 0;
                    balance += stockPrice;
                }
                
                if (game == games.Last())
                    gameString += "\"" + balance.ToString() + "\"]}";
                else
                    gameString += "\"" + balance.ToString() + "\",";
            }

            JsonResult result = new JsonResult() {
                Data = gameString,
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            return result;
        }
    }
}