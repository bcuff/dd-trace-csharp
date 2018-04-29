using Microsoft.AspNetCore.Mvc;

namespace TestWebApp
{
    [Route("")]
    public class HomeController : Controller
    {
        [Route("")]
        public string Index()
        {
            return "Hello, World!";
        }
    }
}
