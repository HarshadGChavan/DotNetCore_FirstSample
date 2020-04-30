using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FirstSample.Models;
using FirstSample.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace FirstSample.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private IWebHostEnvironment _hostingEnvironment;

        public HomeController(IEmployeeRepository employeeRepository,
                                IWebHostEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            _hostingEnvironment =hostingEnvironment;
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            try
            {
                 var model = _employeeRepository.GetAllEmployees();
                 return View(model);
            }
            catch (Exception ex)
            {
                 ViewBag.ErrorTitle =ex.Message;
                 ViewBag.ErrorDescription = ex.StackTrace;
                 return View("Error");
            }
        }

        // [Route("Details/{id?}")]
        [AllowAnonymous]
        public ViewResult Details(int? id)
        {
            
             try
                {
                    Employee employee = _employeeRepository.GetEmployee(id.Value);
                    if(employee==null)
                    {
                        Response.StatusCode = 404;
                        return View("EmployeeNotFound",id.Value);
                    }

                    HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel(){
                    Employee = employee,
                    PageTitle ="Employee Details"
                    };

                    return View(homeDetailsViewModel);       
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorTitle =ex.Message;
                    ViewBag.ErrorDescription = ex.StackTrace;
                    return View("Error");
                }
        }

        [HttpGet]
        [ActionName("Create")]
        [Authorize]
        public ViewResult CreateGet()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [Authorize]
        public IActionResult CreatePost(EmployeeCreateViewModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    string uniqueFileName =ProcessUploadFile(model);

                    Employee newEmployee = new Employee{
                        Name=model.Name,
                        Email=model.Email,
                        Department=model.Department,
                        PhotoPath=uniqueFileName
                    };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details",new {id = newEmployee.Id});
                }
                return View();      
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

         [HttpGet]
        [ActionName("Edit")]
         [Authorize]
        public ViewResult EditGet(int id)
        {
           try
            {
               Employee employee = _employeeRepository.GetEmployee(id);
                EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel{
                    Id= employee.Id,
                    Name = employee.Name,
                    Email =employee.Email,
                    Department =employee.Department,
                    ExistingPhotoPath = employee.PhotoPath
                };
                return View(employeeEditViewModel);             
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        [ActionName("Edit")]
         [Authorize]
        public IActionResult EditPost(EmployeeEditViewModel model)
        {
             try
            {
                if(ModelState.IsValid)
                {
                    Employee employee = _employeeRepository.GetEmployee(model.Id);
                    employee.Name =model.Name;
                    employee.Email = model.Email;
                    employee.Department = model.Department;
                    if(model.Photo !=null)
                    {
                        if(model.ExistingPhotoPath !=null)
                        {
                        string filePath =  Path.Combine(_hostingEnvironment.WebRootPath,"images",model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                        }
                        employee.PhotoPath =ProcessUploadFile(model);
                    }

                _employeeRepository.Update(employee);
                return RedirectToAction("index");
                }
                return View();           
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle =ex.Message;
                ViewBag.ErrorDescription = ex.StackTrace;
                return View("Error");
            }
        }


        private string ProcessUploadFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
                if(model.Photo !=null)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath,"images");
                    uniqueFileName = Guid.NewGuid().ToString()+"_"+model.Photo.FileName;
                    string filePath = uploadsFolder +"\\"+ uniqueFileName;
                    using (var FileStream = new FileStream(filePath,FileMode.Create))
                    {
                        model.Photo.CopyTo(FileStream);
                    }
                }
            return uniqueFileName;
        }

    }
}
