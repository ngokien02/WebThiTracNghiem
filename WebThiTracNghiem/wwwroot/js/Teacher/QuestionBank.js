$(document).on('change', '#qbexam-file', function (e) {
    const loadingOverlay = $('#loading-overlay');
    const questionModal = $('#qb-question-modal');
    const modalQuestionList = $('#modal-question-list');
    const file = e.target.files[0];
    if (!file) return;

    const allowedTypes = [
        'application/pdf',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/msword'
    ];
    if (!allowedTypes.includes(file.type)) {
        alert('Chỉ hỗ trợ file .doc, .docx hoặc .pdf');
        return;
    }

    const formData = new FormData();
    formData.append('examFile', file);

    loadingOverlay.show();

    $.ajax({
        url: '/Teacher/Home/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (data) {
            loadingOverlay.hide();
            if (!data || data.trim() === "") {
                alert("Định dạng file lỗi hoặc không hỗ trợ, vui lòng thử lại!");
                return;
            }
            modalQuestionList.html(data);
            questionModal.css('display', 'flex');
        },
        error: function () {
            loadingOverlay.hide();
            alert("Có lỗi khi xử lý file!");
        }
    });
});

function qbRenderQuestionModal(questions) {
    const qbModalQuestionList = $('#modal-question-list');
    qbModalQuestionList.empty();

    questions.forEach((q, i) => {
        const qHtml = $(`
            <div class="question-item">
                <div class="question-text"><b>Câu ${i + 1}:</b> ${q.NoiDung}</div>
                <ul class="answer-list"></ul>
            </div>
        `);

        q.DapAnList.forEach(ans => {
            const li = $(`<li class="answer-item"><span class="answer-text">${ans.NoiDung}</span></li>`);
            if (ans.DungSai) li.addClass('correct').attr('data-correct', 'true');
            qHtml.find('.answer-list').append(li);
        });

        qbModalQuestionList.append(qHtml);
    });
}
$(document).on('click', '#qb-modal-accept-btn', function () {
    const chuDeInput = $('#ChuDe');
    const chuDe = chuDeInput.val().trim();

    // Kiểm tra chủ đề
    if (!chuDe) {
        Swal.fire({
            title: 'Lỗi',
            text: 'Vui lòng nhập Chủ đề!',
            icon: 'warning',
            confirmButtonColor: '#d33'
        }).then(() => {
            setTimeout(() => {
                chuDeInput.focus(); 
            }, 10);
        });
        return; // dừng tiếp tục
    }
    const questions = [];

    $('#modal-question-list .question-item').each(function () {
        const qObj = {
            NoiDung: $(this).find('.question-text').text().replace(/^Câu \d+:/, '').trim(),
            Loai: "TracNghiem",
            DapAnList: []
        };

        $(this).find('.answer-item').each(function () {
            const ansText = $(this).find('.answer-text').text().trim();
            const isCorrect = $(this).hasClass('correct') || $(this).data('correct') === 'true';
            qObj.DapAnList.push({ NoiDung: ansText, DungSai: !!isCorrect });
        });

        const correctCount = qObj.DapAnList.filter(a => a.DungSai).length;
        qObj.Loai = (correctCount > 1) ? "NhieuDapAn" : "TracNghiem";

        questions.push(qObj);
    });

    // Tạo FormData đúng chuẩn
    const formData = new FormData();
    formData.append('ChuDe', chuDe);

    questions.forEach((q, i) => {
        formData.append(`CauHoiList[${i}].NoiDung`, q.NoiDung);
        formData.append(`CauHoiList[${i}].Loai`, q.Loai);

        q.DapAnList.forEach((ans, j) => {
            formData.append(`CauHoiList[${i}].DapAnList[${j}].NoiDung`, ans.NoiDung);
            formData.append(`CauHoiList[${i}].DapAnList[${j}].DungSai`, ans.DungSai.toString());
            // chuyển boolean sang string để ASP.NET Core bind tốt
        });
    });


    $.ajax({
        url: '/Teacher/Home/SaveImported',
        type: 'POST',
        data: formData,
        processData: false, // quan trọng
        contentType: false, // quan trọng
        success: function (res) {
            console.log("✅ AJAX thành công:", res);
            if (res && res.success) {
                $('#qb-question-modal').hide();
                Swal.fire({
                    title: 'Thành công!',
                    text: 'Câu hỏi đã được lưu vào cơ sở dữ liệu.',
                    icon: 'success',
                    confirmButtonColor: '#0963a3'
                }).then(() => location.reload());
            } else {
                Swal.fire({
                    title: 'Lỗi',
                    text: 'Lỗi khi lưu: ' + (res && res.message ? res.message : 'Không xác định'),
                    icon: 'error',
                    confirmButtonColor: '#d33'
                });
            }
        },
        error: function (xhr) {
            let msg = 'Có lỗi khi lưu câu hỏi!';
            try {
                const r = xhr.responseJSON || JSON.parse(xhr.responseText);
                if (r && r.message) msg = r.message;
            } catch (e) { }
            Swal.fire({
                title: 'Lỗi',
                text: msg,
                icon: 'error',
                confirmButtonColor: '#d33'
            });
        }
    });
});

