(function () {
    'use strict';

    var cuerpo = document.body;
    var sidebar = document.getElementById('app-sidebar');
    var botonAbrir = document.querySelector('[data-sidebar-abrir]');

    if (sidebar && botonAbrir) {
        var cerradores = document.querySelectorAll('[data-sidebar-cerrar]');

        var abrirSidebar = function () {
            cuerpo.classList.add('sidebar-abierto');
            botonAbrir.setAttribute('aria-expanded', 'true');

            var primero = sidebar.querySelector('[data-sidebar-cerrar]') || sidebar.querySelector('a');
            if (primero) { primero.focus(); }
        };

        var cerrarSidebar = function (devolverFoco) {
            if (!cuerpo.classList.contains('sidebar-abierto')) { return; }

            cuerpo.classList.remove('sidebar-abierto');
            botonAbrir.setAttribute('aria-expanded', 'false');

            // sin esto el foco queda en un elemento que pasa a visibility:hidden
            if (devolverFoco) { botonAbrir.focus(); }
        };

        botonAbrir.addEventListener('click', abrirSidebar);

        Array.prototype.forEach.call(cerradores, function (elemento) {
            elemento.addEventListener('click', function () { cerrarSidebar(true); });
        });

        sidebar.addEventListener('click', function (evento) {
            if (evento.target.closest('a')) { cerrarSidebar(false); }
        });

        document.addEventListener('keydown', function (evento) {
            if (evento.key === 'Escape') { cerrarSidebar(true); }
        });

        // al pasar a escritorio el panel vuelve a ser fijo: hay que soltar
        // el bloqueo de scroll que el estado abierto dejó en el body
        var escritorio = window.matchMedia('(min-width: 992px)');
        var alCambiarAncho = function (consulta) {
            if (consulta.matches) { cerrarSidebar(false); }
        };

        if (escritorio.addEventListener) {
            escritorio.addEventListener('change', alCambiarAncho);
        } else {
            escritorio.addListener(alCambiarAncho);
        }
    }

    var botonBusqueda = document.querySelector('[data-busqueda-toggle]');
    var campoBusqueda = document.getElementById('busqueda-global');

    if (botonBusqueda && campoBusqueda) {
        botonBusqueda.addEventListener('click', function () {
            var abierta = cuerpo.classList.toggle('busqueda-abierta');
            botonBusqueda.setAttribute('aria-expanded', abierta ? 'true' : 'false');
            if (abierta) { campoBusqueda.focus(); }
        });
    }
})();
