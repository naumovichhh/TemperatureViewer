$(function () {
    $("#Role").on("change", function (e) {
        console.log("ROLE CHANGE");
        let collapseDiv = $("#collapse");
        if ($(this).val() != "u" && collapseDiv.hasClass("show"))
            collapseDiv.collapse("hide");
        else if ($(this).val() == "u" && !collapseDiv.hasClass("show"))
            collapseDiv.collapse("show");
    })

    $("#checkAll").on("click", function (e) {
        let checked = this.checked;
        let collapseNested = $("#collapse-nested");
        if (checked) {
            collapseNested.collapse("hide");
        }
        else {
            collapseNested.collapse("show");
            if (collapseNested.css("display") == "none")
                collapseNested.css("display", "");
        }
    })

    setNestedVisibility();

    let collapseDiv = $("#collapse");
    if ($("#Role").val() == "u")
        collapseDiv.addClass("collapse show");
    else
        collapseDiv.addClass("collapse");
    collapseDiv.css("display", "");
});

function setNestedVisibility() {
    let allChecked = true;
    $(".sensor-checkbox").each(function () {
        if (!this.checked) {
            allChecked = false;
            return false;
        }
    });
    if (allChecked) {
        $("#collapse-nested").css("display", "none");
    }
}