let context = document.getElementById("chart");
let colors = ["#ff0000", "#00ff00", "#0000ff", "#000000", "#ffff00", "#ff00ff", "#00ffff", "#a0a0a0"];
let numberOfMeasurements = modelJson.map(m => m.measurements.length).reduce((acc, next) => {
    if (next > acc)
        return next;
    else
        return acc;
});
let data = {
    labels: modelJson[0].measurements.map((e) => {
        return e.time.slice(0, 16).replace("T", " ");
    }),
    datasets: modelJson.map((e, i) => {
        let a = e.measurements.map(m => m.value);
        let b = new Array(numberOfMeasurements - e.measurements.length).fill(null);
        a.unshift(...b);
        return {
            label: e.sensorName,
            backgroundColor: colors[i],
            borderColor: colors[i],
            data: a,
            tension: 0.2
        };
    })
};
let options = {
    scales: {
        y: {
            suggestedMin: 10,
            suggestedMax: 25
        }
    }
};
let chart = new Chart(context, {
    type: 'line',
    data: data,
    options: options
});

console.log(modelJson[0].measurements);