let tablaData;
let idEditar = 0;
const controlador = "Citas";
const modal = "mdData";
const preguntaEliminar = "Desea cancelar su cita?";
const confirmaEliminar = "Su cita fue cancela.";

// Para documentos (si el backend los devuelve en el JSON)
let docPacienteBase64 = null;
let docPacienteContentType = null;
let docDoctorBase64 = null;
let docDoctorContentType = null;

document.addEventListener("DOMContentLoaded", function (event) {

    console.log("cita.js Mis Citas cargado");

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasPendiente`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { title: "Fecha Cita", data: "fechaCita", width: "150px" },
            { title: "Hora Cita", data: "horaCita", width: "150px" },
            {
                title: "Especialidad",
                data: "especialidad",
                render: function (data, type, row) {
                    return data && data.nombre ? data.nombre : "";
                }
            },
            {
                title: "Doctor",
                data: "doctor",
                render: function (data, type, row) {
                    if (!data) return "";
                    const nom = data.nombres || "";
                    const ape = data.apellidos || "";
                    return `${nom} ${ape}`.trim();
                }
            },
            {
                // 🔹 AQUÍ metemos los dos botones
                title: "Acciones",
                data: "idCita",
                width: "220px",
                orderable: false,
                render: function (data, type, row) {

                    const btnDetalle = '<button type="button" class="btn btn-sm btn-outline-primary me-1 btn-detalle">Detalle</button>';
                    const btnCancelar = '<button type="button" class="btn btn-sm btn-outline-danger btn-cancelar">Cancelar</button>';

                    return btnDetalle + " " + btnCancelar;
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});


/// ================== BOTÓN CANCELAR (igual que antes) ==================

$("#tbData tbody").on("click", ".btn-cancelar", function () {
    let filaSeleccionada = $(this).closest('tr');
    let data = tablaData.row(filaSeleccionada).data();

    Swal.fire({
        text: `${preguntaEliminar}`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, continuar",
        cancelButtonText: "No, volver"
    }).then((result) => {
        if (result.isConfirmed) {

            fetch(`/${controlador}/Cancelar?Id=${data.idCita}`, {
                method: "DELETE",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {
                if (responseJson.data == "") {
                    Swal.fire({
                        title: "Listo!",
                        text: confirmaEliminar,
                        icon: "success"
                    });
                    tablaData.ajax.reload();
                } else {
                    Swal.fire({
                        title: "Error!",
                        text: "No se pudo cancelar.",
                        icon: "warning"
                    });
                }
            }).catch((error) => {
                console.error(error);
                Swal.fire({
                    title: "Error!",
                    text: "No se pudo cancelar.",
                    icon: "warning"
                });
            })
        }
    });
});


/// ================== BOTÓN DETALLE (abre el modal) ==================

$("#tbData tbody").on("click", ".btn-detalle", function () {
    let filaSeleccionada = $(this).closest('tr');
    let data = tablaData.row(filaSeleccionada).data();
    idEditar = data.idCita;

    console.log("Detalle cita (Mis Citas):", data);

    // Limpiamos estados previos
    limpiarModalPaciente();

    // Datos básicos
    $("#txtFechaCitaResumen").val(data.fechaCita || "");
    $("#txtHoraCitaResumen").val(data.horaCita || "");

    $("#txtOrigenCita").val(data.origenCita || "");

    const especialidadNombre =
        data.especialidad && data.especialidad.nombre
            ? data.especialidad.nombre
            : "";
    $("#txtEspecialidad").val(especialidadNombre);

    const doctorNombre =
        data.doctor
            ? `${data.doctor.nombres || ""} ${data.doctor.apellidos || ""}`.trim()
            : "";
    $("#txtDoctor").val(doctorNombre);

    // Motivo de la cita (paciente)
    $("#txtRazonCitaUsr").val(data.razonCitaUsr || "");

    // Indicaciones del doctor (solo lectura)
    $("#txtIndicacionesDoctor").val(data.indicaciones || "");

    // Documento del PACIENTE (si el backend lo envía)
    if (data.documentoCitaUsr) {
        docPacienteBase64 = data.documentoCitaUsr;
        docPacienteContentType = data.contentType || "application/octet-stream";

        $("#btnVerDocPaciente").prop("disabled", false);
        $("#lblHayDocPaciente").removeClass("d-none");
        $("#lblSinDocPaciente").addClass("d-none");
    } else {
        docPacienteBase64 = null;
        docPacienteContentType = null;

        $("#btnVerDocPaciente").prop("disabled", true);
        $("#lblHayDocPaciente").addClass("d-none");
        $("#lblSinDocPaciente").removeClass("d-none");
    }

    // Documento del DOCTOR (solo consulta)
    if (data.docIndicacionesDoctor) {
        docDoctorBase64 = data.docIndicacionesDoctor;
        docDoctorContentType = data.contentTypeDoctor || "application/octet-stream";

        $("#btnVerDocDoctor").prop("disabled", false);
        $("#lblHayDocDoctor").removeClass("d-none");
        $("#lblSinDocDoctor").addClass("d-none");
    } else {
        docDoctorBase64 = null;
        docDoctorContentType = null;

        $("#btnVerDocDoctor").prop("disabled", true);
        $("#lblHayDocDoctor").addClass("d-none");
        $("#lblSinDocDoctor").removeClass("d-none");
    }

    // Reseteamos selección de archivo del paciente
    $("#fileDocPaciente").val("");

    // Mostramos el modal
    $(`#${modal}`).modal('show');
});


