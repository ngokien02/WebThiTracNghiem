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
                chuDeInput.focus(); // focus sau khi alert đóng
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

    console.log("🔹 Gửi AJAX SaveImported với FormData...");

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
            console.log("❌ AJAX lỗi:", xhr);
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

    $.ajax({
        url: '/teacher/home/LoadQuestionByTopic',
        type: 'GET',
        data: {
            chuDeId: selectedId === 0 ? null : selectedId
        },
        success: function (res) {
            $(".questions-list").html(res);
        },
        error: function () {
            alert("Lỗi khi load câu hỏi!");
        }
    });
});

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