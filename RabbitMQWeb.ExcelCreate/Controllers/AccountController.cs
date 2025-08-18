using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var hasUser= await _userManager.FindByEmailAsync(Email);

            if (hasUser == null)
            {
                return View();
            }
            var signInResult = await _signInManager.PasswordSignInAsync(hasUser,Password,true,false);

            if (!signInResult.Succeeded) 
            {
                return View();
            }

            return RedirectToAction(nameof(HomeController.Index),"Home");
        }




    }
}
