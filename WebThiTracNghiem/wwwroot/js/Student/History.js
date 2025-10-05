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

$(document).on("click", "a.Guide", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    $.get(url, function (data) {
        $(".main-content").html(data);
    });
    $('a').removeClass('active');
    $('a.Guide').addClass('active');
});

$(() => {
    let currentPage = 1;
    let currentKeyword = "";

    // 🔍 Khi click nút tìm kiếm
    $(document).on("click", ".btnHistorySearch", function () {
        currentKeyword = $(".exam-filters input").val().trim();
        currentPage = 1;
        loadHistory(currentPage, currentKeyword);
    });

    // 🔢 Khi click phân trang
    $(document).on("click", ".page-History", function () {
        if ($(this).prop("disabled")) return;
        currentPage = $(this).data("page") || parseInt($(this).text());
        loadHistory(currentPage, currentKeyword);
    });

    function loadHistory(page, keyword) {
        $.ajax({
            url: '/student/home/History',
            type: 'GET',
            data: { page, keyword },
            success: function (data) {
                $("#history-wrapper").html(data);
                const noResults = $(data).find("#no-results").length > 0;
                if (noResults) {
                    showNotification("Không tìm thấy kết quả phù hợp!", "info");
                }
            },
            error: function () {
                showNotification("Không thể tải lịch sử làm bài.", "error");
            }
        });
    }
});

// ===== Hàm hiển thị notification =====
function showNotification(message, type = "info") {
    let icon = type === "success" ? "success" : type === "error" ? "error" : "info";
    Swal.fire({
        text: message,
        icon: icon,
        showConfirmButton: true,
        timerProgressBar: true,
        allowOutsideClick: false,
        allowEscapeKey: true
    });
}
