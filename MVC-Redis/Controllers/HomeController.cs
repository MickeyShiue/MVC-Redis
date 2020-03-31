using MVC_Redis.Models;
using MVC_Redis.Service;
using System.Web.Mvc;

namespace MVC_Redis.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var result =  RedisHelper.GetCacheData<Example>("Table");
            return View();
        }
    }
}