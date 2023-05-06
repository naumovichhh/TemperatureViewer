$(function () {
    $("#validatable").validate().settings.ignore = ":hidden, .ignore-validation";
    $("#validatable").valid();
});