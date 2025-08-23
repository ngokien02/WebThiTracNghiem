// Hien thi lich su de thi
$(document).on("click", "a.History", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    $.get(url, function (data) {
        $(".main-content").html(data);
    });
    $('a').removeClass('active');
    $('a.History').addClass('active');
});