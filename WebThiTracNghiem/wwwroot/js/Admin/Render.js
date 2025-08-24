$(() => {

    function handleAjaxNav(selector) {
        $(document).on("click", selector, function (e) {
            e.preventDefault();

            var url = $(this).attr("href");

            $.get(url, (data) => {
                $(".main-content").html(data);
            });

            // Xóa active của tất cả, rồi add lại cho link hiện tại
            $("a").removeClass("active");
            $(this).addClass("active");
        });
    }

    handleAjaxNav("a.UserManager");
    handleAjaxNav("a.ExamManager");

});
