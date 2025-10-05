(() => {
    // ===== Biến giữ trạng thái =====
    let currentKeyword = "";
    let currentPage = 1;

    // ===== Khi nhập vào ô tìm kiếm =====
    $(document).on("input", "#searchInput", function () {
        currentKeyword = $(this).val().trim();
    });

    // ===== Khi bấm nút tìm kiếm =====
    $(document).on("click", "#SearchStudentExam", function (e) {
        e.preventDefault();
        currentKeyword = $("#searchInput").val().trim();
        currentPage = 1;

        loadExamPage(currentPage, currentKeyword, true);
    });

    // ===== Khi bấm phân trang =====
    $(document).on("click", ".page-ExamList", function (e) {
        e.preventDefault();
        const page = $(this).data("page");
        if (!page || $(this).prop("disabled")) return;

        currentPage = page;
        loadExamPage(currentPage, currentKeyword, false);
    });

    // ===== Hàm load dữ liệu từ server =====
    function loadExamPage(page, keyword, isSearch = false) {
        $.ajax({
            url: "/Student/Exam",
            type: "GET",
            data: { page, keyword },
            success: function (data) {
                const tempDom = $("<div>").html(data);
                const newExamGrid = tempDom.find(".exam-grid").html();

                if (newExamGrid && newExamGrid.trim() !== "") {
                    $(".exam-grid").html(newExamGrid);
                    const newPagination = tempDom.find(".pagination").html();
                    if (newPagination) $(".pagination").html(newPagination);
                } else if (isSearch) {
                    showNotification("Không tìm thấy bài thi phù hợp với từ khóa!", "info");
                }

                // Luôn giữ giá trị tìm kiếm trong input
                $("#searchInput").val(keyword);
            },
            error: function () {
                showNotification("Không thể tải danh sách bài thi. Vui lòng thử lại!", "error");
            }
        });
    }

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
})();
