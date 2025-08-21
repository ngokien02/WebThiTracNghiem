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
    let isHiddenHandled = false;

    function initVisibilityTracking() {
        const handleViolation = () => {
            if (hasTriggered || isHiddenHandled) return; // tránh gọi lặp

            isHiddenHandled = true; // Đánh dấu đã xử lý lần ẩn này
            count++;
            alert(`⚠️ Bạn đã chuyển tab ${count} lần. Quá 3 lần sẽ tự hủy bài thi.`);

            if (count > 3) {
                hasTriggered = true;
                alert("🚫 Bạn đã vi phạm quá 3 lần. Bài thi sẽ bị hủy.");
                // Thực hiện hành động như submit bài hoặc chuyển trang
                // window.location.href = "/Exam/Cancel";
            }
        };

        document.addEventListener("visibilitychange", function () {
            if (document.hidden) {
                handleViolation();
            } else {
                isHiddenHandled = false; // Cho phép lần xử lý tiếp theo
            }
        });

        window.addEventListener("blur", function () {
            setTimeout(() => {
                if (document.hidden) return; // Đã xử lý rồi
                handleViolation();
            }, 100);
        });

        window.addEventListener("focus", () => {
            isHiddenHandled = false; // reset lại sau khi quay lại
        });
    }


    // Vao trang lam bai thi
    $(document).on("click", "button.DoExam", function (e) {
        e.preventDefault();
        initVisibilityTracking();
        var url = $(this).attr("href");

        $.get(url, function (data) {
            if (data.success === false) {
                showAlert('Đợi xíu!', data.message);
                return;
            }

            $("body").html(data);

            const examId = $("input#IdDeThi").val();
            const timeInput = document.getElementById("ExamTime");
            const totalMinute = parseInt(timeInput.value);
            const totalTime = totalMinute * 60;
            const timerKey = `remainingTime_${examId}`;
            const finishKey = `finishTime_ ${examId}`;

            let remainingTime = localStorage.getItem(timerKey) ? parseInt(localStorage.getItem(timerKey)) : totalTime;

            const timerDisplay = document.getElementById('exam-timer');
            let countdownInterval;

            function formatTime(seconds) {
                const m = String(Math.floor(seconds / 60)).padStart(2, '0');
                const s = String(seconds % 60).padStart(2, '0');
                return `${m} : ${s}`;
            }

            function startCountdown() {
                timerDisplay.textContent = formatTime(remainingTime);

                countdownInterval = setInterval(() => {
                    remainingTime--;
                    timerDisplay.textContent = formatTime(remainingTime);

                    localStorage.setItem(timerKey, remainingTime);

                    if (remainingTime <= 0) {
                        clearInterval(countdownInterval);
                        localStorage.removeItem(timerKey);
                        handleTimeUp();
                    }
                }, 1000);
            }

            function handleTimeUp() {
                showAlert('Hết giờ', 'Đã hết giờ làm bài, hệ thống sẽ tự động nộp bài!');
                handleSubmitExam();
            }

            startCountdown();
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

    //button dieu huong trai phai
    $(document).on("click", "button.exam-page-nav-btn", function (e) {

        var direct = $(this).attr("direct");

        if (direct === "next") {
            handleDirectButtons("next");
        } else {
            handleDirectButtons("prev");
        }
    });

    //ham xu ly dieu huong
    function handleDirectButtons(direct) {

        document.querySelectorAll(".exam-page-question-number").forEach(div => {
            if (div.classList.contains("current")) {

                var directDiv = direct === "next" ? $(div).next(".exam-page-question-number") : $(div).prev(".exam-page-question-number");
                var directId = directDiv.data("quest-id");
                var directIndex = directDiv.data("quest-index");

                $(div).removeClass("current");
                if (directDiv.length === 0) {
                    return;
                }
                renderQuestion(directId);
                $("#QuestionIndex").val(directIndex);
            }
        });
        var sttCauHoi = parseInt($("#QuestionIndex").val());
        $("div.exam-page-question-number").each(function () {
            let value = parseInt($(this).text().trim());
            if (value == sttCauHoi) {
                $(this).addClass("current");
            }
        });
        SaveQuestion();
    }

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
    document.addEventListener('click', function (e) {
        if (e.target.closest('button.exam-page-submit-btn')) {
            handleSubmitExam();
        }
    });

    function handleSubmitExam() {
        SaveQuestion();

        setTimeout(() => {
            $.ajax({
                url: '/student/exam/SubmitExam',
                type: 'POST',
                success: function (res) {
                    if (res.success) {
                        showExamResultModal(res);
                    } else {
                        alert(res.message || "Có lỗi xảy ra.");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Lỗi khi gửi yêu cầu:", error);
                    alert("Không thể nộp bài. Vui lòng thử lại.");
                }
            });
        }, 200);
    }

    // Hàm hiển thị modal kết quả thi
    function showExamResultModal(examData) {
        // Cập nhật nội dung modal với dữ liệu từ server
        document.getElementById('examResultExamName').textContent = examData.title;
        document.getElementById('examResultStudentName').textContent = examData.username;
        document.getElementById('examResultCompletionTime').textContent = examData.timeDone;
        document.getElementById('examResultCorrectAnswers').textContent = examData.correctAnswers || 'Chưa có';
        document.getElementById('examResultScore').textContent = examData.score || 'Chưa có';

        // Cập nhật thông điệp kết quả
        const score = examData.score || 8.0;
        const resultMessage = getResultMessage(score);
        const scoreClass = getScoreClass(score);

        const resultMessageElement = document.getElementById('examResultMessage');
        resultMessageElement.className = `exam-result-message-text ${scoreClass}`;
        resultMessageElement.textContent = resultMessage;

        // Hiển thị modal
        document.getElementById('examResultModalWrapper').classList.add('show');
    }

    // Hàm chuyển hướng về trang chủ
    $(document).on('click', 'button.exam-result-btn-primary', function () {
        window.location.href = '/student';
    });

    // Hàm lấy class CSS dựa trên điểm số
    function getScoreClass(score) {
        if (score >= 8.0) return 'exam-result-excellent';
        if (score >= 6.5) return 'exam-result-good';
        if (score >= 5.0) return 'exam-result-average';
        return 'exam-result-poor';
    }

    // Hàm lấy thông điệp kết quả
    function getResultMessage(score) {
        if (score >= 8.0) return '🎉 Xuất sắc! Bạn đã hoàn thành bài thi rất tốt!';
        if (score >= 6.5) return '👍 Tốt! Bạn đã làm bài thi khá tốt!';
        if (score >= 5.0) return '✅ Đạt! Bạn đã hoàn thành bài thi!';
        return '📚 Cần cố gắng thêm! Hãy ôn tập lại kiến thức!';
    }
})