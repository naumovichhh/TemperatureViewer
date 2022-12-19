let context = document.getElementById("chart").getContext("2d");
let colors = ["#ff0000", "#00ff00", "#0000ff", "#000000", "#ffff00", "#ff00ff", "#00ffff",
    "#a0a0a0", "#ff8000", "#ff0080", "#80ff00", "#00ff80", "#8000ff", "#0080ff", "#808080",
    "#808000", "#800080", "#008080", "#800000", "#008000", "#000080", "#3f3f3f", "#ff8080",
    "#80ff80", "#8080ff", "#c08000", "#c00080", "#80c000", "#00c080", "#8000c0", "#0080c0"];
let data;
let options;
let chart;
let hidden = false;

$(function () {

    $("#date-from").val(bagValueFrom);
    $("#date-to").val(bagValueTo);

    $("#apply-period").click(function () {
        let valueFrom = $("#date-from").val();
        let valueTo = $("#date-to").val();
        let searchParams = [`from=${valueFrom ?? ""}`, `to=${valueTo ?? ""}`, `locationId=${locationId ?? ""}`];
        if (!valueFrom || !valueTo) {
            if (!valueFrom && !valueTo) {
                location.href = location.protocol + "//" + location.host + "/Home/History" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
            }
            else {
                alert("Некорректный период");
                return;
            }
        }

        let dateFrom = new Date(valueFrom);
        let dateTo = new Date(valueTo);
        if (dateFrom > dateTo || dateTo - dateFrom > 366 * 24 * 3600 * 1000) {
            alert("Некорректный период");
            return;
        }

        location.href = location.href = location.protocol + "//" + location.host + "/Home/History" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
    });

    $("#download-data").click(function () {
        let valueFrom = $("#date-from").val();
        let valueTo = $("#date-to").val();
        let searchParams = [`from=${valueFrom ?? ""}`, `to=${valueTo ?? ""}`, `locationId=${locationId ?? ""}`];
        if (!valueFrom || !valueTo) {
            if (!valueFrom && !valueTo) {
                location.href = location.protocol + "//" + location.host + "/Home/Download" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
            }
            else {
                alert("Некорректный период");
                return;
            }
        }


        let dateFrom = new Date(valueFrom);
        let dateTo = new Date(valueTo);
        if (dateFrom > dateTo || dateTo - dateFrom > 366 * 24 * 3600 * 1000) {
            alert("Некорректный период");
            return;
        }

        location.href = location.protocol + "//" + location.host + "/Home/Download" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
    });

    if (modelJson != null && modelJson.length > 0) {
        let numberOfMeasurements = modelJson.map(m => m.measurements.length).reduce((acc, next) => {
            if (next > acc)
                return next;
            else
                return acc;
        });
        data = {
            labels: measurementTimes.map(t => {
                return t.slice(0, 16).replace("T", " ");
            }),
            datasets: modelJson.map((e, i) => {
                let a = e.measurements.map(m => m?.value);
                return {
                    label: e.sensorName,
                    backgroundColor: colors[i % colors.length],
                    borderColor: colors[i % colors.length],
                    data: a,
                    spanGaps: true,
                    tension: 0.22
                };
            })
        };
        options = {
            scales: {
                y: {
                    suggestedMin: 10,
                    suggestedMax: 25
                }
            },
            responsive: true,
            animation: false
        };
    }
    else {
        data = { labels: [], datasets: [] };
        options = {
            responsive: true
        };
    }

    chart = new Chart(context, {
        type: 'line',
        data: data,
        options: options
    });

    $("#toggleAll").click(toggleAll);
});

function toggleAll() {
    if (hidden) {
        showAll();
        $("#toggleAll").html("Скрыть все");
        hidden = false;
    }
    else {
        hideAll();
        $("#toggleAll").html("Показать все");
        hidden = true;
    }
}

function showAll() {
    chart.data.datasets.forEach(function (ds, i) {
        chart.show(i);
    });

    chart.update();
}

function hideAll() {
    chart.data.datasets.forEach(function (ds, i) {
        chart.hide(i);
    });

    chart.update();
}