$("#date-from").val(bagValueFrom);
$("#date-to").val(bagValueTo);

$("#apply-period").click(function () {
    let valueFrom = $("#date-from").val();
    let valueTo = $("#date-to").val();
    let params = new URLSearchParams(location.search);
    if (!valueFrom || !valueTo) {
        if (!valueFrom && !valueTo) {
            location.href = location.protocol + "//" + location.host + location.pathname + (params.get("locationId") ? `?locationId=${params.get("locationId")}` : "");
        }
        else {
            return;
        }
    }

    if (valueFrom > valueTo) {
        alert("Некорректный ввод");
        return;
    }

    location.href = location.protocol + "//" + location.host + location.pathname + `?from=${valueFrom}&to=${valueTo}`
        + (params.get("locationId") ? `&locationId=${params.get("locationId")}` : "");
});

$("#download-data").click(function () {
    let valueFrom = $("#date-from").val();
    let valueTo = $("#date-to").val();
    let params = new URLSearchParams(location.search);
    if (!valueFrom || !valueTo) {
        if (!valueFrom && !valueTo) {
            location.href = location.protocol + "//" + location.host + location.pathname.replace(/history/i, "Download")
                + (params.get("locationId") ? `?locationId=${params.get("locationId")}` : "");
        }
        else {
            return;
        }
    }

    if (valueFrom > valueTo) {
        alert("Некорректный ввод");
        return;
    }

    location.href = location.protocol + "//" + location.host + location.pathname.replace(/history/i, "Download") + `?from=${valueFrom}&to=${valueTo}`
        + (params.get("locationId") ? `&locationId=${params.get("locationId")}` : "");
});

let context = document.getElementById("chart").getContext("2d");
let colors = ["#ff0000", "#00ff00", "#0000ff", "#000000", "#ffff00", "#ff00ff", "#00ffff", "#a0a0a0", "ff8000", "ff0080", "80ff00", "00ff80", "8000ff", "0080ff", "808080"];
let data;
let options;

if (modelJson.length > 0) {
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
                backgroundColor: colors[i],
                borderColor: colors[i],
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
        responsive: true
    };
}
else {
    data = { labels: [], datasets: [] };
    options = { responsive: true };
}

let chart = new Chart(context, {
    type: 'line',
    data: data,
    options: options
});