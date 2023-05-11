$(function () {
    const hubConnection = new signalR.HubConnectionBuilder().withUrl("/updateTemperature").build();

    hubConnection.on("Update", function (arr) {
        Array.from(arr).forEach(e => {
            console.log("#sensor-id-" + e.sensor);
            console.log(e.value?.toLocaleString());
            $("#sensor-id-" + e.sensor).text(e.value?.toLocaleString());
        });
    });

    hubConnection.start()
        .then(function () {
            let arr = $(".temp-value").get();
            let tempValueSensorIds = arr.map(e => parseInt($(e).attr("id").substring(10)));
            hubConnection.invoke("Subscribe", tempValueSensorIds);
        });
});