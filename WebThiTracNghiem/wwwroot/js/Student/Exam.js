﻿$(() => {
    // Hien thi ds de thi
    $(document).on("click", "a.ListExam", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        $.get(url, function (data) {
            $(".main-content").html(data);
        });
        $('a').removeClass('active');
        $('a.ListExam').addClass('active');
    });

    // Vao trang lam bai thi
    $(document).on("click", "button.DoExam", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        $.get(url, function (data) {
            $("body").html(data);
        });
    });
})