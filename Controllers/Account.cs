using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using FirstSample.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace FirstSample.Controllers
{
    public class Account : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public Account(UserManager<IdentityUser> userManager,
                       SignInManager<IdentityUser> signInManager)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

         [HttpPost][HttpGet]
        [ActionName("IsEmailAvailable")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailAvailable(string  email)
        {
            var result =await _userManager.FindByEmailAsync(email);
            if(result == null)
            {
                // if user not avilable, then return true
                return Json(true);
            }
            else
            {
                return Json($"{email} already in use. Please enter another userid.");
            }
        }

        [HttpGet]
        [ActionName("Register")]
        [AllowAnonymous]
        public IActionResult RegisterGet()
        {
            return View();
        }


        [HttpPost]
        [ActionName("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPost(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                // Create identity user and assign values for userid and Email
                var identityUser = new IdentityUser()
                {
                    UserName=model.Email ,
                    Email=model.Email
                };

                // call async method of usermanger to create user and store it in Database
                var Result = await _userManager.CreateAsync(identityUser,model.Password);
                // If result is success i.e. user created
                if(Result.Succeeded)
                {
                    await _signInManager.SignInAsync(identityUser,isPersistent:false);
                    return RedirectToAction("Index","Home");
                }
                
                //loopthrough and send al error back from Result.Errors
                foreach (var error in Result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
           await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        [ActionName("Login")]
        [AllowAnonymous]
        public IActionResult LoginGet()
        {

                return View();
        }


         [HttpPost]
         [ActionName("Login")]
         [AllowAnonymous]
        public async Task<IActionResult> LoginPost(LoginViewModel model,string returnUrl)
        {
            if(ModelState.IsValid)
            {
                var result =await _signInManager.PasswordSignInAsync(
                 model.Email,
                 model.Password,
                 model.RememberMe,
                 false  );

                if(result.Succeeded)
                {
                    if(!string.IsNullOrEmpty(returnUrl))
                        return LocalRedirect(returnUrl);
                   return  RedirectToAction("Index","Home");
                }
                    ModelState.AddModelError(string.Empty,"Invalid Login Attempt");
            }

                return View(model);
        }

    }
}