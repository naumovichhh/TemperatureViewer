$(document).ready(function () {
    $(".therm-li").each((i, e) => {
        let r = 0, g = brightness, b = 0;
        let tempValueSpan = $(e).find(".temp-value");
        let str = tempValueSpan.contents().text();
        if (str == "") {
            let next = tempValueSpan[0].nextSibling;
            next.textContent = "";
            r = g = b = brightness;
            setBackgroundColor(e, r, g, b);
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

        setBackgroundColor(e, r, g, b)
    });
});

function setBackgroundColor(e, r, g, b) {
    let thermColor = `rgb(${r}, ${g}, ${b})`;
    let liColor = `rgb(${r + 110}, ${g + 110}, ${b + 110})`
    $(e).find(".therm").each((i, t) => {
        t.style.backgroundColor = thermColor;
    })
    e.style.backgroundColor = liColor;
}

function onValueUpdated() {
    $(".therm-li").each((i, e) => {
        let r = 0, g = brightness, b = 0;
        let tempValueSpan = $(e).find(".temp-value");
        let str = tempValueSpan.contents().text();
        if (str == "") {
            r = g = b = brightness;
            setBackgroundColor(e, r, g, b);
            return;
        }
        else {
            let next = tempValueSpan[0].nextSibling;
            next.textContent = "°";
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

        setBackgroundColor(e, r, g, b);
    });
}