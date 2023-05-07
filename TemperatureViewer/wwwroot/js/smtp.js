$(function () {
    $("#test-form").on("submit", function (e) {
        e.preventDefault();
        let form = $(this);
        let mainForm = $("#main-form");
        let formDataArray = form.serializeArray();
        let mainFormData = mainForm.serialize();
        let concated = mainFormData + "&testEmail=" + formDataArray.find(e => e.name == "testEmail").value;
        let testButton = $("#test-button");
        testButton.prop("disabled", true);
        $.ajax("/Admin/Test", {
            data: concated,
            method: "POST"
        })
            .done((r) => {
                let span = $("#test-result");
                if (r) {
                    span.removeClass("text-danger");
                    span.addClass("text-success");
                    span.text("Успешно.");
                }
                else {
                    span.removeClass("text-success");
                    span.addClass("text-danger");
                    span.text("Неудачно.");
                }
            })
            .fail((r) => {
                let span = $("#test-result");
                span.removeClass("text-success");
                span.addClass("text-danger");
                span.text("Ошибка.");
            })
            .always(() => testButton.prop("disabled", false));
    });
});