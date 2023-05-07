$(function () {
    $("#checkAll").on("change", function () {
        let checked = this.checked;
        $(".sensor-checkbox").each(function () {
            this.checked = checked;
        });
    });
    $(".sensor-checkbox").on("change", function () {
        setCheckAll();
    });
    setCheckAll();
});

function setCheckAll() {
    let allChecked = true;
    $(".sensor-checkbox").each(function () {
        if (!this.checked) {
            allChecked = false;
            return false;
        }
    });
    $("#checkAll").prop("checked", allChecked);
}