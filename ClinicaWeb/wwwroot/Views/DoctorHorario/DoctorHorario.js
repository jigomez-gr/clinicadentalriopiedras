/* DoctorHorario (sin timepicker) - Bootstrap 5 + DataTables + Select2 */
(function () {
    "use strict";

    // ========= HELP VIDEO (simple) =========
    window.openHelpVideo = function (fileName) {
        const w = 900, h = 520;
        const left = Math.max(0, (screen.width - w) / 2);
        const top = Math.max(0, (screen.height - h) / 2);

        const url = "/helps/" + encodeURIComponent(fileName);

        window.open(
            url,
            "helpVideo",
            `width=${w},height=${h},left=${left},top=${top},resizable=yes,scrollbars=no,toolbar=no,menubar=no,location=no,status=no`
        );
    };
})();

let tablaData;
let idEditar = 0;
const controlador = "DoctorHorario";
const modal = "mdData";

document.addEventListener("DOMContentLoaded", function () {
    inicializarTabla();
    inicializarTooltips(document);
    inicializarSelectsModal();
    cargarDoctores();

    // Abrir modal "Nuevo Horario"
    const btnNuevo = document.getElementById("btnNuevo");
    if (btnNuevo) {
        btnNuevo.addEventListener("click", function () {
            // Si la ayuda está abierta, la cerramos para evitar que "tape" el modal / selects
            const ayudaEl = document.getElementById("mdAyuda");
            if (ayudaEl && window.bootstrap) {
                const instAyuda = window.bootstrap.Modal.getInstance(ayudaEl);
                if (instAyuda) instAyuda.hide();
            }

            limpiarModal();
            idEditar = 0;

            // Abrir modal
            if (window.jQuery && window.jQuery.fn.modal) {
                window.jQuery(`#${modal}`).modal("show");
            } else if (window.bootstrap) {
                window.bootstrap.Modal.getOrCreateInstance(document.getElementById(modal)).show();
            }
        });
    }

    // Guardar
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", guardarHorario);
    }

    // Eliminar (datatable)
    $("#tbData tbody").on("click", ".btn-eliminar", function () {
        const fila = $(this).closest("tr");
        const data = tablaData.row(fila).data();
        if (!data) return;

        Swal.fire({
            text: "¿Desea eliminar el horario?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Sí, continuar",
            cancelButtonText: "No, volver"
        }).then((result) => {
            if (!result.isConfirmed) return;

            fetch(`/${controlador}/Eliminar?Id=${data.idDoctorHorario}`, {
                method: "DELETE",
                headers: { "Content-Type": "application/json;charset=utf-8" }
            })
                .then(r => r.ok ? r.json() : Promise.reject(r))
                .then(rj => {
                    if (rj.data === "") {
                        Swal.fire({ title: "Listo!", text: "Horario eliminado.", icon: "success" });
                        tablaData.ajax.reload();
                    } else {
                        Swal.fire({ title: "Aviso", text: rj.data || "No se pudo eliminar.", icon: "warning" });
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire({ title: "Error", text: "No se pudo eliminar.", icon: "error" });
                });
        });
    });
});

