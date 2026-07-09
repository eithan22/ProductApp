using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Productos.CategoriaModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;

namespace Web.Controllers.Modulo_Productos
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaHttpServices _categoriaHttpServices;

        public CategoriaController(ICategoriaHttpServices categoriaHttpServices)
        {
            _categoriaHttpServices = categoriaHttpServices;
        }

        public async Task<ActionResult> Index(bool incluirInactivos = false, int pageNumber = 1)
        {
            var result = await _categoriaHttpServices.GetCategoriasPagedAsync(incluirInactivos, pageNumber);
            ViewBag.IncluirInactivos = incluirInactivos;
            return View(result);
        }

        public async Task<ActionResult> Details(int id)
        {
            var result = await _categoriaHttpServices.GetCategoriaByIdAsync(id);
            return View(result);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCategoriaModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoriaHttpServices.CreateCategoriaAsync(model);
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
                var categoria = await _categoriaHttpServices.GetCategoriaByIdAsync(id);

                var model = new UpdateCategoriaModel
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion
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
        public async Task<ActionResult> Edit(UpdateCategoriaModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoriaHttpServices.UpdateCategoriaAsync(model);
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
            try
            {
                var categoria = await _categoriaHttpServices.GetCategoriaByIdAsync(id);
                if (categoria == null)
                    return RedirectToAction(nameof(Index));

                return View(categoria);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _categoriaHttpServices.DeleteCategoriaAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categoria = await _categoriaHttpServices.GetCategoriaByIdAsync(id);
                return View(categoria);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(int id)
        {
            await _categoriaHttpServices.EnableCategoriaAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
