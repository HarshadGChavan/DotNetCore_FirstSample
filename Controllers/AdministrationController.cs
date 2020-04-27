using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using FirstSample.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using FirstSample.Models;
using System.Linq;

namespace FirstSample.Controllers
{
    
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
        }

        [HttpGet]
        [ActionName("CreateRole")]
        public IActionResult CreateRole_Get()
        {
            return View();
        }


        [HttpPost]
        [ActionName("CreateRole")]
        public async Task<IActionResult> CreateRole_Post(CreateRoleViewModel model)
        {
            if(ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole 
                {
                    Name = model.RoleName
                };

             var result =await _roleManager.CreateAsync(identityRole);
             if(result.Succeeded)
             {
                return RedirectToAction("ListRoles","Administration");
             }

             foreach (var error in result.Errors )
             {
                 ModelState.AddModelError(string.Empty,error.Description);
             } 
                
            }

            return View(model);
        }


        [HttpGet]
        [ActionName("ListRoles")]
        public IActionResult ListRoles()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        [ActionName("EditRole")]
        public async Task<IActionResult> EditRoleGet(string id)
        {
           var roleResult = await _roleManager.FindByIdAsync(id) ;
           if(roleResult == null)
           {
               ViewBag.ErrorMessage =$"Role with id: {id} not found.";
               return RedirectToAction("NotFound");
           }

            // else if we have role then set edit role view model elements and pass it 
           var model =new  EditRoleViewModel {
               Id=roleResult.Id,
               RoleName = roleResult.Name
           };

            // .... Iterate through all users and find users in role selected
                foreach (var itemUser in _userManager.Users.ToList() )
                {
                   if( await _userManager.IsInRoleAsync(itemUser,roleResult.Name))
                   {
                      model.Users.Add(itemUser.UserName);
                   }
                }
                          
            return View(model);
        }

         [HttpPost]
        [ActionName("EditRole")]
        public async Task<IActionResult> EditRolePost(EditRoleViewModel  model)
        {
            var roleResult = await _roleManager.FindByIdAsync(model.Id);
             if(roleResult == null)
           {
               ViewBag.ErrorMessage =$"Role with id: {model.Id} not found.";
               return RedirectToAction("NotFound");
           }
            else
            {
                roleResult.Name = model.RoleName;
                var result = await _roleManager.UpdateAsync(roleResult);
                if(result.Succeeded)
                {
                    return RedirectToAction("ListRoles","Administration");
                }
                else
                {
                    foreach (var itemError in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty,itemError.Description);
                    }
                }
            }
            return View(model);

        }

        [HttpGet]
        [ActionName("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole_Get(string roleId)
        {
            ViewBag.roleId = roleId;
            var role =await _roleManager.FindByIdAsync(roleId);
            if(role == null)
            {
                ViewBag.ErrorMessage =$"Role with ID: {roleId} cannot be found.";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();
            foreach (var user in _userManager.Users.ToList())
            {
                var userRole = new UserRoleViewModel(){
                    UserId=user.Id,
                    UserName = user.UserName,
                };
                
                  if(await _userManager.IsInRoleAsync(user,role.Name))
                  {
                     userRole.IsSelected = true;
                  }
                  else 
                       userRole.IsSelected = false;

                model.Add(userRole);
            }           

            return View(model);
        }

        [HttpPost]
        [ActionName("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole_Post(List<UserRoleViewModel> model,string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if(role==null)
            {
                ViewBag.ErrorMessage=$"Role with Id : {roleId} can not be found.";
                return View("NotFound");
            }

            int i =0;
            foreach (var itemUserRole in model.ToList())
            {
                var user = await _userManager.FindByIdAsync(itemUserRole.UserId);
                IdentityResult result =null;

                if(itemUserRole.IsSelected && !(await _userManager.IsInRoleAsync(user,role.Name)) )
                {
                    //... if user selected and not in role then add user from role
                     result =await _userManager.AddToRoleAsync(user,role.Name);
                }
                else if (!(itemUserRole.IsSelected) && (await _userManager.IsInRoleAsync(user,role.Name)))
                {
                    //... if user not selected and already in role then remove user from role
                    result =await _userManager.RemoveFromRoleAsync(user,role.Name);
                }
                else
                    continue;

                if(result.Succeeded)
                {
                    if(i < model.Count - 1)
                        continue;
                    else
                        return RedirectToAction("EditRole",new {Id = roleId});
                }

                i++;
            }

          return RedirectToAction("EditRole",new {Id = roleId});
        }

    }
}