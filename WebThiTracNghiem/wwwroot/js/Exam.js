$(() => {
    // Hien thi trang tao de thi
    $(document).on("click", "a.CreateExam", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        $.get(url, function (data) {
            $(".main-content").html(data);
        })
    })

    // Chuyển đổi giữa upload file và tạo thủ công
    $(document).on('change', 'input[name="exam-create-method"]', function () {
        if ($(this).val() === 'upload') {
            $('#upload-section').show();
            $('#manual-section').hide();
        } else {
            $('#upload-section').hide();
            $('#manual-section').show();
        }
    });

    // Thêm/xóa câu hỏi thủ công
    let questionCount = 0;

    function createQuestionItem(idx) {
        const div = $(`
        <div class="manual-question" style="border:1px solid #eee;padding:1rem;margin-bottom:1rem;border-radius:6px;">
            <div class="form-group">
                <label>Câu hỏi ${idx + 1}:</label>
                <input type="text" name="question-${idx}" required placeholder="Nhập nội dung câu hỏi...">
            </div>
            <div class="form-row" style="display:flex; gap:1rem;">
                <div class="form-group" style="flex:1;">
                    <label>Đáp án A:</label>
                    <input type="text" name="answerA-${idx}" required>
                </div>
                <div class="form-group" style="flex:1;">
                    <label>Đáp án B:</label>
                    <input type="text" name="answerB-${idx}" required>
                </div>
                <div class="form-group" style="flex:1;">
                    <label>Đáp án C:</label>
                    <input type="text" name="answerC-${idx}" required>
                </div>
                <div class="form-group" style="flex:1;">
                    <label>Đáp án D:</label>
                    <input type="text" name="answerD-${idx}" required>
                </div>
            </div>
            <div class="form-group">
                <label>Đáp án đúng:</label>
                <select name="correct-${idx}" required>
                    <option value="A">A</option>
                    <option value="B">B</option>
                    <option value="C">C</option>
                    <option value="D">D</option>
                </select>
            </div>
            <button type="button" class="btn secondary remove-question-btn">Xóa câu hỏi</button>
        </div>
    `);
        return div;
    }

    function updateQuestionLabels() {
        $('#questions-list .manual-question').each(function (idx) {
            $(this).find('label:first').text(`Câu hỏi ${idx + 1}:`);
        });
    }

    $(document).on('click', '#add-question-btn', function () {
        $('#questions-list').append(createQuestionItem($('#questions-list .manual-question').length));
        updateQuestionLabels();
    });

    $(document).on('click', '.remove-question-btn', function () {
        $(this).closest('.manual-question').remove();
        updateQuestionLabels();
    });

    // Modal xử lý sau khi upload file
    $(document).on('change', '#exam-file', function (e) {
        const loadingOverlay = $('#loading-overlay');
        const questionModal = $('#question-modal');
        const modalQuestionList = $('#modal-question-list');
        const fileInput = e.target;
        const file = fileInput.files[0];

        if (!file) return;
        // Kiểm tra định dạng
        const allowedTypes = ['application/pdf',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'application/msword'];
        if (!allowedTypes.includes(file.type)) {
            alert('Chỉ hỗ trợ file .doc, .docx hoặc .pdf');
            return;
        }
        const formData = new FormData();
        formData.append('examFile', file);

        loadingOverlay.show();
        setTimeout(() => {
            loadingOverlay.hide();
            $.ajax({
                url: 'teacher/exam/xulyupload',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    modalQuestionList.html(data);
                    questionModal.css('display', 'flex');
                },
                error: function () {
                    alert("Định dạng file lỗi hoặc không hỗ trợ, vui lòng thử lại!");
                }
            });
        }, 2000);
    });
    // Event delegation cho nút "Chấp nhận"
    $(document).on('click', '#modal-accept-btn', function () {
        $('#question-modal').hide();
        // Xử lý sau khi chấp nhận
    });

    // Event delegation cho nút "Hủy"
    $(document).on('click', '#modal-cancel-btn', function () {
        $('#question-modal').hide();
        // Xử lý sau khi hủy
    });

    // Xử lý nút Huỷ bỏ trong form tạo đề thi
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btnHuyDeThi')) {
            const form = document.getElementById('exam-form');
            if (form) {
                form.reset();
                // Nếu có phần nhập thủ công, xóa hết câu hỏi động
                const questionsList = document.getElementById('questions-list');
                if (questionsList) questionsList.innerHTML = '';
                // Nếu có upload file, reset input file
                const examFile = document.getElementById('exam-file');
                if (examFile) examFile.value = '';
                // Focus lại vào tiêu đề
                const tieuDe = document.getElementById('TieuDe');
                if (tieuDe) tieuDe.focus();
            }
            e.preventDefault();
        }
    });

    let pendingSubmit = false;

    document.addEventListener('submit', function (e) {
        const form = e.target;

        if (form && form.matches('#exam-form') && !pendingSubmit) {
            e.preventDefault();

            // Sinh mã đề ngẫu nhiên (6 ký tự)
            const examCode = 'MD' + Math.random().toString(36).substring(2, 8).toUpperCase();

            // Lấy ngày và giờ từ input
            const startDate = document.getElementById('start_date')?.value || '';
            const startTime = document.getElementById('start_time')?.value || '';
            const endDate = document.getElementById('end_date')?.value || '';
            const endTime = document.getElementById('end_time')?.value || '';
            const startDateTime = `${startDate} ${startTime}`;
            const endDateTime = `${endDate} ${endTime}`;

            // Lấy các giá trị khác từ form
            const totalQuestions = form.querySelector('[name="total_questions"]')?.value || '';
            const maxScore = 10;
            const shuffleQuestions = form.querySelector('[name="RandomCauHoi"]')?.checked;
            const shuffleAnswers = form.querySelector('[name="RandomDapAn"]')?.checked;
            const showResults = form.querySelector('[name="ShowKQ"]')?.checked;

            // Hiển thị thông tin lên modal
            const modalExamInfo = document.getElementById('modal-exam-info');
            if (modalExamInfo) {
                modalExamInfo.innerHTML = `
                <div class='modal-info-row'><span class='modal-info-label'>Mã đề:</span><span class='modal-info-value'>${examCode}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Giờ bắt đầu:</span><span class='modal-info-value'>${startDateTime}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Giờ kết thúc:</span><span class='modal-info-value'>${endDateTime}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Tổng số câu hỏi:</span><span class='modal-info-value'>${totalQuestions}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Điểm tối đa:</span><span class='modal-info-value'>${maxScore}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Random câu hỏi:</span><span class='modal-info-value'>${shuffleQuestions ? '✅' : '❌'}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Random đáp án:</span><span class='modal-info-value'>${shuffleAnswers ? '✅' : '❌'}</span></div>
                <div class='modal-info-row'><span class='modal-info-label'>Hiển thị kết quả:</span><span class='modal-info-value'>${showResults ? '✅' : '❌'}</span></div>
            `;
            }

            const confirmModal = document.getElementById('confirm-modal');
            if (confirmModal) confirmModal.style.display = 'flex';
        }
    });

    // DOM delegated cho các nút trong modal
    document.addEventListener('click', function (e) {
        const target = e.target;

        if (target.closest('#modal-confirm-btn')) {
            const confirmModal = document.getElementById('confirm-modal');
            if (confirmModal) confirmModal.style.display = 'none';

            pendingSubmit = true;
            const form = document.getElementById('exam-form');
            if (form) form.requestSubmit();
            pendingSubmit = false;
        }

        if (target.closest('#modal-cancel-btn')) {
            const confirmModal = document.getElementById('confirm-modal');
            if (confirmModal) confirmModal.style.display = 'none';
        }
    });

})