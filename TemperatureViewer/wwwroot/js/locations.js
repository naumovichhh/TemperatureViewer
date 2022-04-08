$(function () {
    var modal = $("#modal");

    $(".img-clickable").click(function (e) {
        modal.css("display", "block");
        $("#modalImg").attr("src", this.src)
    });

    $(".close").click(function (e) {
        modal.css("display", "none");
    });
});