var div = $(".toggle");
$(".select select").change(function () {
    var res = $(this).val();
    if (res == 5) {
        div.fadeIn(500);
    } else {
        div.fadeOut(500);
    }
});