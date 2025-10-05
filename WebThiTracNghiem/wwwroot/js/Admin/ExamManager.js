// ExamManager.js — bản rút gọn (chỉ giữ Export + Filter)
(() => {
  const qs  = (sel, root=document) => root.querySelector(sel);
  const qsa = (sel, root=document) => Array.from(root.querySelectorAll(sel));

  let examStatusFilter, examSearchInput, examForm;

  function init() {
    // Kiểm tra có đúng trang quản lý kỳ thi không
    const onRightPage = document.getElementById('examStatusFilter') || document.getElementById('examForm');
    if (!onRightPage) return;

    examStatusFilter = document.getElementById('examStatusFilter');
    examSearchInput  = document.getElementById('examSearchInput');
    examForm         = document.getElementById('examForm');

    // Bộ lọc
    examStatusFilter && examStatusFilter.addEventListener('change', filterExams);
    examSearchInput  && examSearchInput.addEventListener('input', filterExams);

    // Nút export
    const btnExport = document.getElementById('btnExportExam');
    btnExport && btnExport.addEventListener('click', (e) => {
      e.preventDefault();
      exportExams();
    });
  }

  document.addEventListener('DOMContentLoaded', init);
  window.ExamManagerReinit = init;

  // ===== Filter =====
  function filterExams() {
    const status = examStatusFilter ? examStatusFilter.value.toLowerCase() : '';
    const search = examSearchInput ? examSearchInput.value.trim().toLowerCase() : '';

    qsa('.data-table tbody tr').forEach(tr => {
      const tds = tr.querySelectorAll('td');
      if (tds.length < 4) return;

      const tenKyThi  = tds[1].innerText.toLowerCase();
      const monHoc    = tds[2].innerText.toLowerCase();
      const ttText    = tds[3].innerText.toLowerCase();

      const matchText   = !search || tenKyThi.includes(search) || monHoc.includes(search);
      let   matchStatus = true;
      if (status) {
        if (status === 'running')  matchStatus = ttText.includes('đang diễn ra');
        if (status === 'upcoming') matchStatus = ttText.includes('sắp diễn ra');
        if (status === 'finished') matchStatus = ttText.includes('đã kết thúc');
      }
      tr.style.display = (matchText && matchStatus) ? '' : 'none';
    });
  }

  // ===== Export =====
    function exportExams() {
        const keyword = $("#examSearchInput").val().trim();
        const status = $("#examStatusFilter").val();

        $.ajax({
            url: "/Admin/Exam/GetAllExams",
            type: "GET",
            data: { keyword, status },
            success: function (exams) {
                if (!exams || exams.length === 0) {
                    showNotification('Không có dữ liệu để export!', 'error');
                    return;
                }

                const data = exams.map(e => {
                    const now = new Date();
                    let trangThai = '';
                    const gioBD = new Date(e.gioBD);
                    const gioKT = new Date(e.gioKT);

                    if (gioBD <= now && gioKT > now) trangThai = 'Đang diễn ra';
                    else if (gioBD > now) trangThai = 'Sắp diễn ra';
                    else trangThai = 'Đã kết thúc';

                    return {
                        TieuDe: e.tieuDe,
                        MaDe: e.maDe,
                        GioBD: e.gioBD,
                        GioKT: e.gioKT,
                        TrangThai: trangThai
                    };
                });

                const ws = XLSX.utils.json_to_sheet(data, { header: ["TieuDe", "MaDe", "GioBD", "GioKT", "TrangThai"] });
                const wb = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(wb, ws, "Exams");
                XLSX.writeFile(wb, "exams.xlsx");

                showNotification('Export Excel thành công!', 'success');
            },
            error: function () {
                showNotification('Xuất Excel thất bại!', 'error');
            }
        });
    }
  window.exportExams = exportExams;
})();
// ===== Biến giữ trạng thái filter =====
var examStatus = "";
var examKeyword = "";

// ===== Cập nhật biến khi filter thay đổi =====
$(document).on("change", "#examStatusFilter", function () {
    examStatus = $(this).val();
});
$(document).on("input", "#examSearchInput", function () {
    examKeyword = $(this).val().trim();
});

// ===== Hàm load page phân trang =====
let loadExamPage = (url) => {
    $.get(url, function (data) {
        const tempDom = $("<div>").html(data);

        // Lấy table và pagination mới
        const newTable = tempDom.find(".table-container").html();
        const newPagination = tempDom.find(".pagination").html();

        if (newTable) $(".table-container").html(newTable);
        if (newPagination) $(".pagination").html(newPagination);

        // Giữ lại filter và từ khóa
        $("#examStatusFilter").val(examStatus);
        $("#examSearchInput").val(examKeyword);
    });
};

// ===== Xử lý click phân trang =====
$(document).on("click", "button.page-examManager", function (e) {
    e.preventDefault();

    const pageUrl = $(this).attr("href");
    if (!pageUrl) return;

    // Thêm filter & keyword vào URL
    const urlWithParams = pageUrl + `&status=${examStatus}&keyword=${encodeURIComponent(examKeyword)}`;
    loadExamPage(urlWithParams);
});

// ===== Xử lý tìm kiếm =====
$(document).on("click", "#btnExamSearch", function () {
    const keyword = $("#examSearchInput").val().trim();
    const status = $("#examStatusFilter").val();

    // Cập nhật biến toàn cục
    examKeyword = keyword;
    examStatus = status;

    $.ajax({
        url: "/admin/home/ExamManager",
        type: "GET",
        data: {
            page: 1,
            status: status,
            keyword: keyword
        },
        success: function (data) {
            const tempDom = $("<div>").html(data);

            const newTable = tempDom.find(".table-container").html();
            const newPagination = tempDom.find(".pagination").html();

            if (newTable) $(".table-container").html(newTable);
            if (newPagination) $(".pagination").html(newPagination);

            // Giữ lại filter và từ khóa
            $("#examStatusFilter").val(status);
            $("#examSearchInput").val(keyword);
        },
        error: function () {
            showNotification("Không thể tải dữ liệu tìm kiếm.", "error");
        }
    });
});


