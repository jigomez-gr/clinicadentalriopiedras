let tablaData;
let idCitaSeleccionada = 0;
const controlador = "Doctor";
const modal = "mdData";

// Documento del paciente
let docPacienteBase64 = null;
let docPacienteContentType = null;

// Documento del doctor
let docDoctorBase64 = null;
let docDoctorContentType = null;

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        processing: true,
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasAsignadas?IdEstadoCita=1`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { title: "Fecha Cita", "data": "fechaCita", width: "150px" },
            { title: "Hora Cita", "data": "horaCita", width: "150px" },
            {
                title: "Paciente", "data": "usuario", render: function (data, type, row) {
                    return `${data.nombre} ${data.apellido}`;
                }
            },
            {
                title: "Estado", "data": "estadoCita", render: function (data, type, row) {
                    return data.nombre == "Pendiente"
                        ? `<span class="badge bg-primary">${data.nombre}</span>`
                        : `<span class="badge bg-success">${data.nombre}</span>`;
                }
            },
            {
                title: "", "data": "idCita", width: "100px", render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-outline-warning me-1 btn-indicaciones">Indicaciones</button>`;
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});


$("#cboEstadoCita").on("change", function () {
    const nueva_url = `/${controlador}/ListaCitasAsignadas?IdEstadoCita=${$("#cboEstadoCita").val()}`;
    tablaData.ajax.url(nueva_url).load();
});

$("#tbData tbody").on("click", ".btn-indicaciones", function () {
    const filaSeleccionada = $(this).closest('tr');
    const data = tablaData.row(filaSeleccionada).data();
    console.log("Cita seleccionada:", data);

    idCitaSeleccionada = data.idCita;

    // 🔹 Cabecera del modal
    $("#txtPaciente").val(`${data.usuario.nombre} ${data.usuario.apellido}`);
    $("#txtFechaCitaResumen").val(data.fechaCita || "");
    $("#txtHoraCitaResumen").val(data.horaCita || "");

    // 🔹 Indicaciones actuales del doctor
    $("#txtIndicaciones").val(data.indicaciones || "");

    // 🔹 Motivo del paciente
    $("#txtRazonCitaUsr").val(data.razonCitaUsr || "");

    // 🔹 Documento del paciente (viene como base64 en documentoCitaUsr)
    if (data.documentoCitaUsr) {
        docPacienteBase64 = data.documentoCitaUsr;
        docPacienteContentType = data.contentType || "application/octet-stream";
        $("#btnVerDocPaciente").prop("disabled", false);
        $("#lblSinDocPaciente").hide();
    } else {
        docPacienteBase64 = null;
        docPacienteContentType = null;
        $("#btnVerDocPaciente").prop("disabled", true);
        $("#lblSinDocPaciente").show();
    }

    // 🔹 Documento del doctor (si ya existiera)
    if (data.docIndicacionesDoctor) {
        docDoctorBase64 = data.docIndicacionesDoctor;
        docDoctorContentType = data.contentTypeDoctor || "application/octet-stream";
        $("#btnVerDocDoctor").prop("disabled", false);
        $("#lblSinDocDoctor").hide();
    } else {
        docDoctorBase64 = null;
        docDoctorContentType = null;
        $("#btnVerDocDoctor").prop("disabled", true);
        $("#lblSinDocDoctor").show();
    }

    // Reset del file input
    $("#fileDocDoctor").val("");

    // Abrir modal
    $(`#${modal}`).modal('show');
    $("#txtIndicaciones").trigger("focus");

    const esAtendido = data.estadoCita.nombre == "Atendido";

    $("#txtIndicaciones").prop('disabled', esAtendido);
    $("#btnTerminarCita").prop('disabled', esAtendido);
    $("#fileDocDoctor").prop('disabled', esAtendido);

    if (esAtendido) {
        $('.alert-primary').hide();
    } else {
        $('.alert-primary').show();
    }
});

// 🔹 Ver documento del paciente
$("#btnVerDocPaciente").on("click", function () {
    if (!docPacienteBase64) {
        Swal.fire({
            title: "Información",
            text: "El paciente no adjuntó ningún documento.",
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
            text: "No se pudo mostrar el documento del paciente.",
            icon: "error"
        });
    }
});

// 🔹 Ver documento del doctor
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

// 🔹 Marcar cita como atendida + guardar documento del doctor
$("#btnTerminarCita").on("click", function () {

    if ($("#txtIndicaciones").val().trim() === "") {
        Swal.fire({
            title: "Importante",
            text: "Debe ingresar las indicaciones.",
            icon: "warning"
        });
        return;
    }

    const objeto = {
        IdCita: idCitaSeleccionada,
        EstadoCita: {
            IdEstadoCita: 2
        },
        Indicaciones: $("#txtIndicaciones").val().trim(),
    };

    // 1) Cambiar estado e indicaciones
    fetch(`/${controlador}/CambiarEstado`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(objeto)
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.data == "") {

            // 2) Si hay fichero del doctor, lo subimos
            const file = document.getElementById("fileDocDoctor").files[0];

            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const base64 = e.target.result.split(',')[1]; // quitamos el prefijo data:...

                    const objetoDoc = {
                        IdCita: idCitaSeleccionada,
                        DocIndicacionesDoctor: base64,
                        ContentTypeDoctor: file.type
                    };

                    fetch(`/${controlador}/GuardarDocumentoDoctor`, {
                        method: "POST",
                        headers: { 'Content-Type': 'application/json;charset=utf-8' },
                        body: JSON.stringify(objetoDoc)
                    }).then(r => {
                        return r.ok ? r.json() : Promise.reject(r);
                    }).then(rJson => {
                        if (rJson.data && rJson.data !== "") {
                            Swal.fire({
                                title: "Aviso",
                                text: "La cita se marcó como atendida, pero el documento del doctor devolvió: " + rJson.data,
                                icon: "warning"
                            });
                        } else {
                            Swal.fire({
                                title: "Listo!",
                                text: "La cita fue marcada como ATENDIDO y se guardó el documento del doctor.",
                                icon: "success"
                            });
                        }
                        $(`#${modal}`).modal('hide');
                        tablaData.ajax.reload();
                    }).catch((error) => {
                        console.error(error);
                        Swal.fire({
                            title: "Aviso",
                            text: "La cita se marcó como atendida, pero el documento del doctor no se pudo guardar.",
                            icon: "warning"
                        });
                        $(`#${modal}`).modal('hide');
                        tablaData.ajax.reload();
                    });
                };

                reader.readAsDataURL(file);
            } else {
                Swal.fire({
                    title: "Listo!",
                    text: "La cita fue marcada como ATENDIDO.",
                    icon: "success"
                });
                $(`#${modal}`).modal('hide');
                tablaData.ajax.reload();
            }

        } else {
            Swal.fire({
                title: "Error!",
                text: responseJson.data,
                icon: "warning"
            });
        }
    }).catch((error) => {
        console.error(error);
        Swal.fire({
            title: "Error!",
            text: "No se pudo registrar.",
            icon: "warning"
        });
    });

});
