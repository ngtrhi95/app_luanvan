using BidARide.Models;
using BidARide.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BidARide.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/
        public ActionResult Login()
        {
            AccountLogin account = checkCookie();
            if (account == null)
                return View();
            Session["username"] = account.Username;
            return View("Welcome");
        }

        public ActionResult Logout()
        {
            Session.Remove("username");

            if (Response.Cookies["username"] != null)
            {
                HttpCookie ckUsername = new HttpCookie("username");
                ckUsername.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(ckUsername);
            }


            if (Response.Cookies["password"] != null)
            {
                HttpCookie ckPassword = new HttpCookie("password");
                ckPassword.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(ckPassword);
            }

            return View("Login");
        }

        private static readonly HttpClient client = new HttpClient();
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var values = new Dictionary<string, string>
            {
               { "username", model.account.Username },
               { "password", model.account.Password }
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://fast-hollows-58498.herokuapp.com/user/login", content);
            var responseString = await response.Content.ReadAsStringAsync();

            string s = responseString.ToString();
            if (s != "{\"status\":200}")
            {
                ViewBag.Error = "Username or Password is incorrect.";
                return View(model);
            }
            Session["username"] = model.account.Username;
            
            if (model.remember)
            {
                HttpCookie ckUsername = new HttpCookie("username");
                ckUsername.Expires = DateTime.Now.AddSeconds(3600);
                ckUsername.Value = model.account.Username;
                Response.Cookies.Add(ckUsername);

                HttpCookie ckPassword = new HttpCookie("password");
                ckPassword.Expires = DateTime.Now.AddSeconds(3600);
                ckPassword.Value = model.account.Password;
                Response.Cookies.Add(ckPassword);
            }

            return View("Welcome");
        }

        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                var values = new Dictionary<string, string>
                {
                   { "username", model.account.Username },
                   { "password", model.account.Password },
                   { "email", model.account.Email },
                   { "fullname", model.account.Fullname },
                   { "phone", model.account.Phone }

                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://fast-hollows-58498.herokuapp.com/user/register", content);
                var responseString = await response.Content.ReadAsStringAsync();

                string s = responseString.ToString();
                if (s != "OK")
                {
                    ViewBag.Error = "Username or Email already exist";
                    return View(model);
                }
                Session["username"] = model.account.Username;

                return View("Welcome");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public AccountLogin checkCookie()
        {
            AccountLogin account = null;
            string username = string.Empty, password = string.Empty;
            if (Request.Cookies["username"] != null)
                username = Request.Cookies["username"].Value;

            if (Request.Cookies["password"] != null)
                password = Request.Cookies["password"].Value;

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                account = new AccountLogin { Username = username, Password = password };

            return account;
        }

	}
}