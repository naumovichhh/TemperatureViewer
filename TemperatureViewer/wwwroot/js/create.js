$(function () {
    $("#checkAll").on("change", function () {
        let checked = this.checked;
        $(".sensor-checkbox").each(function (c) {
            this.checked = checked;
        });
    });
});