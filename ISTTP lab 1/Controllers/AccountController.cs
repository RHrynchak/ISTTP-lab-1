using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISTTP_lab_1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string loginOrEmail, string password, string? returnUrl = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == loginOrEmail || u.Email == loginOrEmail);
            if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Неправильний логін або пароль.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
            await SignInUser(user);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsGuest()
        {
            var guest = new User
            {
                Username = "Guest_" + Guid.NewGuid().ToString().Substring(0, 8),
                Role = "User",
                PasswordHash = null,
                Email = null
            };

            _context.Users.Add(guest);
            await _context.SaveChangesAsync();

            await SignInUser(guest);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають!");
                return View();
            }
            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Цей логін вже зайнятий! Оберіть інший.");
                return View();
            }
            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Ця електронна пошта вже зареєстрована!");
                return View();
            }
            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    Email = email,
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "User"
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                await SignInUser(newUser);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true }; 

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}