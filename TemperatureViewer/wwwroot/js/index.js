$(document).ready(function () {
    $(".therm-li").each((i, e) => {
        let r = 0, g = brightness, b = 0;
        let str = $(e).find(".temp-value").contents().text();
        if (str == "") {
            e.nextSibling.textContent = "";
            r = g = b = brightness;
            let thermColor = `rgb(${r}, ${g}, ${b})`;
            let liColor = `rgb(${r + 110}, ${g + 110}, ${b + 110})`
            $(e).find(".therm").each((i, t) => {
                t.style.backgroundColor = thermColor;
            })
            e.style.backgroundColor = liColor;
            return;
        }

        let value = Number(str.replace(",", "."));

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