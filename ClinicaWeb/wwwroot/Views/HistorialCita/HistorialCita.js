// HistorialCita.js (solo lectura)
// Requiere: jQuery, DataTables, Bootstrap (modal), SweetAlert2 (opcional)

let tablaHistorial;
let filaActual = null; // objeto cita seleccionado

function safe(v) {
    return (v === undefined || v === null) ? "" : String(v);
}

function getEspecialidadNombre(cita) {
    return safe(cita?.especialidad?.nombre ?? cita?.Especialidad?.Nombre ?? cita?.nombreEspecialidad ?? cita?.NombreEspecialidad);
}

function getDoctorNombre(cita) {
    const nom = safe(cita?.doctor?.nombres ?? cita?.Doctor?.Nombres ?? cita?.nombres ?? cita?.Nombres);
    const ape = safe(cita?.doctor?.apellidos ?? cita?.Doctor?.Apellidos ?? cita?.apellidos ?? cita?.Apellidos);
    return (nom + " " + ape).trim();
}

function normHora(v) {
    const s = safe(v).trim();
    return s.length >= 5 ? s.substring(0, 5) : s;
}

function base64ToBlob(base64, contentType) {
    if (!base64) return null;
    // base64 puede venir con prefijo data:...;base64,
    const clean = base64.includes(",") ? base64.split(",")[1] : base64;
    const byteChars = atob(clean);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: contentType || "application/octet-stream" });
}

function downloadBlob(blob, filename) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => window.URL.revokeObjectURL(url), 1000);
}

function info(msg) {
    if (window.Swal) {
        Swal.fire("Información", msg, "info");
    } else {
        alert(msg);
    }
}

function abrirModalDetalle(cita) {
    filaActual = cita;

    const id = cita?.idCita ?? cita?.IdCita ?? 0;
    const fecha = safe(cita?.fechaCita ?? cita?.FechaCita);
    const hora = normHora(cita?.horaCita ?? cita?.HoraCita);

    const esp = getEspecialidadNombre(cita);
    const doc = getDoctorNombre(cita);

    $("#hdIdCita").val(id);
    $("#txtFechaCita").val(fecha);
    $("#txtHoraCita").val(hora);
    $("#txtEspecialidad").val(esp);
    $("#txtDoctor").val(doc);

    $("#txtOrigenCita").val(safe(cita?.origenCita ?? cita?.OrigenCita));
    $("#txtRazonCitaUsr").val(safe(cita?.razonCitaUsr ?? cita?.RazonCitaUsr));
    $("#txtIndicacionesDoctor").val(safe(cita?.indicaciones ?? cita?.Indicaciones));

    // habilita/inhabilita botones docs
    const docPac = cita?.documentoCitaUsr ?? cita?.DocumentoCitaUsr;
    const ctPac = cita?.contentType ?? cita?.ContentType;
    const docDoc = cita?.docIndicacionesDoctor ?? cita?.DocIndicacionesDoctor;
    const ctDoc = cita?.contentTypeDoctor ?? cita?.ContentTypeDoctor;

    $("#btnVerDocPaciente").prop("disabled", !docPac);
    $("#btnVerDocDoctor").prop("disabled", !docDoc);

    const modal = new bootstrap.Modal(document.getElementById("mdCita"));
    modal.show();
}

function descargarDocPaciente() {
    if (!filaActual) return;
    const id = filaActual?.idCita ?? filaActual?.IdCita ?? 0;
    const base64 = filaActual?.documentoCitaUsr ?? filaActual?.DocumentoCitaUsr;
    const ct = filaActual?.contentType ?? filaActual?.ContentType;

    if (!base64) {
        info("Esta cita no tiene documento del paciente.");
        return;
    }

    const blob = base64ToBlob(base64, ct);
    if (!blob) {
        info("No se pudo preparar el documento del paciente.");
        return;
    }

    // extensión simple por content-type
    const ext = (ct && ct.includes("pdf")) ? ".pdf" : (ct && ct.includes("png")) ? ".png" : (ct && ct.includes("jpeg")) ? ".jpg" : "";
    downloadBlob(blob, `documento_paciente_cita_${id}${ext}`);
}

function descargarDocDoctor() {
    if (!filaActual) return;
    const id = filaActual?.idCita ?? filaActual?.IdCita ?? 0;
    const base64 = filaActual?.docIndicacionesDoctor ?? filaActual?.DocIndicacionesDoctor;
    const ct = filaActual?.contentTypeDoctor ?? filaActual?.ContentTypeDoctor;

    if (!base64) {
        info("Esta cita no tiene documento del doctor.");
        return;
    }

    const blob = base64ToBlob(base64, ct);
    if (!blob) {
        info("No se pudo preparar el documento del doctor.");
        return;
    }

    const ext = (ct && ct.includes("pdf")) ? ".pdf" : (ct && ct.includes("png")) ? ".png" : (ct && ct.includes("jpeg")) ? ".jpg" : "";
    downloadBlob(blob, `documento_doctor_cita_${id}${ext}`);
}

$(document).ready(function () {
    // Tabla
    tablaHistorial = $('#tbCita').DataTable({
        responsive: true,
        autoWidth: false,
        scrollX: true,
        order: [[0, 'desc'], [1, 'desc']],
        ajax: {
            url: '/HistorialCitas/ListaHistorialCitas',
            type: 'GET',
            datatype: 'json'
        },
        columns: [
            { data: 'fechaCita', title: 'Fecha' },
            { data: 'horaCita', title: 'Hora', render: function (d) { return normHora(d); } },
            {
                data: 'especialidad',
                title: 'Especialidad',
                render: function (d, type, row) {
                    return getEspecialidadNombre(row);
                }
            },
            {
                data: 'doctor',
                title: 'Doctor',
                render: function (d, type, row) {
                    return getDoctorNombre(row);
                }
            },
            {
                data: 'origenCita',
                title: 'Origen',
                render: function (d, type, row) {
                    return safe(row?.origenCita ?? row?.OrigenCita);
                }
            },
            {
                data: null,
                title: 'Acciones',
                orderable: false,
                searchable: false,
                render: function () {
                    return `<button class="btn btn-sm btn-primary btn-detalle"><i class="fas fa-eye"></i> Ver</button>`;
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        }
    });

    // Click acción
    $('#tbCita tbody').on('click', '.btn-detalle', function () {
        const data = tablaHistorial.row($(this).parents('tr')).data();
        if (!data) return;
        abrirModalDetalle(data);
    });

    // Botones docs (modal)
    $('#btnVerDocPaciente').on('click', function () {
        descargarDocPaciente();
    });

    $('#btnVerDocDoctor').on('click', function () {
        descargarDocDoctor();
    });
});
