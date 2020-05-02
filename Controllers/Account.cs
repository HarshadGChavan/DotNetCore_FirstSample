using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using FirstSample.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using FirstSample.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace FirstSample.Controllers
{
    [AllowAnonymous]
    public class Account : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Account(UserManager<ApplicationUser> userManager,
                       SignInManager<ApplicationUser> signInManager)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

         [HttpPost][HttpGet]
        [ActionName("IsEmailAvailable")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailAvailable(string  email)
        {
            try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
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
             try
            {
                if(ModelState.IsValid)
                {
                    // Create identity user and assign values for userid and Email
                    var user = new ApplicationUser()
                    {
                        UserName=model.Email ,
                        Email=model.Email,
                        City =model.City
                    };

                    // call async method of usermanger to create user and store it in Database
                    var Result = await _userManager.CreateAsync(user,model.Password);
                    // If result is success i.e. user created
                    if(Result.Succeeded)
                    {
                        if(_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                        {
                            return RedirectToAction("ListUsers","Administration");
                        }
                        await _signInManager.SignInAsync(user,isPersistent:false);
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
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
        public async  Task<IActionResult> LoginGet(string returnUrl)
        {
             try
            {
                LoginViewModel loginViewModel = new LoginViewModel()
                {
                ReturnURL = returnUrl,   
                ExternalLogin = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
                };

                return View(loginViewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }


         [HttpPost]
         [ActionName("Login")]
         [AllowAnonymous]
        public async Task<IActionResult> LoginPost(LoginViewModel model,string returnUrl)
        {
             try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName("ExternalLogin")]
        public IActionResult ExternalLogin_Get(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback","Account",
                            new {ReturnUrl = returnUrl});

            var properties = _signInManager.
                            ConfigureExternalAuthenticationProperties(provider,redirectUrl);
            return new ChallengeResult(provider,properties);            
        }


        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null,string remoteError =null)
        {
            // in case of blank returnurl set it to home page
            returnUrl = returnUrl ?? Url.Content("/Home/index");

            var loginViewModel = new LoginViewModel{
                ReturnURL = returnUrl,
                ExternalLogin= (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if(remoteError!=null)
            {
                ModelState.AddModelError(string.Empty,$"Error from external sign in provider {remoteError}");
                return View("Login",loginViewModel);
            }

            var info =await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                 ModelState.AddModelError(string.Empty,$"Error loading external login information.");
                return View("Login",loginViewModel);
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
            info.ProviderKey,isPersistent:false,bypassTwoFactor:true);

            if(signInResult.Succeeded)
            {
                // user sign in successfull
                return LocalRedirect(returnUrl);
            }
            else
            {
                // In case user dont have existing account create and sign in
                
                //... Fetch email first
                var email =info.Principal.FindFirstValue(ClaimTypes.Email);
                
                if(email !=null)
                {
                    // try to get user with email ...
                    var user = await _userManager.FindByEmailAsync(email);

                    if(user == null)
                    {
                        user = new ApplicationUser{
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await _userManager.CreateAsync(user);
                    }

                    //... if we found email then we have local account for user sign in user
                    await _userManager.AddLoginAsync(user,info);
                    await _signInManager.SignInAsync(user,isPersistent:false);

                    return LocalRedirect(returnUrl);
                }

                // in case we dont have email from provider 
                ViewBag.ErrorTitle = $"Error claim not received from :{info.LoginProvider}";
                ViewBag.ErrorDescription = "Please contact us on Email";
                return View("Error");
            }

        }

    }
}