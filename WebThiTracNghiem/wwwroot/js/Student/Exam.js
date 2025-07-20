$(() => {

    // Hiển thị thông báo swal
    function showAlert(title, message, icon = "info", href) {
        Swal.fire({
            title: title,
            text: message,
            icon: icon,
            confirmButtonColor: "#0963a3"
        }).then((result) => {
            if (href != null || href != undefined) {
                window.location.href = href;
            }
        });
    };

    // Hien thi ds de thi
    $(document).on("click", "a.ListExam", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        $.get(url, function (data) {
            $(".main-content").html(data);
        });
        $('a').removeClass('active');
        $('a.ListExam').addClass('active');
    });

    // Xử lý chuyển tab
    let count = 0;
    let hasTriggered = false;
    function initVisibilityTracking() {
        const handleViolation = () => {
            if (hasTriggered) return; // tránh lặp
            count++;
            alert(`Bạn đã chuyển tab ${count} lần. Quá 3 lần sẽ tự hủy bài thi.`);

            if (count > 3) {
                hasTriggered = true;
                // Gọi xử lý hủy bài thi ở đây (submit bài hoặc redirect)
                alert("Bạn đã vi phạm quá 3 lần. Bài thi sẽ bị hủy.");
            }
        };

        document.addEventListener("visibilitychange", function () {
            if (document.hidden) {
                handleViolation();
            }
        });

        window.addEventListener('blur', function () {
            setTimeout(() => {
                if (document.hidden) return;
                handleViolation();
            }, 100);
        });
    };

    // Vao trang lam bai thi
    $(document).on("click", "button.DoExam", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        $.get(url, function (data) {
            if (data.success === false) {
                showAlert('Đợi xíu!', data.message);
                return;
            }

            $("body").html(data);
        });
    });

    //render cau hoi, ap an
    $(document).on("click", "div.exam-page-question-number", function (e) {

        SaveQuestion();
        $('div.exam-page-question-number').removeClass('current');
        $(this).addClass('current');

        var questionId = $(this).data('quest-id');
        var questionIndex = $(this).data('quest-index');

        renderQuestion(questionId);
        setTimeout(function () {
            const input = document.getElementById("IdCauHoi");
            const stt = document.querySelector('#SttCauHoi');
            if (stt) stt.textContent = `Câu ${questionIndex} `;
            if (input) input.value = questionId;
            else console.warn("Không tìm thấy input #IdCauHoi sau render");
        }, 100);
    });

    //luu cau hoi, ap an dang lam
    function SaveQuestion() {
        const questionId = parseInt($("#IdCauHoi").val());

        const selectedAnswerIds = $(".exam-page-answers input:checked")
            .map(function () {
                return parseInt($(this).val());
            }).get();

        const dataToSend = {
            cauHoiId: questionId,
            dapAnIds: selectedAnswerIds
        };

        $.ajax({
            url: "/student/exam/SaveQuestion",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(dataToSend),
            success: function (res) {
                console.log("Lưu câu hỏi thành công");
            },
            error: function (xhr) {
                console.error("Lỗi khi lưu câu hỏi:", xhr.responseText);
            }
        });
    }

    // Lay json cau hoi tu session
    function renderQuestion(questionId) {
        $.ajax({
            url: "/student/exam/RenderQuestion",
            type: "GET",
            data: { questionId: questionId },
            success: function (res) {

                // Cập nhật nội dung câu hỏi
                document.getElementById('question-content').textContent = res.noiDung +
                    (res.loai === 'NhieuDapAn' ? ' (Có thể chọn nhiều đáp án)' : '');

                // Xác định loại input
                const inputType = res.loai === "TracNghiem" ? "radio" : "checkbox";

                // Xóa đáp án cũ
                const answerDiv = document.getElementById('exam-page-answers');
                answerDiv.innerHTML = '';

                // Render lại các đáp án
                res.dapAnList.forEach((da, index) => {
                    const answerKey = String.fromCharCode(65 + index);
                    const isChecked = res.dapAnIdsDaChon?.includes(da.id);

                    const answerLabel = document.createElement('label');
                    answerLabel.className = 'exam-page-answer-option';

                    answerLabel.innerHTML = `
                    <input type="${inputType}" name="dapan" class="exam-page-answer-radio" value="${da.id}" ${da.isSelected ? 'checked' : ''}>
                    <span class="exam-page-answer-text">${answerKey}. ${da.noiDung}</span>
                `;

                    answerDiv.appendChild(answerLabel);
                });

                // Gán lại IdCauHoi hiện tại
                $("#IdCauHoi").val(res.id);
            },
            error: function (xhr) {
                console.error("Lỗi khi tải câu hỏi:", xhr.responseText);
            }
        });
    }

    // xu ly nop bai, cham diem
    $(document).on("click", "button.exam-page-submit-btn", function () {
        SaveQuestion();

        setTimeout(() => {
            $.ajax({
                url: '/student/exam/SubmitExam',
                type: 'POST',
                success: function (res) {
                    if (res.success) {
                        showAlert('Thành công', res.message, 'success', '/student');
                        console.log(JSON.stringify(res));
                    } else {
                        alert(res.message || "Có lỗi xảy ra.");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Lỗi khi gửi yêu cầu:", error);
                    alert("Không thể nộp bài. Vui lòng thử lại.");
                }
            });
        }), 100;
    });
})