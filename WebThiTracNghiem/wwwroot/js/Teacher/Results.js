

// ===== Biến giữ trạng thái filter =====
var resultKeyword = "";
var resultFromDate = "";
var resultToDate = "";
var resultExam = "";

// ===== Toggle dropdown lọc =====
function toggleFilterDropdown() {
    const dropdown = document.getElementById("filterDropdown");
    if (dropdown) {
        dropdown.style.display = dropdown.style.display === "block" ? "none" : "block";
    }
}

// ===== Cập nhật biến khi thay đổi =====
$(document).on("input", ".search-box input", function () {
    resultKeyword = $(this).val().trim();
});
$(document).on("change", "#fromDate", function () {
    resultFromDate = $(this).val();
});
$(document).on("change", "#toDate", function () {
    resultToDate = $(this).val();
});
$(document).on("change", "#examSelect", function () {
    resultExam = $(this).val();
});

// ===== Helper: build URL safe (thêm params) =====
function appendParamsToUrl(url, params) {
    if (!params) return url;
    return (url.indexOf('?') === -1) ? url + '?' + params : url + '&' + params;
}

// ===== Helper: thay thế nội dung bảng kết quả =====
function replaceResultsTableFromHtml(html, keepInputs = true) {
    const temp = $("<div>").append($.parseHTML(html));

    const newTableHtml = temp.find(".results-table-container").html();
    const newPaginationHtml = temp.find(".pagination").html();

    if (newTableHtml) $(".results-table-container").html(newTableHtml);
    if (newPaginationHtml) $(".pagination").html(newPaginationHtml);

    if (keepInputs) {
        $(".search-box input").val(resultKeyword);
        $("#fromDate").val(resultFromDate);
        $("#toDate").val(resultToDate);
        $("#examSelect").val(resultExam);
    }

    // 🔍 Kiểm tra nếu không có dữ liệu
    checkNoResultData();
}

// ===== Hàm kiểm tra dữ liệu rỗng =====
function checkNoResultData() {
    const rows = $(".results-table-container table tbody tr");
    const noData = rows.length === 0 || (rows.length === 1 && rows.text().trim() === "");

    if (noData) {
        $(".results-table-container").html(`
            <div class="no-results" style="text-align:center; padding:30px; color:#777;">
                <i class="fas fa-search-minus" style="font-size:40px; color:#ccc; margin-bottom:10px;"></i><br>
                Không tìm thấy kết quả phù hợp với từ khóa hoặc bộ lọc bạn chọn.
            </div>
        `);
        showNotification("Không tìm thấy kết quả phù hợp.", "info");
    }
}

// ===== Hàm load page phân trang =====
let loadResultPage = (pageUrl) => {
    if (!pageUrl) return;

    const params = `keyword=${encodeURIComponent(resultKeyword || "")}` +
        `&fromDate=${encodeURIComponent(resultFromDate || "")}` +
        `&toDate=${encodeURIComponent(resultToDate || "")}` +
        `&exam=${encodeURIComponent(resultExam || "")}`;

    const urlWithParams = appendParamsToUrl(pageUrl, params);

    $.get(urlWithParams, function (data) {
        replaceResultsTableFromHtml(data);
    }).fail(function () {
        showNotification("Không thể tải trang kết quả.", "error");
    });
};

// ===== Xử lý click phân trang =====
$(document).on("click", "button.page-Relsutlt", function (e) {
    e.preventDefault();
    const pageUrl = $(this).attr("href");
    if (!pageUrl) return;
    loadResultPage(pageUrl);
});

// ===== Xử lý tìm kiếm (nút kính lúp) =====
$(document).on("click", "#btnRelsutlSearch", function () {
    const keyword = $(".search-box input").val().trim();
    const fromDate = $("#fromDate").val();
    const toDate = $("#toDate").val();
    const exam = $("#examSelect").val();

    resultKeyword = keyword;
    resultFromDate = fromDate;
    resultToDate = toDate;
    resultExam = exam;

    $.get("/teacher/home/Results", {
        page: 1,
        keyword,
        fromDate,
        toDate,
        exam
    }, function (data) {
        replaceResultsTableFromHtml(data);
    }).fail(function () {
        showNotification("Không thể tải dữ liệu kết quả.", "error");
    });
});

// ===== Xử lý nút Áp dụng lọc =====
$(document).on("click", ".btn-apply", function () {
    const keyword = $(".search-box input").val().trim();
    const fromDate = $("#fromDate").val();
    const toDate = $("#toDate").val();
    const exam = $("#examSelect").val();

    resultKeyword = keyword;
    resultFromDate = fromDate;
    resultToDate = toDate;
    resultExam = exam;

    $.get("/teacher/home/Results", {
        page: 1,
        keyword,
        fromDate,
        toDate,
        exam
    }, function (data) {
        replaceResultsTableFromHtml(data);
        toggleFilterDropdown();
    }).fail(function () {
        showNotification("Không thể lọc kết quả.", "error");
    });
});// ===== Nút Xuất Excel =====
$(document).on("click", ".btn-exportResults", function () {
    const table = document.querySelector(".results-table");
    if (!table) {
        showNotification("Không có bảng dữ liệu để xuất!", "error");
        return;
    }

    // Chuyển bảng HTML sang workbook
    const wb = XLSX.utils.table_to_book(table, { sheet: "KetQua" });

    // Xuất file
    XLSX.writeFile(wb, "KetQuaThi.xlsx");
    showNotification("Export Excel thành công!", "success");
});

$(document).on("click", ".btn-deleteResult", function () {
    const id = $(this).data("id");
    if (!id) return showNotification("Không xác định được Id kết quả.", "error");

    Swal.fire({
        title: "Xác nhận",
        text: "Bạn có chắc muốn xóa kết quả này không?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Có",
        cancelButtonText: "Không"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Teacher/Result/DeleteKetQua',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    showNotification(res.message, res.success ? "success" : "error");
                    if (res.success) {
                        const currentPageUrl = $(".pagination button.active").attr("href") || "/teacher/home/Results?page=1";
                        loadResultPage(currentPageUrl);
                    }
                },
                error: function () {
                    showNotification("Không thể kết nối đến server", "error");
                }
            });
        }
    });
});


