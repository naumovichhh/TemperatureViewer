$(document).ready(function () {
    $(".therm-li").each((i, e) => {
        let str = $(e).find(".temp-value").contents().text();
        let value = Number(str.replace(",", "."));
        let r = 0, g = brightness, b = 0;

        if (value >= thresholds[i][3] || value <= thresholds[i][0]) {
            g = 0;
        }
        if (value >= thresholds[i][2]) {
            r = brightness;
        }
        if (value <= thresholds[i][1]) {
            b = brightness
        }

        let thermColor = `rgb(${r}, ${g}, ${b})`;
        let liColor = `rgb(${r + 110}, ${g + 110}, ${b + 110})`
        $(e).find(".therm").each((i, t) => {
            t.style.backgroundColor = thermColor;
        })
        e.style.backgroundColor = liColor;
    });
});