// Hủy modal
$(document).on('click', '#qb-modal-cancel-btn', function () {
    $('#qb-question-modal').hide();
});


//xu ly load ngan hang cau hoi theo chu de
$(document).on("change", "#questionTopic", function () {
    let selectedId = $(this).val();
    reloadQuestionBank(selectedId);
});
function reloadQuestionBank(chuDeId) {
    $.ajax({
        url: '/teacher/home/LoadQuestionByTopic',
        type: 'GET',
        data: { chuDeId: chuDeId || null },
        success: function (res) {
            $(".questions-list").fadeOut(100, function () {
                $(this).html(res).fadeIn(100);
            });
        },
        error: function () {
            showNotification("Lỗi khi load danh sách câu hỏi!", "error");
        }
    });
}


//xu ly phan trang questionbank
let loadPageQB = (url) => {
    $.get(url, function (data) {
        $(".questions-list").fadeOut(100, function () {
            $(".questions-list").html(data).fadeIn(100);
        });
    });
}
$(document).on("click", "button.btn-qb", function (e) {
    e.preventDefault();
    let pageUrl = $(this).attr("href");

    if (pageUrl) {
        loadPageQB(pageUrl);
    }
});

// Khi click vào nút sửa
$(document).on("click", "a.btn-editQuestionBank", function (e) {
    e.preventDefault();

    const id = $(this).data("id");
    if (!id) return alert("Không xác định được ID câu hỏi.");

    $.get(`/Teacher/QuestionBank/GetEditQuestion?id=${id}`, function (data) {
        if (!data || data.success === false) {
            alert(data.message || "Không tải được dữ liệu câu hỏi.");
            return;
        }

        // Gán dữ liệu vào modal
        $("#qb-edit-id").val(data.id);
        $("#qb-edit-noidung").val(data.noiDung);

        // Fill dropdown chủ đề
        $.get('/Teacher/QuestionBank/GetAllChuDe', function (chuDes) {
            const select = $("#qb-edit-chude").empty();
            chuDes.forEach(cd => {
                const selected = cd.id === data.chuDeId ? "selected" : "";
                select.append(`<option value="${cd.id}" ${selected}>${cd.tenChuDe}</option>`);
            });
        });

        // Fill danh sách đáp án
        const list = $("#qb-edit-dapan-list").empty();
        data.dapAnList?.forEach(d => {
            list.append(`
				<div class="dapan-item">
					<input type="text" class="dapan-noidung" value="${d.noiDung}" data-id="${d.id}" />
					<label><input type="checkbox" class="dapan-dungsai" ${d.dungSai ? "checked" : ""}> Đúng</label>
				</div>
			`);
        });

        // Hiển thị modal và overlay
        openModal();
    });
});

// Mở modal
function openModal() {
    $("#qb-edit-modal").fadeIn(200).css("display", "flex"); // hiện modal, giữ display:flex
}

// Đóng modal
function closeModal(modalId) {
    $("#" + modalId).fadeOut(150, function () {
        $(this).css("display", "none");
    });
}

// Click nút Hủy
$(document).on("click", "#qb-edit-cancel-btn", function () {
    console.log("Clicked cancel");
    closeModal("qb-edit-modal");
});
$(document).on("submit", "#qb-edit-form", function (e) {
    e.preventDefault(); // ngăn form submit mặc định

    const questionId = $("#qb-edit-id").val();
    const noiDung = $("#qb-edit-noidung").val().trim();
    const chuDeId = $("#qb-edit-chude").val();

    if (!noiDung) {
        showNotification("Vui lòng nhập nội dung câu hỏi!", "error");
        return;
    }

    // Lấy danh sách đáp án
    const dapAnList = [];
    $("#qb-edit-dapan-list .dapan-item").each(function () {
        const daNoiDung = $(this).find(".dapan-noidung").val().trim();
        const daDungSai = $(this).find(".dapan-dungsai").is(":checked");

        if (daNoiDung) {
            dapAnList.push({
                Id: $(this).find(".dapan-noidung").data("id"),
                NoiDung: daNoiDung,
                DungSai: daDungSai
            });
        }
    });

    if (dapAnList.length === 0) {
        showNotification("Cần ít nhất 1 đáp án!", "error");
        return;
    }

    // Tạo object gửi lên controller
    const payload = {
        Id: questionId,
        NoiDung: noiDung,
        ChuDeId: chuDeId,
        DapAnList: dapAnList
    };

    // Gọi AJAX
    $.ajax({
        url: '/Teacher/QuestionBank/EditQuestion',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.success) {
                showNotification(res.message, "success");
                closeModal("qb-edit-modal");
                const chuDeId = $("#qb-edit-chude").val();
                reloadQuestionBank(chuDeId);
            } else {
                showNotification(res.message || "Lỗi khi cập nhật câu hỏi!", "error");
            }
        },
        error: function (xhr) {
            let msg = "Lỗi khi gọi server!";
            try {
                const r = xhr.responseJSON || JSON.parse(xhr.responseText);
                if (r && r.message) msg = r.message;
            } catch { }
            showNotification(msg, "error");
        }
    });
});
$(document).on("click", "a.btn-deleteQuestionBank", function (e) {
    e.preventDefault();

    const questionId = $(this).data("id");
    if (!questionId) return showNotification("ID câu hỏi không xác định!", "error");

    Swal.fire({
        title: "Xác nhận",
        text: "Bạn có chắc muốn xóa câu hỏi này?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Xóa",
        cancelButtonText: "Hủy",
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Teacher/QuestionBank/DeleteQuestionBank',
                type: 'POST',
                data: { id: questionId },
                success: function (res) {
                    if (res.success) {
                        showNotification(res.message, "success");
                        const chuDeId = $("#questionTopic").val();
                        reloadQuestionBank(chuDeId);
                    } else {
                        showNotification(res.message || "Xóa câu hỏi thất bại!", "error");
                    }
                },
                error: function () {
                    showNotification("Có lỗi xảy ra khi xóa câu hỏi!", "error");
                }
            });
        }
    });
});
// Open modal
$(document).on("click", "#btn-add-question", function () {
    // Load dropdown chủ đề
    $.get('/Teacher/QuestionBank/GetAllChuDe', function (chuDes) {
        const select = $("#add-chude").empty();
        chuDes.forEach(cd => select.append(`<option value="${cd.id}">${cd.tenChuDe}</option>`));
    });

    $("#qb-add-modal").fadeIn(200);
});

