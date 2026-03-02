/* Especialidad.js completo */
let tablaData;
let idEditar = 0;
const controlador = "Especialidad";
const modal = "mdData";
const preguntaEliminar = "Desea eliminar la especialidad";
const confirmaEliminar = "La especialidad fue eliminada.";
const confirmaRegistro = "Especialidad registrada!";

document.addEventListener("DOMContentLoaded", function (event) {

    /* cambio datatable comzo */
    tablaData = $('#tbData').DataTable({
        // Se desactiva responsive:true para evitar conflictos con scrollX y la columna sticky móvil
        responsive: false,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/Lista`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                // Nueva columna de Acciones (primera posición) para coincidir con el <thead> del Index
                title: "Acciones",
                "data": "idEspecialidad",
                width: "100px",
                orderable: false,
                render: function (data, type, row) {
                    return `<div class="acciones-container">
                                <button type="button" class="btn btn-primary btn-circle btn-editar" title="Editar">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-danger btn-circle btn-eliminar" title="Eliminar">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>`;
                }
            },
            { title: "Nombre", "data": "nombre" },
            { title: "Fecha Creacion", "data": "fechaCreacion" }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
        // Forzar ajuste de columnas al redibujar (importante para scrollX)
        drawCallback: function () {
            this.api().columns.adjust();
        }
    });
    /* cambio datatable fin */

});


$("#tbData tbody").on("click", ".btn-editar", function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();

    idEditar = data.idEspecialidad;
    $("#txtNombre").val(data.nombre);
    $(`#${modal}`).modal('show');
})


$("#btnNuevo").on("click", function () {
    idEditar = 0;
    $("#txtNombre").val("")
    $(`#${modal}`).modal('show');
})

$("#tbData tbody").on("click", ".btn-eliminar", function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();


    Swal.fire({
        text: `${preguntaEliminar} ${data.nombre}?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, continuar",
        cancelButtonText: "No, volver"
    }).then((result) => {
        if (result.isConfirmed) {

            fetch(`/${controlador}/Eliminar?Id=${data.idEspecialidad}`, {
                method: "DELETE",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {
                if (responseJson.data == 1) {
                    Swal.fire({
                        title: "Eliminado!",
                        text: confirmaEliminar,
                        icon: "success"
                    });
                    tablaData.ajax.reload();
                } else {
                    Swal.fire({
                        title: "Error!",
                        text: "No se pudo eliminar.",
                        icon: "warning"
                    });
                }
            }).catch((error) => {
                Swal.fire({
                    title: "Error!",
                    text: "No se pudo eliminar.",
                    icon: "warning"
                });
            })
        }
    });
})



$("#btnGuardar").on("click", function () {
    if ($("#txtNombre").val().trim() == "") {
        Swal.fire({
            title: "Error!",
            text: "Debe ingresar el nombre.",
            icon: "warning"
        });
        return
    }

    let objeto = {
        IdEspecialidad: idEditar,
        Nombre: $("#txtNombre").val().trim()
    }

    if (idEditar != 0) {

        fetch(`/${controlador}/Editar`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(objeto)
        }).then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data == "") {
                idEditar = 0;
                Swal.fire({
                    text: "Se guardaron los cambios!",
                    icon: "success"
                });
                $(`#${modal}`).modal('hide');
                tablaData.ajax.reload();
            } else {
                Swal.fire({
                    title: "Error!",
                    text: responseJson.data,
                    icon: "warning"
                });
            }
        }).catch((error) => {
            Swal.fire({
                title: "Error!",
                text: "No se pudo editar.",
                icon: "warning"
            });
        })
    } else {
        fetch(`/${controlador}/Guardar`, {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(objeto)
        }).then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data == "") {
                Swal.fire({
                    text: confirmaRegistro,
                    icon: "success"
                });
                $(`#${modal}`).modal('hide');
                tablaData.ajax.reload();
            } else {
                Swal.fire({
                    title: "Error!",
                    text: responseJson.data,
                    icon: "warning"
                });
            }
        }).catch((error) => {
            Swal.fire({
                title: "Error!",
                text: "No se pudo registrar.",
                icon: "warning"
            });
        })
    }
});