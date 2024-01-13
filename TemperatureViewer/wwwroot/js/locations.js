$(function () {
    $(".therm").each((i, e) => {
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
        let p1 = Number($(e).find(".p1").contents().text());
        let p2 = Number($(e).find(".p2").contents().text());
        let p3 = Number($(e).find(".p3").contents().text());
        let p4 = Number($(e).find(".p4").contents().text());

        if (value >= p4 || value <= p1) {
            g = 0;
        }
        if (value >= p3) {
            r = brightness;
        }
        if (value <= p2) {
            b = brightness
        }

        setBackgroundColor(e, r, g, b);
    });
});

function setBackgroundColor(e, r, g, b) {
    let thermColor = `rgb(${r}, ${g}, ${b})`;
    e.style.backgroundColor = thermColor;
}

function onValueUpdated() {
    $(".therm").each((i, e) => {
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
        let p1 = Number($(e).find(".p1").contents().text());
        let p2 = Number($(e).find(".p2").contents().text());
        let p3 = Number($(e).find(".p3").contents().text());
        let p4 = Number($(e).find(".p4").contents().text());
        let r = 0, g = brightness, b = 0;

        if (value >= p4 || value <= p1) {
            g = 0;
        }
        if (value >= p3) {
            r = brightness;
        }
        if (value <= p2) {
            b = brightness
        }

        let thermColor = `rgb(${r}, ${g}, ${b})`;
        e.style.backgroundColor = thermColor;
    });
}

setTimeout(() => location.reload(), 900000);