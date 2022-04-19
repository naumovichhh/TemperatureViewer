$(function () {
    $(".therm:has(.p1)").css("backgroundColor", `rgb(0, 0, ${brightness})`);
    $(".therm:has(.p2)").css("backgroundColor", `rgb(0, ${brightness}, ${brightness})`);
    $(".therm:has(.p3)").css("backgroundColor", `rgb(${brightness}, ${brightness}, 0)`);
    $(".therm:has(.p4)").css("backgroundColor", `rgb(${brightness}, 0, 0)`);
});