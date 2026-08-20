using Microsoft.AspNetCore.Mvc;

namespace ResumeTailorAI.Controllers;

public class ResumeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Analysis()
    {
        return View();
    }

    public IActionResult Result()
    {
        return View();
    }

    public IActionResult Approval()
    {
        return View();
    }

    public IActionResult Download()
    {
        return View();
    }
}
