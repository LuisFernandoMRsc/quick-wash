using Microsoft.AspNetCore.Mvc;
using QuickWash.Services;

namespace QuickWash.Controllers;

public class MaquinasController : Controller
{
    private readonly IReservaService _reservaService;

    public MaquinasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fecha)
    {
        var fechaConsulta = fecha ?? DateTime.Today;
        var viewModel = await _reservaService.ObtenerDisponibilidadAsync(fechaConsulta);
        return View(viewModel);
    }
}
