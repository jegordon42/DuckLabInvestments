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
    public class GamesController : Controller
    {
        private ducklabdbEntities db = new ducklabdbEntities();

        public ActionResult Index()
        {
            return View();
            
        }

        public ActionResult Create()
        {
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Game game)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            game.adminId = userId;
            game.gameStatus = "New";
            if (ModelState.IsValid)
            {
                db.Games.Add(game);
                db.Entry(game).State = EntityState.Added;
                db.SaveChanges();

                GameUser player = new GameUser();
                player.availableBalance = game.startingBalance;
                player.userId = userId;
                player.gameId = game.gameId;
                db.GameUsers.Add(player);
                db.Entry(player).State = EntityState.Added;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(game);
        }

        public PartialViewResult _Games(string gameName = "", string gameType = "", int lastPage = 0,  string page = "")
        {
            ViewBag.gameName = gameName;
            ViewBag.gameType = gameType;
            ViewBag.pageCount = db.Games.Where(x => x.gameName.StartsWith(gameType) && x.gameType.StartsWith(gameType)).Count();
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

            return PartialView(db.Games.Where(x => x.gameName.StartsWith(gameName) && x.gameType.StartsWith(gameType)).OrderBy(x => x.gameName).Skip(numPage * 25).Take(25).ToList());
        }

        public ActionResult Details(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            ViewBag.joined = false;
            DuckLab.ViewModels.GameViewModel theGame = new ViewModels.GameViewModel();
            theGame.game = game;
            foreach(GameUser user in game.GameUsers)
            {
                if (user.userId == userId)
                    ViewBag.joined = true;
                DuckLab.ViewModels.Player player = new ViewModels.Player();
                player.name = user.User.firstName + " " + user.User.lastName;
                player.gameBalance = Convert.ToDouble(user.availableBalance);
                foreach(UserStock stock in db.UserStocks.Where(x=>x.gameId == id && x.userId == user.userId))
                {
                    decimal stockPrice = stock.Company.CompanyStocks.OrderByDescending(x => x.stockTime).FirstOrDefault().stockPrice * stock.quantityPurchased ?? 0;
                    player.gameBalance += Convert.ToDouble(stockPrice);
                }
                theGame.Players.Add(player);
            }
            return View(theGame);
        }

        public ActionResult Join(int gameId)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            GameUser player = new GameUser();
            player.gameId = gameId;
            player.userId = userId;
            player.availableBalance = db.Games.Find(gameId).startingBalance;
            db.GameUsers.Add(player);
            db.Entry(player).State = EntityState.Added;
            db.SaveChanges();
            return RedirectToAction("Details", new {id = gameId });

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
