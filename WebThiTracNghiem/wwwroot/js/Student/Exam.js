$(() => {
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
            $("body").html(data);
            $(document).on("click", "div.exam-page-question-number", function (e) {
                $('div.exam-page-question-number').removeClass('current');
                $(this).addClass('current');
                var questIndex = $(this).data('quest-index') - 1;
                getQuestionObj(questIndex);
            });
            //initVisibilityTracking();
        });
    });

    // Lay json cau hoi tu session
    function getQuestionObj(index) {
        $.ajax({
            url: '/student/exam/LoadQuestion',
            type: 'GET',
            data: { index: index },
            success: function (res) {
                if (res) {
                    renderQuestion(res);
                } else {
                    console.warn("Không tìm thấy câu hỏi.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Lỗi khi lấy câu hỏi:", error);
                console.log("Chi tiết:", xhr.responseText);
                alert("Lỗi khi lấy câu hỏi.");
            }
        });
    }

    function renderQuestion(res) {
        document.getElementById('question-content').textContent = res.noiDung + ` ${res.loai === 'NhieuDapAn' ? '(Có thể chọn nhiều đáp án)' : ''}`;

        const answerDiv = document.getElementById('exam-page-answers');
        answerDiv.innerHTML = '';

        const inputType = res.loai === "TracNghiem" ? "radio" : "checkbox";

        res.dapAnList.forEach((da, index) => {
            const answerKey = String.fromCharCode(65 + index);
            const answerLabel = document.createElement('label');
            answerLabel.className = 'exam-page-answer-option';

            answerLabel.innerHTML = `
                <input type="${inputType}" name="dapan" class="exam-page-answer-radio">
		        <span class="exam-page-answer-text" data-id-dapan="${da.id}">${answerKey}. ${da.noiDung}</span>
            `;
            answerDiv.appendChild(answerLabel);
        });
    }
})