function inicializarTabla() {
    tablaData = $("#tbData").DataTable({
        responsive: true,
        autoWidth: false,
        scrollX: false,
        ajax: {
            url: `/${controlador}/Lista`,
            type: "GET",
            datatype: "json"
        },
        columns: [
            {
                data: "doctor",
                render: function (data) {
                    if (!data) return "";
                    const nom = data.nombres || data.nombre || "";
                    const ape = data.apellidos || data.apellido || "";
                    const full = data.nombreCompleto || `${nom} ${ape}`.trim();
                    return full;
                }
            },
            { data: "numeroMes" },
            {
                data: null,
                render: function (data, type, row) {
                    const hi = row.horaInicioAM || "";
                    const hf = row.horaFinAM || "";
                    return (hi && hf) ? `${hi} - ${hf}` : "";
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    const hi = row.horaInicioPM || "";
                    const hf = row.horaFinPM || "";
                    return (hi && hf) ? `${hi} - ${hf}` : "";
                }
            },
            {
                data: null,
                orderable: false,
                width: "120px",
                render: function () {
                    return `
                        <button type="button"
                                class="btn btn-sm btn-danger btn-eliminar"
                                data-bs-toggle="tooltip" data-bs-placement="top" data-bs-custom-class="tooltip-dh"
                                title="Eliminar: permite eliminar el horario de la base de datos.">
                            Eliminar
                        </button>`;
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
        drawCallback: function () { inicializarTooltips(document.getElementById("tbData")); },
        initComplete: function () { inicializarTooltips(document.getElementById("tbData")); }
    });
}

function inicializarSelectsModal() {
    // Mes: lo rellena la vista, pero si viniera vacío, lo rellenamos también aquí.
    const cboMes = document.getElementById("cboMes");
    if (cboMes && cboMes.options.length <= 1) {
        const meses = [
            { v: "", t: "-- Seleccione mes --" },
            { v: "1", t: "Enero" },
            { v: "2", t: "Febrero" },
            { v: "3", t: "Marzo" },
            { v: "4", t: "Abril" },
            { v: "5", t: "Mayo" },
            { v: "6", t: "Junio" },
            { v: "7", t: "Julio" },
            { v: "8", t: "Agosto" },
            { v: "9", t: "Septiembre" },
            { v: "10", t: "Octubre" },
            { v: "11", t: "Noviembre" },
            { v: "12", t: "Diciembre" }
        ];
        cboMes.innerHTML = "";
        meses.forEach(m => {
            const opt = document.createElement("option");
            opt.value = m.v;
            opt.textContent = m.t;
            cboMes.appendChild(opt);
        });
    }

    // Select2 dentro del modal
    if (window.jQuery && window.jQuery.fn.select2) {
        const $modal = $("#mdData");
        $("#cboDoctor").select2({
            theme: "bootstrap-5",
            dropdownParent: $modal,
            placeholder: "-- Seleccione doctor --",
            width: "100%"
        });

        $("#cboMes").select2({
            theme: "bootstrap-5",
            dropdownParent: $modal,
            placeholder: "-- Seleccione mes --",
            width: "100%"
        });
    }
}

function cargarDoctores() {
    fetch("/Doctor/Lista", { method: "GET" })
        .then(r => r.ok ? r.json() : Promise.reject(r))
        .then(rj => {
            const lista = rj.data || [];
            const cbo = document.getElementById("cboDoctor");
            if (!cbo) return;

            cbo.innerHTML = `<option value="">-- Seleccione doctor --</option>`;
            lista.forEach(d => {
                const id = d.idDoctor ?? d.IdDoctor;
                const nom = d.nombres || d.Nombres || "";
                const ape = d.apellidos || d.Apellidos || "";
                const full = d.nombreCompleto || d.NombreCompleto || `${nom} ${ape}`.trim();

                const opt = document.createElement("option");
                opt.value = id;
                opt.textContent = full;
                cbo.appendChild(opt);
            });

            // refrescar select2 si aplica
            if (window.jQuery && window.jQuery.fn.select2) {
                $("#cboDoctor").trigger("change.select2");
            }
        })
        .catch(err => {
            console.error(err);
            Swal.fire({
                title: "Aviso",
                text: "No se pudo cargar la lista de doctores.",
                icon: "warning"
            });
        });
}

async function guardarHorario() {
    const idDoctor = (document.getElementById("cboDoctor")?.value || "").trim();
    const mes = (document.getElementById("cboMes")?.value || "").trim();
    const hiAM = (document.getElementById("txtHoraInicioAM")?.value || "").trim();
    const hfAM = (document.getElementById("txtHoraFinAM")?.value || "").trim();
    const hiPM = (document.getElementById("txtHoraInicioPM")?.value || "").trim();
    const hfPM = (document.getElementById("txtHoraFinPM")?.value || "").trim();
    const fechasRaw = (document.getElementById("txtFechas")?.value || "").trim();

    if (!idDoctor) return aviso("Seleccione un doctor.");
    if (!mes) return aviso("Seleccione el mes de atención.");
    if (!hiAM || !hfAM) return aviso("Indique el horario de mañana (inicio y fin).");
    if (!hiPM || !hfPM) return aviso("Indique el horario de tarde (inicio y fin).");
    if (!fechasRaw) return aviso("Indique las fechas de atención (separadas por coma o salto de línea).");

    // Normalizar fechas -> CSV dd/MM/yyyy
    const fechasTokens = fechasRaw
        .split(/[\n,;]+/g)
        .map(s => s.trim())
        .filter(Boolean);

    const fechasNorm = [];
    for (const tok of fechasTokens) {
        const norm = normalizarFecha(tok);
        if (!norm) return aviso(`Fecha no válida: "${tok}". Use dd/MM/yyyy o yyyy-MM-dd.`);
        fechasNorm.push(norm);
    }

    // Comprobar que todas las fechas están en el mes seleccionado
    const mesInt = parseInt(mes, 10);
    for (const f of fechasNorm) {
        const parts = f.split("/");
        const m = parseInt(parts[1], 10);
        if (m !== mesInt) return aviso("Todas las fechas deben estar dentro del mes seleccionado.");
    }

    const payload = {
        Doctor: { IdDoctor: parseInt(idDoctor, 10) },
        NumeroMes: mesInt,
        HoraInicioAM: hiAM.substring(0, 5),
        HoraFinAM: hfAM.substring(0, 5),
        HoraInicioPM: hiPM.substring(0, 5),
        HoraFinPM: hfPM.substring(0, 5),
        DoctorHorarioDetalle: { Fecha: fechasNorm.join(",") }
    };

    $("#mdData").LoadingOverlay("show");

    fetch(`/${controlador}/Guardar`, {
        method: "POST",
        headers: { "Content-Type": "application/json;charset=utf-8" },
        body: JSON.stringify(payload)
    })
        .then(r => r.ok ? r.json() : Promise.reject(r))
        .then(rj => {
            if (rj.data === "") {
                Swal.fire({ title: "Listo!", text: "Horario registrado correctamente.", icon: "success" });
                $(`#${modal}`).modal("hide");
                tablaData.ajax.reload();
            } else {
                Swal.fire({ title: "Aviso", text: rj.data || "No se pudo registrar.", icon: "warning" });
            }
        })
        .catch(err => {
            console.error(err);
            Swal.fire({ title: "Error", text: "No se pudo registrar el horario.", icon: "error" });
        })
        .finally(() => {
            $("#mdData").LoadingOverlay("hide");
        });
}

function limpiarModal() {
    // selects
    $("#cboDoctor").val("").trigger("change");
    $("#cboMes").val("").trigger("change");

    // horas
    const set = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };
    set("txtHoraInicioAM", "");
    set("txtHoraFinAM", "");
    set("txtHoraInicioPM", "");
    set("txtHoraFinPM", "");
    set("txtFechas", "");
}

function aviso(msg) {
    Swal.fire({ title: "Aviso", text: msg, icon: "warning" });
}

function normalizarFecha(s) {
    // dd/MM/yyyy o d/M/yyyy
    let m = s.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
    if (m) {
        const dd = m[1].padStart(2, "0");
        const mm = m[2].padStart(2, "0");
        const yyyy = m[3];
        return `${dd}/${mm}/${yyyy}`;
    }

    // yyyy-MM-dd
    m = s.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/);
    if (m) {
        const yyyy = m[1];
        const mm = m[2].padStart(2, "0");
        const dd = m[3].padStart(2, "0");
        return `${dd}/${mm}/${yyyy}`;
    }

    return null;
}

function inicializarTooltips(scope) {
    if (!window.bootstrap) return;
    const root = scope || document;
    root.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((el) => {
        // Evita duplicados
        if (el._tooltipInstance) return;
        el._tooltipInstance = new bootstrap.Tooltip(el);
    });
}
