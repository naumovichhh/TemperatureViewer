$(function () {
    console.log($("#Role").val());
    if ($("#Role").val() != "u")
        $("#collapse").collapse();

    $("#Role").on("change", function (e) {
        console.log($(this).val());
        let collapseDiv = $("#collapse");
        console.log(collapseDiv.hasClass("show"));
        if ($(this).val() != "u" && collapseDiv.hasClass("show"))
            collapseDiv.collapse("hide");
        else if ($(this).val() == "u" && !collapseDiv.hasClass("show"))
            collapseDiv.collapse("show");
    })
});