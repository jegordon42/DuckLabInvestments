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
using DuckLab.ViewModels;


namespace DuckLab.Controllers
{
    public class PortfolioController : Controller
    {
        private ducklabdbEntities db = new ducklabdbEntities();

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            List<GameViewModel> games = new List<GameViewModel>();
            foreach(Game game in db.GameUsers.Where(x => x.userId == userId).Select(x => x.Game))
            {
                GameViewModel userGame = new GameViewModel();
                userGame.game = game;
                Player player = new Player();
                player.gameBalance = Convert.ToDouble(db.GameUsers.Where(x => x.userId ==userId && x.gameId == game.gameId).FirstOrDefault().availableBalance ?? 0);
                foreach (UserStock stock in db.UserStocks.Where(x => x.gameId == game.gameId && x.userId == userId))
                {
                    decimal stockPrice = stock.Company.CompanyStocks.OrderByDescending(x => x.stockTime).FirstOrDefault().stockPrice * stock.quantityPurchased ?? 0;
                    player.gameBalance += Convert.ToDouble(stockPrice);
                }
                userGame.Players.Add(player);
                games.Add(userGame);
            }

            return View(games);
            
        }

        public ActionResult Manage(int gameid)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            ViewBag.availableBalance = db.GameUsers.Where(x => x.gameId == gameid && x.userId == userId).Select(x => x.availableBalance).First() ?? 0;
            ViewBag.game = db.Games.Find(gameid);
            return View(db.UserStocks.Where(x => x.userId == userId && x.gameId == gameid).ToList());
        }

        public ActionResult Buy(int gameId)
        {
            ViewBag.gameId = gameId;
            return View();
        }

        public ActionResult SellStock(int gameId, int companyId, string error = "")
        {
            int userId = Convert.ToInt32(Session["userId"]);
            ViewBag.availableBalance = db.GameUsers.Where(x => x.gameId == gameId && x.userId == userId).First().availableBalance ?? 0;
            ViewBag.stockPrice = db.CompanyStocks.Where(x => x.companyId == companyId).OrderByDescending(x => x.stockTime).Select(x => x.stockPrice).First();
            ViewBag.companyName = db.Companies.Find(companyId).companyName;
            ViewBag.companyId = companyId;
            ViewBag.gameName = db.Games.Find(gameId).gameName;
            ViewBag.gameId = gameId;
            ViewBag.stocksOwned = db.UserStocks.Where(x => x.companyId == companyId && x.gameId == gameId && x.userId == userId).First().quantityPurchased;
            ViewBag.error = error;
            return View();
        }

        public ActionResult BuyStock(int gameId, int companyId, string error = "")
        {
            int userId = Convert.ToInt32(Session["userId"]);
            ViewBag.availableBalance = db.GameUsers.Where(x => x.gameId == gameId && x.userId == userId).First().availableBalance ?? 0;
            ViewBag.stockPrice = db.CompanyStocks.Where(x => x.companyId == companyId).OrderByDescending(x => x.stockTime).Select(x => x.stockPrice).First();
            ViewBag.companyName = db.Companies.Find(companyId).companyName;
            ViewBag.companyId = companyId;
            ViewBag.gameName = db.Games.Find(gameId).gameName;
            ViewBag.gameId = gameId;
            ViewBag.error = error;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SellStock(int gameId, int companyId, int quantity)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            decimal stockPrice = db.CompanyStocks.Where(x => x.companyId == companyId).OrderByDescending(x => x.stockTime).First().stockPrice ?? 0;
            decimal totalPrice = stockPrice * quantity;
            GameUser player = db.GameUsers.Where(x => x.userId == userId && x.gameId == gameId).First();
            UserStock stock = db.UserStocks.Where(x => x.gameId == gameId && x.companyId == companyId && x.userId == userId).First();
            if (stock.quantityPurchased < quantity)
                return RedirectToAction("SellStock", new { gameId = gameId, companyId = companyId, error = "Quantity of " + quantity + " exceeds quantity owned." });
            else if(stock.quantityPurchased == quantity)
            {
                db.UserStocks.Remove(stock);
                player.availableBalance += totalPrice;
                db.Entry(stock).State = EntityState.Deleted;
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                stock.quantityPurchased -= quantity;
                player.availableBalance += totalPrice;
                db.Entry(stock).State = EntityState.Modified;
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Manage", new { gameId = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyStock(int gameId, int companyId, int quantity)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            decimal stockPrice = db.CompanyStocks.Where(x => x.companyId == companyId).OrderByDescending(x => x.stockTime).First().stockPrice ?? 0;
            GameUser player = db.GameUsers.Where(x => x.userId == userId && x.gameId == gameId).First();
            decimal availableBalance = player.availableBalance ?? 0;
            decimal totalPrice = stockPrice * quantity;
            if (availableBalance < stockPrice*quantity)
                return RedirectToAction("BuyStock", new {gameId = gameId, companyId = companyId, error = "Purchace of $" + totalPrice + " exceeds Available Balance" });
            UserStock Stock = new UserStock();
            Stock.quantityPurchased = quantity;
            Stock.companyId = companyId;
            Stock.gameId = gameId;
            Stock.userId = userId;
            db.UserStocks.Add(Stock);
            db.Entry(Stock).State = EntityState.Added;

            player.availableBalance = availableBalance - totalPrice;
            db.Entry(player).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Manage", new { gameId = gameId});
        }

        public PartialViewResult _Companies(int gameId, string name = "", string symbol = "", int lastPage = 0, string page = "")
        {
            ViewBag.name = name;
            ViewBag.symbol = symbol;
            ViewBag.gameId = gameId;
            ViewBag.pageCount = db.Companies.Where(x => x.companyName.StartsWith(name) && x.companySymbol.StartsWith(symbol)).Count();
            if (ViewBag.pageCount % 25 == 0)
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
