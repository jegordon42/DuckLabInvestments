using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DuckLab.Models;

namespace DuckLab.Controllers
{
    public class UsersController : Controller
    {
        private ducklabdbEntities db = new ducklabdbEntities();

        // GET: Users
        public ActionResult Login(string error = "")
        {
            ViewBag.error = error;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email = "", string password = "" )
        {
            User user = db.Users.Where(x => x.email == email && x.password == password).FirstOrDefault();
            if(user == null)
                return RedirectToAction("Login", new {error = "Invalid Email and Password." });
            else
            {
                Session["userId"] = user.userID;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult SignUp(string email = "", string firstName = "", string lastName = "", string password = "", string passwordConfirm = "")
        {
            if(password != passwordConfirm)
                return Redirect(Url.RouteUrl(new { controller = "Users", action = "Login"}) + "?error=passwords%20dont%20match#toregister");
            User user = new User();
            user.username = "";
            user.email = email;
            user.firstName = firstName;
            user.lastName = lastName;
            user.password = password;

            db.Users.Add(user);
            db.Entry(user).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
            Session["userId"] = user.userID;
            return RedirectToAction("Index", "Home");
        }
        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userID,firstName,lastName,username,password,email,active")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userID,firstName,lastName,username,password,email,active")] User user)
        {
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
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
