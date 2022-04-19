$(function () {
    $(".therm").each((i, e) => {
        let str = $(e).find(".temp-value").contents().text();
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

    setTimeout(() => location.reload(), 120000);
});