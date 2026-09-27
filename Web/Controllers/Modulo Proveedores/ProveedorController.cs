using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using Web.Models.Modelo_Proveedores.ProveedorModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores;

namespace Web.Controllers.Modulo_Proveedores
{
    public class ProveedorController : Controller
    {
        private readonly IProveedorHttpServices _proveedorHttpServices;

        public ProveedorController(IProveedorHttpServices proveedorHttpServices)
        {
            _proveedorHttpServices = proveedorHttpServices;
        }

        public async Task<ActionResult> Index(bool incluirInactivos = false, int pageNumber = 1)
        {
            var result = await _proveedorHttpServices.GetProveedoresAsync(incluirInactivos, pageNumber);
            ViewBag.IncluirInactivos = incluirInactivos;
            ViewBag.EsAdministrador = EsAdministrador();
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(string? nombre, bool incluirInactivos = false)
        {
            ViewBag.IncluirInactivos = incluirInactivos;
            ViewBag.NombreBuscado = nombre;

            try
            {
                var result = await _proveedorHttpServices.BuscarProveedoresAsync(nombre, incluirInactivos);
                var paged = new PagedResult<ProveedorModel> { Items = result, PageNumber = 1, PageSize = result.Count, TotalCount = result.Count };
                return View("Index", paged);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new PagedResult<ProveedorModel>());
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            var result = await _proveedorHttpServices.GetProveedorByIdAsync(id);
            return View(result);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateProveedorModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _proveedorHttpServices.CreateProveedorAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var proveedor = await _proveedorHttpServices.GetProveedorByIdAsync(id);

                var model = new UpdateProveedorModel
                {
                    Id = proveedor.Id,
                    Nombre = proveedor.Nombre,
                    Telefono = proveedor.Telefono,
                    Correo = proveedor.Correo,
                    Direccion = proveedor.Direccion
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateProveedorModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _proveedorHttpServices.UpdateProveedorAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            if (!EsAdministrador())
                return SinPermiso();

            try
            {
                var proveedor = await _proveedorHttpServices.GetProveedorByIdAsync(id);

                if (proveedor == null)
                    return RedirectToAction(nameof(Index));

                return View(proveedor);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmado(int id)
        {
            if (!EsAdministrador())
                return SinPermiso();

            try
            {
                await _proveedorHttpServices.DisableProveedorAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Se recarga el proveedor para poder volver a pintar la vista con el error.
                ModelState.AddModelError("", ex.Message);
                var proveedor = await _proveedorHttpServices.GetProveedorByIdAsync(id);
                return View(proveedor);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(int id)
        {
            if (!EsAdministrador())
                return SinPermiso();

            await _proveedorHttpServices.EnableProveedorAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EsAdministrador() =>
            HttpContext.Session.GetString("ROL") == "Administrador";

        private ActionResult SinPermiso()
        {
            TempData["Error"] = "No tenés permisos para realizar esta acción.";
            return RedirectToAction(nameof(Index));
        }
    }
}
