// Usuario.js (guardado contra doble ejecución / hot reload)
if (window.__usuariosJsInit) {
    console.log("Usuario.js ya inicializado, evitando doble ejecución");
} else {
    window.__usuariosJsInit = true;

    let tablaData;
    let idEditar = 0;
    const controlador = "Usuario";
    const modal = "mdData";
    const preguntaEliminar = "Desea eliminar al usuario";
    const confirmaEliminar = "El usuario fue eliminado.";
    const confirmaRegistro = "Usuario registrado!";

    // ===== Helpers Bootstrap Modal (BS5/BS4) =====
    function mostrarModal(idModal) {
        const el = document.getElementById(idModal);
        if (!el) return;

        // Bootstrap 5 (sin jQuery modal)
        if (window.bootstrap?.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(el).show();
            return;
        }

        // Bootstrap 4 (con jQuery modal)
        if (window.jQuery) {
            $('#' + idModal).modal('show');
            return;
        }

        console.error("No hay Bootstrap Modal ni jQuery cargados.");
    }

    function ocultarModal(idModal) {
        const el = document.getElementById(idModal);
        if (!el) return;

        if (window.bootstrap?.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(el).hide();
            return;
        }

        if (window.jQuery) {
            $('#' + idModal).modal('hide');
            return;
        }

        console.error("No hay Bootstrap Modal ni jQuery cargados.");
    }

    // Devuelve siempre la fila "real" aunque la tabla esté en modo child (responsive)
    function obtenerDataFilaDesdeBoton(btn) {
        const $tr = $(btn).closest('tr');
        const $filaReal = $tr.hasClass('child') ? $tr.prev() : $tr;
        return tablaData.row($filaReal).data();
    }

    $(function () {
        const tabla = $('#tbData');

        // Evita reinicialización si el JS se ejecuta dos veces
        if ($.fn.dataTable.isDataTable(tabla)) {
            tabla.DataTable().clear().destroy();
        }

        tablaData = tabla.DataTable({
            responsive: true,
            scrollX: true,
            ajax: {
                url: `/${controlador}/Lista`,
                type: "GET",
                datatype: "json"
            },
            columns: [
                { title: "Nro Documento", data: "numeroDocumentoIdentidad", width: "150px" },
                { title: "Nombres", data: "nombre" },
                { title: "Apellidos", data: "apellido" },
                { title: "Correo", data: "correo" },
                {
                    title: "Movil",
                    data: function (row) {
                        return row.movil ?? row.Movil ?? row.telefono ?? row.Telefono ?? "";
                    }
                },
                {
                    title: "Rol",
                    data: function (row) {
                        const rol = row.rolUsuario ?? row.RolUsuario;
                        if (!rol) return "";
                        return rol.nombre ?? rol.Nombre ?? "";
                    }
                },
                { title: "Fecha Creación", data: "fechaCreacion" },
                {
                    title: "Acciones",
                    data: null,
                    width: "130px",
                    orderable: false,
                    searchable: false,
                    render: function () {
                        return `
                            <div class="btn-group dropstart">
                                <button type="button" class="btn btn-secondary btn-sm dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                                    Acción
                                </button>
                                <ul class="dropdown-menu">
                                    <li><button type="button" class="dropdown-item btn-editar">Editar</button></li>
                                    <li><button type="button" class="dropdown-item btn-eliminar">Eliminar</button></li>
                                </ul>
                            </div>`;
                    }
                }
            ],
            language: {
                url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
            }
        });

        // --- Filtro por rol (columna 5) ---
        $('#cboFiltroRol').off('change').on('change', function () {
            const valor = $(this).val();
            if (!valor) {
                tablaData.column(5).search('').draw();
            } else {
                tablaData.column(5).search('^' + valor + '$', true, false).draw();
            }
        });

        // --- EDITAR ---
        $("#tbData tbody").off("click", ".btn-editar").on("click", ".btn-editar", function () {
            const data = obtenerDataFilaDesdeBoton(this);
            if (!data) return;

            idEditar = data.idUsuario ?? data.IdUsuario ?? 0;

            $("#txtNroDocumento").val(data.numeroDocumentoIdentidad ?? data.NumeroDocumentoIdentidad ?? "");
            $("#txtNombres").val(data.nombre ?? data.Nombre ?? "");
            $("#txtApellidos").val(data.apellido ?? data.Apellido ?? "");
            $("#txtCorreo").val(data.correo ?? data.Correo ?? "");
            $("#txtClave").val(data.clave ?? data.Clave ?? "");
            $("#txtMovil").val(data.movil ?? data.Movil ?? data.telefono ?? data.Telefono ?? "");

            const rol = data.rolUsuario ?? data.RolUsuario;
            const idRol = rol?.idRolUsuario ?? rol?.IdRolUsuario ?? 0;
            $("#cboRol").val(idRol ? idRol.toString() : "");

            mostrarModal(modal);
        });

        // --- NUEVO ---
        $("#btnNuevo").off("click").on("click", function () {
            idEditar = 0;

            $("#txtNroDocumento").val("");
            $("#txtNombres").val("");
            $("#txtApellidos").val("");
            $("#txtCorreo").val("");
            $("#txtClave").val("");
            $("#txtMovil").val("");

            // Por defecto: Paciente
            $("#cboRol").val("3");

            mostrarModal(modal);
        });

        // --- ELIMINAR ---
        $("#tbData tbody").off("click", ".btn-eliminar").on("click", ".btn-eliminar", function () {
            const data = obtenerDataFilaDesdeBoton(this);
            if (!data) return;

            Swal.fire({
                text: `${preguntaEliminar} ${(data.nombre ?? data.Nombre ?? "")} ${(data.apellido ?? data.Apellido ?? "")}?`,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "Si, continuar",
                cancelButtonText: "No, volver"
            }).then((result) => {
                if (!result.isConfirmed) return;

                const id = data.idUsuario ?? data.IdUsuario;

                fetch(`/${controlador}/Eliminar?Id=${id}`, {
                    method: "DELETE",
                    headers: { 'Content-Type': 'application/json;charset=utf-8' }
                })
                    .then(response => response.ok ? response.json() : Promise.reject(response))
                    .then(responseJson => {
                        if (responseJson.data == 1) {
                            Swal.fire({ title: "Eliminado!", text: confirmaEliminar, icon: "success" });
                            tablaData.ajax.reload();
                        } else {
                            Swal.fire({ title: "Error!", text: "No se pudo eliminar.", icon: "warning" });
                        }
                    })
                    .catch(() => {
                        Swal.fire({ title: "Error!", text: "No se pudo eliminar.", icon: "warning" });
                    });
            });
        });

        // --- GUARDAR (alta / edición) ---
        $("#btnGuardar").off("click").on("click", function () {

            if ($("#txtNroDocumento").val().trim() == "" ||
                $("#txtNombres").val().trim() == "" ||
                $("#txtApellidos").val().trim() == "" ||
                $("#txtCorreo").val().trim() == "" ||
                $("#txtClave").val().trim() == "" ||
                $("#txtMovil").val().trim() == "") {

                Swal.fire({ title: "Error!", text: "Falta completar datos.", icon: "warning" });
                return;
            }

            const idRolSeleccionado = parseInt($("#cboRol").val() || "0", 10);
            if (!idRolSeleccionado) {
                Swal.fire({ title: "Error!", text: "Debe seleccionar un rol.", icon: "warning" });
                return;
            }

            const objeto = {
                IdUsuario: idEditar,
                NumeroDocumentoIdentidad: $("#txtNroDocumento").val().trim(),
                Nombre: $("#txtNombres").val().trim(),
                Apellido: $("#txtApellidos").val().trim(),
                Correo: $("#txtCorreo").val().trim(),
                Clave: $("#txtClave").val().trim(),
                Movil: $("#txtMovil").val().trim(),
                RolUsuario: { IdRolUsuario: idRolSeleccionado }
            };

            if (idEditar != 0) {
                // EDITAR
                fetch(`/${controlador}/Editar`, {
                    method: "PUT",
                    headers: { 'Content-Type': 'application/json;charset=utf-8' },
                    body: JSON.stringify(objeto)
                })
                    .then(response => response.ok ? response.json() : Promise.reject(response))
                    .then(responseJson => {
                        if (responseJson.data == "") {
                            idEditar = 0;
                            Swal.fire({ text: "Se guardaron los cambios!", icon: "success" });
                            ocultarModal(modal);
                            tablaData.ajax.reload();
                        } else {
                            Swal.fire({ title: "Error!", text: responseJson.data, icon: "warning" });
                        }
                    })
                    .catch(() => Swal.fire({ title: "Error!", text: "No se pudo editar.", icon: "warning" }));

            } else {
                // NUEVO
                fetch(`/${controlador}/Guardar`, {
                    method: "POST",
                    headers: { 'Content-Type': 'application/json;charset=utf-8' },
                    body: JSON.stringify(objeto)
                })
                    .then(response => response.ok ? response.json() : Promise.reject(response))
                    .then(responseJson => {
                        if (responseJson.data == "") {
                            Swal.fire({ text: confirmaRegistro, icon: "success" });
                            ocultarModal(modal);
                            tablaData.ajax.reload();
                        } else {
                            Swal.fire({ title: "Error!", text: responseJson.data, icon: "warning" });
                        }
                    })
                    .catch(() => Swal.fire({ title: "Error!", text: "No se pudo registrar.", icon: "warning" }));
            }
        });
    });
}
