using Microsoft.AspNetCore.Mvc;

namespace Projeto_do_semestre.Controllers
{
    public class AreaPublicaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult VeiculosImportados()
        {
            return View();
        }

        public IActionResult VeiculosNacionais()
        {
            return View();
        }


    }
}