/// ================== VER DOC PACIENTE ==================

$("#btnVerDocPaciente").on("click", function () {
    if (!docPacienteBase64) {
        Swal.fire({
            title: "Información",
            text: "No hay documento del paciente para esta cita.",
            icon: "info"
        });
        return;
    }

    try {
        const byteCharacters = atob(docPacienteBase64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: docPacienteContentType || "application/octet-stream" });
        const url = URL.createObjectURL(blob);
        window.open(url, "_blank");
    } catch (e) {
        console.error(e);
        Swal.fire({
            title: "Error",
            text: "No se pudo mostrar el documento.",
            icon: "error"
        });
    }
});


/// ================== VER DOC DOCTOR ==================

$("#btnVerDocDoctor").on("click", function () {
    if (!docDoctorBase64) {
        Swal.fire({
            title: "Información",
            text: "No hay documento del doctor para esta cita.",
            icon: "info"
        });
        return;
    }

    try {
        const byteCharacters = atob(docDoctorBase64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: docDoctorContentType || "application/octet-stream" });
        const url = URL.createObjectURL(blob);
        window.open(url, "_blank");
    } catch (e) {
        console.error(e);
        Swal.fire({
            title: "Error",
            text: "No se pudo mostrar el documento del doctor.",
            icon: "error"
        });
    }
});


/// ================== LIMPIAR MODAL ==================

function limpiarModalPaciente() {
    $("#txtFechaCitaResumen").val("");
    $("#txtHoraCitaResumen").val("");
    $("#txtOrigenCita").val("");
    $("#txtEspecialidad").val("");
    $("#txtDoctor").val("");
    $("#txtRazonCitaUsr").val("");
    $("#txtIndicacionesDoctor").val("");

    docPacienteBase64 = null;
    docPacienteContentType = null;
    $("#btnVerDocPaciente").prop("disabled", true);
    $("#lblHayDocPaciente").addClass("d-none");
    $("#lblSinDocPaciente").removeClass("d-none");

    docDoctorBase64 = null;
    docDoctorContentType = null;
    $("#btnVerDocDoctor").prop("disabled", true);
    $("#lblHayDocDoctor").addClass("d-none");
    $("#lblSinDocDoctor").removeClass("d-none");

    $("#fileDocPaciente").val("");
}
