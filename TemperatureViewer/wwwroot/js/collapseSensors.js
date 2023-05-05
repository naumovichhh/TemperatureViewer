$(function () {
    $("#Role").on("change", function (e) {
        let collapseDiv = $("#collapse");
        if ($(this).val() != "u" && collapseDiv.hasClass("show"))
            collapseDiv.collapse("hide");
        else if ($(this).val() == "u" && !collapseDiv.hasClass("show"))
            collapseDiv.collapse("show");
    })

    let collapseDiv = $("#collapse");
    if ($("#Role").val() == "u")
        collapseDiv.addClass("collapse show");
    else
        collapseDiv.addClass("collapse");
    collapseDiv.css("display", "");
});