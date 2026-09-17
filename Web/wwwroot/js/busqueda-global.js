(function () {
    'use strict';

    // Menos de dos letras devuelve medio sistema: no vale la pena el viaje.
    var CARACTERES_MINIMOS = 2;
    // Suficiente para que una palabra corta se escriba entera sin disparar
    // una consulta por tecla, y poco para que no se sienta lento.
    var ESPERA_MS = 300;

    var campo = document.getElementById('busqueda-global');
    var panel = document.getElementById('busqueda-panel');

    // El login se renderiza sin el shell: ahí no hay buscador que enganchar.
    if (!campo || !panel) { return; }

    var cuerpo = document.body;
    var url = campo.getAttribute('data-busqueda-url');
    var lista = panel.querySelector('[data-busqueda-cuerpo]');
    var titulo = panel.querySelector('[data-busqueda-titulo]');
    var contador = panel.querySelector('[data-busqueda-contador]');
    var aviso = document.querySelector('[data-busqueda-estado]');

    var iconos = {};
    Array.prototype.forEach.call(
        document.querySelectorAll('[data-busqueda-icono]'),
        function (plantilla) {
            iconos[plantilla.getAttribute('data-busqueda-icono')] = plantilla.innerHTML;
        });

    var temporizador = null;
    var consulta = 0;      // descarta respuestas viejas que lleguen fuera de orden
    var filas = [];        // los resultados en el mismo orden en que se ven
    var seleccion = -1;
    var textoPintado = '';

    function escapar(texto) {
        return String(texto == null ? '' : texto)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // Marca las coincidencias sin usar expresiones regulares: el término lo escribe
    // el usuario y podría traer caracteres que rompan el patrón.
    function resaltar(texto, termino) {
        var original = String(texto == null ? '' : texto);
        if (!termino) { return escapar(original); }

        var base = original.toLowerCase();
        var buscado = termino.toLowerCase();
        var html = '';
        var desde = 0;
        var pos = base.indexOf(buscado);

        while (pos !== -1) {
            html += escapar(original.slice(desde, pos));
            html += '<mark>' + escapar(original.slice(pos, pos + buscado.length)) + '</mark>';
            desde = pos + buscado.length;
            pos = base.indexOf(buscado, desde);
        }

        return html + escapar(original.slice(desde));
    }

    function pintarFila(item, clave, termino, indice) {
        var html = '<a class="busq-item" role="option" id="busq-item-' + indice + '"' +
                   ' href="' + escapar(item.url) + '">';

        html += '<span class="busq-tile ' + clave + '">' + (iconos[clave] || '') + '</span>';

        html += '<span class="busq-texto">';
        html += '<span class="busq-name">' + resaltar(item.nombre, termino) + '</span>';
        html += '<span class="busq-meta">' + resaltar(item.meta, termino);
        if (item.estado) {
            html += ' <span class="estado-pill ' + escapar(item.estadoClase) + '">' +
                    escapar(item.estado) + '</span>';
        }
        html += '</span></span>';

        if (item.valor) {
            html += '<span class="busq-right">';
            html += '<span class="busq-val">' + escapar(item.valor) + '</span>';
            if (item.sub) { html += '<span class="busq-sub">' + escapar(item.sub) + '</span>'; }
            html += '</span>';
        }

        return html + '</a>';
    }

    function pintar(datos) {
        var termino = datos.texto || '';
        var html = '';
        var indice = 0;

        titulo.innerHTML = 'Resultados para <b>«' + escapar(termino) + '»</b>';
        contador.textContent = datos.total === 1 ? '1 coincidencia' : datos.total + ' coincidencias';

        if (!datos.total) {
            // Con todo en cero, cuatro grupos vacíos son ruido: un solo mensaje.
            html = '<p class="busq-nada">' +
                   (datos.mensaje
                       ? escapar(datos.mensaje)
                       : 'No encontramos nada para «' + escapar(termino) + '».') +
                   '</p>';
        } else {
            datos.grupos.forEach(function (grupo) {
                html += '<div class="busq-group">';
                html += '<p class="busq-label">' + escapar(grupo.titulo) +
                        ' <span class="mono">' + grupo.total + '</span></p>';

                if (!grupo.items.length) {
                    html += '<p class="busq-empty">' + (iconos.vacio || '') +
                            'Nada en ' + escapar(grupo.titulo.toLowerCase()) +
                            ' para «' + escapar(termino) + '».</p>';
                } else {
                    grupo.items.forEach(function (item) {
                        html += pintarFila(item, grupo.clave, termino, indice);
                        indice++;
                    });

                    if (grupo.hayMas) {
                        html += '<p class="busq-mas">Hay ' + grupo.total + ' en ' +
                                escapar(grupo.titulo.toLowerCase()) +
                                '. Refiná la búsqueda para verlos todos.</p>';
                    }
                }

                html += '</div>';
            });
        }

        lista.innerHTML = html;
        lista.scrollTop = 0;
        filas = Array.prototype.slice.call(lista.querySelectorAll('.busq-item'));
        textoPintado = termino;

        // La primera fila queda lista para Enter, como en el artboard.
        marcar(filas.length ? 0 : -1);
        abrir();

        if (aviso) {
            aviso.textContent = datos.total === 1
                ? '1 resultado para ' + termino
                : datos.total + ' resultados para ' + termino;
        }
    }

    function marcar(indice) {
        if (seleccion >= 0 && filas[seleccion]) {
            filas[seleccion].classList.remove('activa');
            filas[seleccion].removeAttribute('aria-selected');
        }

        seleccion = indice;

        if (seleccion >= 0 && filas[seleccion]) {
            var fila = filas[seleccion];
            fila.classList.add('activa');
            fila.setAttribute('aria-selected', 'true');
            campo.setAttribute('aria-activedescendant', fila.id);

            // Sin esto la selección se va fuera de la zona visible del panel.
            if (fila.scrollIntoView) { fila.scrollIntoView({ block: 'nearest' }); }
        } else {
            campo.removeAttribute('aria-activedescendant');
        }
    }

    function mover(paso) {
        if (!filas.length) { return; }

        var siguiente = seleccion + paso;
        if (siguiente < 0) { siguiente = filas.length - 1; }
        if (siguiente >= filas.length) { siguiente = 0; }

        marcar(siguiente);
    }

    function abrir() {
        cuerpo.classList.add('busqueda-resultados');
        campo.setAttribute('aria-expanded', 'true');
    }

    function cerrar() {
        cuerpo.classList.remove('busqueda-resultados');
        campo.setAttribute('aria-expanded', 'false');
        marcar(-1);
    }

    function consultar(termino) {
        var propia = ++consulta;

        fetch(url + '?texto=' + encodeURIComponent(termino), {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (respuesta) { return respuesta.json(); })
            .then(function (datos) {
                // Llegó tarde: ya hay una consulta más nueva en camino.
                if (propia !== consulta) { return; }

                // La sesión venció mientras se buscaba: el servidor dice adónde ir.
                if (datos.redireccion) { window.location.href = datos.redireccion; return; }

                pintar(datos);
            })
            .catch(function () {
                if (propia !== consulta) { return; }

                // La búsqueda es un accesorio del shell: si falla la red se avisa
                // en el panel y la página sigue funcionando igual.
                lista.innerHTML = '<p class="busq-nada">No se pudo buscar. Probá de nuevo.</p>';
                filas = [];
                marcar(-1);
                abrir();
            });
    }

    campo.addEventListener('input', function () {
        var termino = campo.value.trim();

        window.clearTimeout(temporizador);

        if (termino.length < CARACTERES_MINIMOS) {
            consulta++;      // anula cualquier respuesta en vuelo
            textoPintado = '';
            cerrar();
            return;
        }

        temporizador = window.setTimeout(function () { consultar(termino); }, ESPERA_MS);
    });

    campo.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape') { cerrar(); return; }

        if (!cuerpo.classList.contains('busqueda-resultados')) { return; }

        if (evento.key === 'ArrowDown') {
            evento.preventDefault();
            mover(1);
        } else if (evento.key === 'ArrowUp') {
            evento.preventDefault();
            mover(-1);
        } else if (evento.key === 'Enter') {
            if (seleccion >= 0 && filas[seleccion]) {
                evento.preventDefault();
                window.location.href = filas[seleccion].href;
            }
        }
    });

    // Volver al buscador no debería obligar a escribir de nuevo lo mismo.
    campo.addEventListener('focus', function () {
        if (filas.length && campo.value.trim() === textoPintado) { abrir(); }
    });

    // Mismo criterio que la campana de notificaciones: clic afuera cierra.
    document.addEventListener('click', function (evento) {
        if (!evento.target.closest('#topbar-search')) { cerrar(); }
    });

    document.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape') { cerrar(); }
    });
})();
