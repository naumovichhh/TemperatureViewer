let context = document.getElementById("chart");
let colors = ["#ff0000", "#00ff00", "#0000ff", "#000000"];
let data = {
    labels: modelJson[0].measurements.map((e) => {
        return e.time;
    }),
    datasets:modelJson.map((e, i) => {
        return {
            label: e.sensorName,
            backgroundColor: colors[i],
            borderColor: colors[i],
            data: e.measurements.map(m => m.value)
        };
    })
};
let chart = new Chart(context, {
    type: 'line',
    data: data
});

console.log(modelJson);