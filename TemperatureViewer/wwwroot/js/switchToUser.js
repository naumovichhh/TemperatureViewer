$("#Role").on("change", function (e) {
    if ($(this).val() == "u" && !$("#checkAll").is(":checked")) {
        $("#checkAll").trigger("click");
    }
    if ($(this).val() != "u" && $("#checkAll").is(":checked")) {
        $("#checkAll").trigger("click");
    }
})