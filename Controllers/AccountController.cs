using Blockchain_Supply_Chain_Tracking_System.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain_Supply_Chain_Tracking_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupplyTrackingContext _context;

        public AccountController(SupplyTrackingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Поиск пользователя в базе данных
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            string passwordHash = "";
            if (password == null) password = passwordHash;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Преобразуем строку в массив байтов и вычисляем хеш
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Преобразуем массив байтов в строку шестнадцатеричных чисел
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                passwordHash = builder.ToString();
            }

            if (user != null && passwordHash == user.Passwordhash)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()), // Add UserId as a claim
                new Claim(ClaimTypes.Role, user.Role)
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                return RedirectToAction("Index", "Tracking");
            }


            // Если логин неудачный
            ViewBag.ErrorMessage = "Invalid username or password";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}
