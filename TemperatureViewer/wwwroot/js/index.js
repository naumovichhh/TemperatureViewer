let perfectTemperature = 20;
let minPossibleTemperature = 12;
let maxPossibleTemperature = 30;
let brightness = 100;

$(document).ready(function () {
    $(".therm-li").each((i, e) => {
        let str = $(e).find(".temp-value").contents().text();
        let value = Number(str.replace(",", "."));
        let r = 0, g = brightness, b = 0;

        let yellowPoint = (maxPossibleTemperature + perfectTemperature) / 2;
        let cyanPoint = (minPossibleTemperature + perfectTemperature) / 2;
        if (value >= maxPossibleTemperature || value <= minPossibleTemperature) {
            g = 0;
        }
        if (value > yellowPoint) {
            r = brightness;
        }
        if (value < cyanPoint) {
            b = brightness
        }

        let thermColor = `rgb(${Math.round(r)}, ${Math.round(g)}, ${Math.round(b)})`;
        let liColor = `rgb(${Math.round(r + 90)}, ${Math.round(g + 90)}, ${Math.round(b + 90)})`
        $(e).find(".therm").each((i, t) => {
            t.style.backgroundColor = thermColor;
        })
        e.style.backgroundColor = liColor;

        setTimeout(() => location.reload(), 120000);
    });
});