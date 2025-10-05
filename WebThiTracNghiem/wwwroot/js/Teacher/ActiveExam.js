$(document).on("click", ".btn-editExam", function () {
    const id = $(this).data("id");
    if (!id) return alert("Không xác định được Id đề thi.");

    $.get(`/Teacher/Exam/GetEditExam?id=${id}`, function (data) {

        if (data.success === false) {
            showNotification(data.message, "error");
            return;
        }

        // ✅ Gán dữ liệu vào form (dùng camelCase key nếu ASP.NET Core trả JSON chuẩn)
        $("#examId").val(data.id);
        $("#examMaDe").val(data.maDe);
        $("#examTieuDe").val(data.tieuDe);
        $("#examGioBD").val(data.gioBD);
        $("#examGioKT").val(data.gioKT);
        $("#examThoiGian").val(data.thoiGian || 0);

        $("#examRandomCauHoi").prop("checked", !!data.randomCauHoi);
        $("#examRandomDapAn").prop("checked", !!data.randomDapAn);
        $("#examShowKQ").prop("checked", !!data.showKQ);

        // ✅ Hiển thị modal thuần
        $("#examModal").css({
            "display": "block",
            "position": "fixed",
            "top": "0",
            "left": "0",
            "width": "100%",
            "height": "100%",
            "background-color": "rgba(0,0,0,0.5)",
            "z-index": "9999"
        });
    }).fail(function () {
        alert("Không tải được dữ liệu đề thi từ server.");
    });
});

// Hàm đóng modal
function closeModal(modalId) {
    $("#" + modalId).css("display", "none");
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
} function loadActiveExams(page = 1) {
    $.ajax({
        url: '/Teacher/home/ActiveExam',
        type: 'GET',
        data: { page },
        success: function (html) {
            $(".main-content").html(html); // Load lại danh sách + pagination
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("AJAX loadActiveExams lỗi:");
            console.error("Status:", jqXHR.status);
            console.error("Response Text:", jqXHR.responseText);
            console.error("TextStatus:", textStatus);
            console.error("ErrorThrown:", errorThrown);
            showNotification("Không thể tải danh sách đề thi.", "error");
        }
    });
}
// ===== Click nút phân trang =====
$(document).on("click", ".page-ActiveExam", function (e) {
    e.preventDefault();
    const $btn = $(this);
    if ($btn.prop("disabled")) return;

    const page = parseInt($btn.data("page"));
    if (isNaN(page)) return;

    loadActiveExams(page);
});


// Xử lí lưu edit
$(document).on("submit", "#examForm", function (e) {
    e.preventDefault();

    const dataObj = {
        Id: $("#examId").val(),
        MaDe: $("#examMaDe").val(),
        TieuDe: $("#examTieuDe").val(),
        GioBD: $("#examGioBD").val(),
        GioKT: $("#examGioKT").val(),
        ThoiGian: parseInt($("#examThoiGian").val()) || 0,
        RandomCauHoi: $("#examRandomCauHoi").is(":checked"),
        RandomDapAn: $("#examRandomDapAn").is(":checked"),
        ShowKQ: $("#examShowKQ").is(":checked")
    };

    $.ajax({
        url: "/Teacher/Exam/EditExam",
        type: "POST",
        contentType: "application/json", // Gửi JSON
        data: JSON.stringify(dataObj),
        success: function (res) {
            showNotification(res.message, res.success ? "success" : "error");
            loadActiveExams();
            if (res.success) closeModal("examModal");
        },
        error: function () {
            showNotification("Không thể kết nối đến server", "error");
        }
    });
});
// Xử lý click nút xóa
// 🧩 Xử lý click nút xóa đề thi
$(document).on("click", ".btn-deleteExam", function () {
    const id = $(this).data("id");
    if (!id) {
        showNotification("Không xác định được Id đề thi.", "error");
        return;
    }

    Swal.fire({
        title: "Xác nhận",
        text: "Bạn có chắc muốn xóa đề thi này không?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Có",
        cancelButtonText: "Không",
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6"
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: '/Teacher/Exam/DeleteExam',
                type: 'POST',
                data: { id: id },
                success: function (res) {

                    if (!res || res.success === false) {
                        showNotification(res?.message || "Xóa thất bại.", "error");
                        return;
                    }

                    // ✅ Hiển thị thông báo thành công
                    showNotification(res.message, "success");

                    // ✅ Làm mới danh sách sau khi xóa
                    loadActiveExams();
                },
                error: function (xhr, status, error) {
                    let message = "Không thể kết nối đến server.";

                    // Nếu server có trả lỗi cụ thể
                    if (xhr.responseText) {
                        message = xhr.responseText;
                    } else if (error) {
                        message = error;
                    }

                    console.error("Chi tiết lỗi:", xhr); 
                    showNotification("Lỗi server: " + message, "error");
                }
            });
        }
    });
});



