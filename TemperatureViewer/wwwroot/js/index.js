let perfectTemperature = 19;
let minPossibleTemperature = 10;
let maxPossibleTemperature = 28;
let brightness = 100;

$(document).ready(function () {
    $(".therm-li").each((i, e) => {
        let str = $(e).find(".temp-value").contents().text();
        let value = Number(str.replace(",", "."));
        let r = 0, g = 0, b = 0;
        if (value > perfectTemperature) {
            let maxRPoint = (maxPossibleTemperature + perfectTemperature) / 2;
            r = (r = brightness * ((value - perfectTemperature) / (maxRPoint - perfectTemperature))) <= brightness ? r : brightness;
            let gSubtrahend;
            gSubtrahend = brightness * ((value - maxRPoint) / (maxPossibleTemperature - maxRPoint));
            gSubtrahend = gSubtrahend > brightness ? brightness : gSubtrahend;
            g = brightness - gSubtrahend;
            g = g > brightness ? brightness : g;
        }
        else {
            let maxBPoint = (minPossibleTemperature + perfectTemperature) / 2;
            b = (b = brightness * ((perfectTemperature - value) / (perfectTemperature - maxBPoint))) <= brightness ? b : brightness;
            let gSubtrahend;
            gSubtrahend = brightness * ((maxBPoint - value) / (maxBPoint - minPossibleTemperature));
            gSubtrahend = gSubtrahend > brightness ? brightness : gSubtrahend;
            g = brightness - gSubtrahend;
            g = g > brightness ? brightness : g;
        }

        let thermColor = `rgb(${Math.round(r)}, ${Math.round(g)}, ${Math.round(b)})`;
        let liColor = `rgb(${Math.round(r + 90)}, ${Math.round(g + 90)}, ${Math.round(b + 90)})`
        $(e).find(".therm").each((i, t) => {
            t.style.backgroundColor = thermColor;
        })
        e.style.backgroundColor = liColor;
    });
});