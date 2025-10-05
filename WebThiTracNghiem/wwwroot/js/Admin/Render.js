$(() => {

    function handleAjaxNav(selector) {
        $(document).on("click", selector, function (e) {
            e.preventDefault();

            var url = $(this).attr("href");

            $.get(url, (data) => {
                $(".main-content").html(data);
            });

            $("a").removeClass("active");
            $(selector).addClass('active');
        });
    }

    handleAjaxNav("a.UserManager");
    handleAjaxNav("a.ExamManager");
    handleAjaxNav("a.PostList, button.PostList");
    handleAjaxNav("a.CreatePost");
    handleAjaxNav("a.GuideLink");
    handleAjaxNav("a.GuideAdmin");
});
