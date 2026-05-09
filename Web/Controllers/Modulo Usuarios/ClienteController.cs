using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Models.ClienteModels;
using Web.Services.Interfaces.ServicesHttp;


namespace Web.Controllers.Modulo_Usuarios
{
    public class ClienteController : Controller
    {
        private readonly IClienteHttpServices _clienteHttpServices;

        public ClienteController(IClienteHttpServices clienteHttpServices)
        {
            _clienteHttpServices = clienteHttpServices;
        }
        // GET: ClienteController
        public async Task<ActionResult> Index()
        {
            var result = await _clienteHttpServices.GetClientesAsync();
            return View(result);

        }

        // GET: ClienteController/Details/5
        public async Task<ActionResult> Details(int id)
        {
           var result = await _clienteHttpServices.GetClienteByIdAsync(id);
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(string? nombre , string? telefono, string? correo)
        {
            try
            {
                var result = await _clienteHttpServices.GetBuscarClienteAsync(nombre, telefono, correo);
                return View("Index", result);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new List<ClienteModel>());



            }

        }





        // GET: ClienteController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ClienteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task  <ActionResult>Create(CreateClienteModel createClienteModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _clienteHttpServices.CreateClienteAsync(createClienteModel);


                    return RedirectToAction(nameof(Index));
                }
                return View(createClienteModel);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(createClienteModel);
            }


            
        }

        // GET: ClienteController/Edit/5
        public async Task <ActionResult> Edit(int id)
        {
            try
            {

                var cliente = await _clienteHttpServices.GetClienteByIdAsync(id);

                var model = new UpdateClientemodel
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Correo = cliente.Email,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Cedula= cliente.Cedula

                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }

        }

        // POST: ClienteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateClientemodel updateClienteModel)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    // 1. Actualizas los datos en la API
                    await _clienteHttpServices.UpdateClienteAsync(updateClienteModel);

                    // 2. ¡ESTA ES LA LÍNEA CLAVE! Redirige a la lista
                    return RedirectToAction(nameof(Index));
                }
                return View(updateClienteModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(updateClienteModel);
            }

            
        }


        // GET: ClienteController/Delete/5
        public async Task <ActionResult>Delete(int id)
        {
            try
            {
                var cliente = await _clienteHttpServices.GetClienteByIdAsync(id);

                if (cliente == null)
                    return RedirectToAction(nameof(Index));

                return View(cliente);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }



        // POST: ClienteController/Delete/5
        [HttpPost, ActionName("Delete")]
     
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> DeleteConfimated(int id)
        {
            try
            {
                await _clienteHttpServices.DeleteClienteAsync(id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // 🔥 Mostrar error en la vista
                ModelState.AddModelError("", ex.Message);

                // 🔥 Recargar el cliente para no romper la vista
                var cliente = await _clienteHttpServices.GetClienteByIdAsync(id);

                return View(cliente);
            }

        }
    }
}
