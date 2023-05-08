$(function () {
    $("#date-from").val(bagValueFrom);
    $("#date-to").val(bagValueTo);

    $("#apply-period").click(function () {
        let valueFrom = $("#date-from").val();
        let valueTo = $("#date-to").val();
        let searchParams = [`from=${valueFrom ?? ""}`, `to=${valueTo ?? ""}`, `locationId=${locationId ?? ""}`];
        if (!valueFrom || !valueTo) {
            if (!valueFrom && !valueTo) {
                location.href = location.protocol + "//" + location.host + action + (id ? `/${id}` : "") + "?" + searchParams.join("&");
            }
            else {
                alert("Некорректный период");
                return;
            }
        }

        let dateFrom = new Date(valueFrom);
        let dateTo = new Date(valueTo);
        if (dateFrom > dateTo || dateTo - dateFrom > 366 * 24 * 3600 * 1000) {
            alert("Некорректный период");
            return;
        }

        location.href = location.href = location.protocol + "//" + location.host + action + (id ? `/${id}` : "") + "?" + searchParams.join("&");
    });

    $("#download-data").click(function () {
        let valueFrom = $("#date-from").val();
        let valueTo = $("#date-to").val();
        let searchParams = [`from=${valueFrom ?? ""}`, `to=${valueTo ?? ""}`, `locationId=${locationId ?? ""}`];
        if (!valueFrom || !valueTo) {
            if (!valueFrom && !valueTo) {
                location.href = location.protocol + "//" + location.host + "/Home/Download" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
            }
            else {
                alert("Некорректный период");
                return;
            }
        }


        let dateFrom = new Date(valueFrom);
        let dateTo = new Date(valueTo);
        if (dateFrom > dateTo || dateTo - dateFrom > 366 * 24 * 3600 * 1000) {
            alert("Некорректный период");
            return;
        }

        location.href = location.protocol + "//" + location.host + "/Home/Download" + (id ? `/${id}` : "") + "?" + searchParams.join("&");
    });
});