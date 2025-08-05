$(() => {

    function handleAjaxNav(selector) {
        $(document).on("click", selector, function (e) {
            e.preventDefault();

            var url = $(this).attr("href");

            $.get(url, function (data) {
                $(".main-content").html(data);
            });
            $('a').removeClass('active');
            $(selector).addClass('active');
        });
    }

    handleAjaxNav("a.QuestionBank");
    handleAjaxNav("a.CreateExam");
    handleAjaxNav("a.Reports");
    handleAjaxNav("a.ActiveExam");

});
