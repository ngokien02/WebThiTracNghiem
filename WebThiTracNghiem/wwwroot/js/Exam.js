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
                    alert("Lỗi xử lý file, vui lòng thử lại!");
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
})