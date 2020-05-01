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
using System;
using System.Security.Claims;

namespace FirstSample.Controllers
{
    // Decorated with autorize to only allow controller acess only with Admin User
    [Authorize(Roles ="Admin")]
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
        [ActionName("ListUsers")]
        public IActionResult ListUsers_Get()
        {
            var usersList = _userManager.Users;
            return View(usersList);
        }

        [HttpGet]
        [ActionName("EditUser")]
        public async Task<IActionResult> EditUser_Get(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if(user == null)
                {
                    ViewBag.ErrorMessage =$"User with ID {id} not found.";
                    return View("Not Found");
                }

                var userClaims = await _userManager.GetClaimsAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);

                var model = new EditUserViewModel{
                    Id = user.Id,
                    Name = user.UserName,
                    Email=user.Email,
                    City=user.City,
                    Claims=userClaims.Select(c=>c.Type +"  : "+ c.Value).ToList(),
                    Roles=userRoles.ToList()
                };

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
        [ActionName("EditUser")]
        public async Task<IActionResult> EditUser_Post(EditUserViewModel model)
        {
            try
            {
                 var user = await _userManager.FindByIdAsync(model.Id);

                if(user==null)
                {
                    ViewBag.ErrorMessage =$"User with id {model.Id} not found";
                    return View("Not Sound");
                }
                else
                {
                    user.Email = model.Email;
                    user.City=model.City;
                    user.UserName = model.Name;

                    var result = await _userManager.UpdateAsync(user);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("ListUsers");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty,error.Description);
                    }

                    return View(model);
                }            
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

         [HttpPost]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUser_Post(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if(user ==null)
                {
                    ViewBag.ErrorMessage =$"User with ID {id} not found.";
                    return View("Not Found");
                }

                var result = await _userManager.DeleteAsync(user);
                if(result.Succeeded)
                {
                return RedirectToAction("ListUsers","Administration");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty,error.Description);
                    }
                }
                return View("ListUsers");            
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        [ActionName("DeleteRole")]
        [Authorize(Policy="DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole_Post(string id)
        {
            try
            {
                   var role = await _roleManager.FindByIdAsync(id);
                    if(role == null)
                    {
                        // in case role not found ... return ack to not found action
                        ViewBag.ErrorMessage =$"Role with ID : {id} not found.";
                        return View("Not Found");
                    }

                    var result = await _roleManager.DeleteAsync(role);
                    if(result.Succeeded)
                        return RedirectToAction("ListRoles");

                    // in case any error while deleting role thenreturn back to view
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty,error.Description);
                    }

                    return View("ListRoles");          
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
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
            try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
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
            try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

         [HttpPost]
        [ActionName("EditRole")]
        public async Task<IActionResult> EditRolePost(EditRoleViewModel  model)
        {
            try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpGet]
        [ActionName("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole_Get(string roleId)
        {
           try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        [ActionName("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole_Post(List<UserRoleViewModel> model,string roleId)
        {   
             try
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
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpGet]
        [ActionName("ManageUserRoles")]
        public async Task<IActionResult> ManageUserRoles(string userid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userid);
                if(user == null)
                {
                    ViewBag.ErrorMessage=$"User with id:{userid} can not found.";
                    return View("Not Found");
                }

                var roles = _roleManager.Roles;
                List<ManageUserRolesViewModel> lstmanageUserRolesViewModel = new List<ManageUserRolesViewModel>();
                foreach (var itemRole in roles.ToList())
                {
                    var manageUserRolesViewModel = new ManageUserRolesViewModel{
                            roleId = itemRole.Id,
                            roleName = itemRole.Name
                    };

                      if(await _userManager.IsInRoleAsync(user,itemRole.Name))
                          manageUserRolesViewModel.isSelected = true;
                      else 
                            manageUserRolesViewModel.isSelected = false;
                    lstmanageUserRolesViewModel.Add(manageUserRolesViewModel);
                }
                ViewBag.UserID = user.Id; 
                return View(lstmanageUserRolesViewModel);             
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

         [HttpPost]
        [ActionName("ManageUserRoles")]
        public async Task<IActionResult> ManageUserRoles_Post(List<ManageUserRolesViewModel> model ,string userid)
        {
            try
            {
                 var user = await _userManager.FindByIdAsync(userid);
                    if(user==null)
                    {
                        ViewBag.ErrorMessage=$"User with Id : {userid} can not be found.";
                        return View("NotFound");
                    }

                    int i =0;
                    foreach (var itemRole in model.ToList())
                    {
                        var role = await _roleManager.FindByIdAsync(itemRole.roleId);
                        IdentityResult result =null;

                        if(itemRole.isSelected && !(await _userManager.IsInRoleAsync(user,role.Name)) )
                        {
                            //... if user selected and not in role then add user from role
                            result =await _userManager.AddToRoleAsync(user,role.Name);
                        }
                        else if (!(itemRole.isSelected) && (await _userManager.IsInRoleAsync(user,role.Name)))
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
                                return RedirectToAction("EditUser",new {Id = user.Id});
                        }

                        i++;
                    }

                return RedirectToAction("EditUser",new {Id = user.Id});

            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpGet]
        [ActionName("ManageClaims")]
        public async Task<IActionResult> ManageClaims_Get(string userid)
        {
            try
            {
               var user = await _userManager.FindByIdAsync(userid);
               if(user==null)
               {
                  ViewBag.ErrorMessage =$"User with ID: {userid} not found.";
                  return View("NotFound");
               }

               var exisitingUserClaim = await _userManager.GetClaimsAsync(user);
               var model = new UserClaimsViewModel(){
                   userID = userid
               };

               foreach (var itemClaim in ClaimsStore.AllClaims)
               {
                   var userClaim = new UserClaim(){
                        ClaimType = itemClaim.Type
                   };
                   
                   if(exisitingUserClaim.Any(c =>c.Type == itemClaim.Type && c.Value =="true"))
                       userClaim.IsSelected = true;
                    else 
                        userClaim.IsSelected = false;

                 model.Claims.Add(userClaim);
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
        [ActionName("ManageClaims")]
        public async Task<IActionResult> ManageClaims_Post(UserClaimsViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.userID);
                if(user == null)
                {
                    ViewBag.ErrorMessage =$"User with ID: {model.userID} not found";
                    return View("NotFound");
                }

                var exisitingUserClaims = await _userManager.GetClaimsAsync(user);
                var removeResult = await _userManager.RemoveClaimsAsync(user,exisitingUserClaims);

                if(!removeResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty,"Cananot remove users existing claims.");
                    return View(model);
                }

                var result = await _userManager.AddClaimsAsync(
                    user,
                    model.Claims.Select(c=>new Claim(c.ClaimType,c.IsSelected ? "true" : "false") )
                  );

                if(!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty,"Ca not add selected claims for user");
                    return View(model);
                }

              return RedirectToAction("EditUser",new{id=model.userID});

            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

    }
}