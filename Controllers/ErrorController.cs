using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FirstSample.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController>  logger)
        {
            this._logger = logger;
        }


        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult =HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage ="Sorry, the resource you requested could not be found.";
                    _logger.LogWarning($"404 Error Occured. Path = {statusCodeResult.OriginalPath}"+
                    $"and QueryString = {statusCodeResult.OriginalQueryString}");
                    break;
            }

            return View("NotFound");
        }

    [Route("Error")]
    [AllowAnonymous]
  public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            _logger.LogError($"The Path {exceptionDetails.Path} threw new exception " +
                    $"{exceptionDetails.Error}");
            
                // ViewBag.ExceptionPath = exceptionDetails.Path;
                // ViewBag.ExceptionMesage = exceptionDetails.Error.Message;
                // ViewBag.ExceptionStacktrace = exceptionDetails.Error.StackTrace;

            return View("Error");
        }

    }
}