// Close modal và reset form
$(document).on("click", "#qb-add-cancel-btn", function () {
    $("#qb-add-modal").fadeOut(150);

    // Reset form
    const form = $("#qb-add-form")[0];
    form.reset(); // reset input, textarea, select

    // Xóa toàn bộ đáp án đã thêm
    $("#add-dapan-list").empty();

    // Nếu muốn, set dropdown về giá trị mặc định (nếu có)
    $("#add-chude").prop('selectedIndex', 0);
});


// Thêm đáp án mới
$(document).on("click", "#add-dapan-btn", function () {
    $("#add-dapan-list").append(`
        <div class="dapan-item">
            <input type="text" class="dapan-noidung" placeholder="Nội dung đáp án" />
            <label><input type="checkbox" class="dapan-dungsai"> Đúng</label>
        </div>
    `);
});

$(document).on("submit", "#qb-add-form", function (e) {
    e.preventDefault();

    const chuDeId = $("#add-chude").val();
    const noiDung = $("#add-noidung").val().trim();
    if (!noiDung) return showNotification("Vui lòng nhập nội dung câu hỏi!", "error");

    const dapAnList = [];
    $("#add-dapan-list .dapan-item").each(function () {
        const daNoiDung = $(this).find(".dapan-noidung").val().trim();
        const daDungSai = $(this).find(".dapan-dungsai").is(":checked");
        if (daNoiDung) dapAnList.push({ NoiDung: daNoiDung, DungSai: daDungSai });
    });

    if (dapAnList.length === 0) return showNotification("Cần ít nhất 1 đáp án!", "error");

    // Xác định loại câu hỏi
    const correctCount = dapAnList.filter(d => d.DungSai).length;
    // Kiểm tra ít nhất 1 đáp án đúng
    if (correctCount === 0) {
        return showNotification("Cần ít nhất 1 đáp án đúng!", "error");
    }

    const loai = correctCount > 1 ? "NhieuDapAn" : "TracNghiem";

    const payload = { ChuDeId: chuDeId, NoiDung: noiDung, Loai: loai, DapAnList: dapAnList };

    $.ajax({
        url: '/Teacher/QuestionBank/AddQuestion', 
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.success) {
                showNotification(res.message || "Tạo câu hỏi thành công!", "success");
                $("#qb-add-modal").fadeOut(150);
                reloadQuestionBank(chuDeId);
            } else {
                showNotification(res.message || "Lỗi khi tạo câu hỏi!", "error");
            }
        },
        error: function () {
            showNotification("Lỗi khi gọi server!", "error");
        }
    });
});

$(document).on("click", "#btn-ExportFormat", function () {
    $.ajax({
        url: '/Teacher/QuestionBank/ExportFormat', 
        method: 'GET',
        xhrFields: {
            responseType: 'blob' 
        },
        success: function (data, status, xhr) {
            // Lấy tên file từ header nếu server gửi
            //var filename = "CauHoiMau.docx";
            //var disposition = xhr.getResponseHeader('Content-Disposition');
            //if (disposition && disposition.indexOf('filename=') !== -1) {
            //    filename = disposition.split('filename=')[1].replace(/"/g, '');
            //}

            // Tạo blob và trigger download
            var blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "CauHoiMau.docx";
            //link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function () {
            showNotification("Có lỗi khi tải file mẫu!");
        }
    });
});








