let context = document.getElementById("chart");
let colors = ["#ff0000", "#00ff00", "#0000ff", "#000000"];
let numberOfMeasurements = modelJson.map(m => m.measurements.length).reduce((acc, next) => {
    if (next > acc)
        return next;
    else
        return acc;
});
//let completedArrays = modelJson.map((e) => e.measurements.map(m => m.value));
//let ca2 = completedArrays.unshift(new Array(numberOfMeasurements - e.measurements.length).fill(null));
let data = {
    labels: modelJson[0].measurements.map((e) => {
        return e.time.slice(0, e.time.length - 11);
    }),
    datasets: modelJson.map((e, i) => {
        return {
            label: e.sensorName,
            backgroundColor: colors[i],
            borderColor: colors[i],
            data: e.measurements.map(m => m.value).unshift(...(new Array(numberOfMeasurements - e.measurements.length).fill(null))),
            tension: 0.2
        };
    })
};
let chart = new Chart(context, {
    type: 'line',
    data: data
});

console.log(modelJson);