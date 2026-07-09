using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.UsuarioDto;
using Web.Models.Modelo_Usuarios.UsuarioModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;

namespace Web.Controllers.Modulo_Usuarios
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioHttpServices _usuarioHttpServices;

        public UsuarioController(IUsuarioHttpServices usuarioHttpServices)
        {
            _usuarioHttpServices = usuarioHttpServices;
        }

        // GET: UsuarioControlle

        public async Task<ActionResult> Index(bool incluirInactivos = false, int pageNumber = 1)
        {
            try
            {
                var result = await _usuarioHttpServices.GetUsuariosAsync(incluirInactivos, pageNumber);
                ViewBag.IncluirInactivos = incluirInactivos;
                return View(result);
            }
            catch (Exception ex)
            {
                // 🔥 Detectar acceso denegado
                if (ex.Message.Contains("Forbidden"))
                {
                    TempData["Error"] = "⛔ No tienes permisos para acceder a Usuarios.";
                    return RedirectToAction("Index", "Home"); // o donde quieras
                }

                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

          

        // GET: UsuarioController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var result = await _usuarioHttpServices.GetUsuarioByIdAsync(id);
            return View(result);
        }

        // GET: UsuarioController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsuarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>Create(CreateUsuarioModel createUsuarioModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _usuarioHttpServices.CreateUsuarioAsync(createUsuarioModel);
                    return RedirectToAction(nameof(Index));
                }
                return View(createUsuarioModel);

            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(createUsuarioModel);
            }
        }

        // GET: UsuarioController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
              var usuario =  await _usuarioHttpServices.GetUsuarioByIdAsync(id);

                var model = new UpdateUsuarioModel
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    UserName = usuario.UserName,
                    FechaNacimiento = usuario.FechaNacimiento,
                 
                };

                return View(model);

            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");

            }

        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateUsuarioModel updateUsuarioModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _usuarioHttpServices.UpdateUsuarioAsync(updateUsuarioModel);

                    return RedirectToAction(nameof(Index));
                }
                return View(updateUsuarioModel);
            }
                
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(updateUsuarioModel);
            }
        }

        // GET: UsuarioController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var usuario =  await _usuarioHttpServices.GetUsuarioByIdAsync(id);
                if(usuario == null)
                {
                    return RedirectToAction(nameof(Index));

                }
                return View(usuario);
            }
            catch (Exception ex) 
            {
                return Content($"Error: {ex.Message}");

            }
        }



        // POST: UsuarioController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmate(int id)
        {
            try
            {
                await _usuarioHttpServices.DisableUsuarioAsync(id);

                return RedirectToAction(nameof(Index));
            }

            catch(Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);

              var usuario = await _usuarioHttpServices.GetUsuarioByIdAsync(id);

               return View(usuario);


            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(int id)
        {
            await _usuarioHttpServices.EnableUsuarioAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
