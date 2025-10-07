$(() => {

    // Hiển thị thông báo swal
    function showAlert(title, message, icon = "info") {
        Swal.fire({
            title: title,
            text: message,
            icon: icon,
            confirmButtonColor: "#0963a3"
        });
    }

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
    function createQuestionItem(idx) {
        const div = $(`
   <div class="manual-question question-item" style="border:1px solid #eee; padding:1rem; margin-bottom:1rem; border-radius:6px;">
       <div class="form-group">
           <label>Câu hỏi ${idx + 1}:</label>
           <input type="text" name="question-${idx}" class="question-content answer-input" placeholder="Nhập nội dung câu hỏi...">
       </div>

       <div class="form-row" style="display:flex; gap:1rem;">
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án A:</label>
               <input type="text" name="answerA-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án B:</label>
               <input type="text" name="answerB-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án C:</label>
               <input type="text" name="answerC-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án D:</label>
               <input type="text" name="answerD-${idx}" class="answer-input">
           </div>
       </div>

       <div class="form-group">
           <label>Đáp án đúng:</label>
           <div class="correct-answers">
               <label>
                   <input type="checkbox" name="correct-${idx}" value="A" class="answer-checkbox"> A
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="B" class="answer-checkbox"> B
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="C" class="answer-checkbox"> C
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="D" class="answer-checkbox"> D
               </label>
           </div>
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
                    if (data.trim() === "") {
                        alert("Định dạng file lỗi hoặc không hỗ trợ, vui lòng thử lại!");
                        return;
                    }
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

    function resetExamForm() {
        const form = document.getElementById('exam-form');
        if (form) {
            form.reset();

            // Xóa hết câu hỏi động nếu có
            const questionsList = document.getElementById('questions-list');
            if (questionsList) questionsList.innerHTML = '';

            // Reset input file nếu có
            const examFile = document.getElementById('exam-file');
            if (examFile) examFile.value = '';

            // Focus lại vào tiêu đề
            const tieuDe = document.getElementById('TieuDe');
            if (tieuDe) tieuDe.focus();
        }
    }
    function isDate(inputId, errorId) {
        const input = document.getElementById(inputId);
        const error = document.getElementById(errorId);
        const value = input.value.trim();

        if (value === "") {
            error.innerText = "Ngày không được để trống.";
            return false;
        }

        const parts = value.split("/");
        if (parts.length !== 3) {
            error.innerText = " Vui lòng nhập đúng định dạng dd/mm/yyyy.";
            return false;
        }

        let [dayStr, monthStr, yearStr] = parts;

        const day = parseInt(dayStr, 10);
        const month = parseInt(monthStr, 10);
        const year = parseInt(yearStr, 10);

        if (day < 1 || day > 31 || month < 1 || month > 12 || year < 1900) {
            error.innerText = " Ngày, tháng, năm không hợp lệ.";
            return false;
        }
        // Kiểm tra xem ngày, tháng, năm có phải là số không

        if (isNaN(day) || isNaN(month) || isNaN(year)) {
            error.innerText = " Ngày, tháng, năm phải là số.";
            return false;
        }

        const testDate = new Date(`${month}/${day}/${year}`);
        if (isNaN(testDate.getTime())) {
            error.innerText = " Ngày tháng năm không hợp lệ.";
            return false;
        }

        if (
            testDate.getDate() !== day ||
            testDate.getMonth() + 1 !== month ||
            testDate.getFullYear() !== year
        ) {
            error.innerText = " Ngày không tồn tại.";
            return false;
        }

        const currentYear = new Date().getFullYear();
        if (year > currentYear + 10) {
            error.innerText = ` Năm không được lớn hơn ${currentYear + 10}.`;
            return false;
        }
        // ✅ Format lại input thành dd/mm/yyyy có đủ 2 chữ số
        const formatted = `${String(day).padStart(2, '0')}/${String(month).padStart(2, '0')}/${year}`;
        input.value = formatted;

        error.innerText = "";
        return true;
    }


    document.addEventListener('click', function (e) {
        if (e.target.closest('.btnHuyDeThi')) {
            resetExamForm();
            e.preventDefault();
        }
    });

    // Xử lý hiện/ẩn modal confirm
    function hideModalById(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
        }
    }

    function showModalById(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'flex';
        }
    }

    // Hàm chuyển chuỗi ngày và giờ thành đối tượng Date
    function parseDateTime(dateStr, timeStr) {
        const parts = dateStr.trim().split("/");
        if (parts.length !== 3) return null;

        const day = parseInt(parts[0], 10);
        const month = parseInt(parts[1], 10) - 1; // Tháng tính từ 0
        const year = parseInt(parts[2], 10);

        const [hour, minute] = timeStr.trim().split(":").map(Number);
        if (isNaN(day) || isNaN(month) || isNaN(year) || isNaN(hour) || isNaN(minute)) return null;

        return new Date(year, month, day, hour, minute);
    }

    // Xử lý submit
    let pendingSubmit = false;
    $(document).on('submit', '#exam-form', function (e) {
        e.preventDefault();

        //Kiểm tra dữ liệu ở đây
        $('.errMessage, .text-danger').text('');
        let hasError = false;
        let firstErrorElement = null;

        // 1. Kiểm tra Tên đề thi
        const tieuDe = $('#TieuDe').val().trim();
        if (!tieuDe) {
            $('#errTenDeThi').text('Vui lòng nhập tên đề thi.');
            hasError = true;
            firstErrorElement = firstErrorElement || $('#TieuDe');
        }

        // 2. Kiểm tra Mã đề thi
        const maDe = $('#MaDe').val().trim();
        if (!maDe) {
            $('#errMaDeThi').text('Vui lòng nhập mã đề thi.');
            hasError = true;
            firstErrorElement = firstErrorElement || $('#MaDe');
        }
        // 3. Kiểm tra phương thức tạo đề thi
        const method = $('input[name="exam-create-method"]:checked').val();
        const soCauHoi = $('.question-item').length;

        if (!method) {
            $('#errFile').text('Vui lòng chọn phương thức tạo đề thi.');
            hasError = true;
        } else if (method === 'upload') {
            const file = $('#exam-file')[0].files[0];
            if (!file) {
                $('#errFile').text('Vui lòng chọn file đề thi.');
                hasError = true;
                firstErrorElement = firstErrorElement || $('#exam-file');
            } else if (!file.name.endsWith('.doc') && !file.name.endsWith('.docx')) {
                $('#errFile').text('Chỉ chấp nhận file .doc hoặc .docx.');
                hasError = true;
                firstErrorElement = firstErrorElement || $('#exam-file');
            }
        }
        else if (method === 'manual') {
            $('#errDeThiFile').text('');

            $(".question-item").each(function () {
                const qText = $(this).find(".question-content").val()?.trim();
                if (!qText) {
                    $(this).remove();
                }
            });

            const soCauHoi = $('.question-item').length;
            const soCauHoiNganHang = $(".qb-item input[type='checkbox']:checked").length;

            if (soCauHoi === 0 && soCauHoiNganHang === 0) {
                $('#errDeThiFile').text('Vui lòng thêm ít nhất một câu hỏi.');
                hasError = true;
            } else {
                let hasEmptyField = false;

                $('.question-item').each(function (index) {
                    const questionText = $(this).find('.question-content').val()?.trim();
                    const answers = $(this).find('.answer-input');
                    const correctChecked = $(this).find('.answer-checkbox:checked');

                    // Kiểm tra nội dung câu hỏi
                    if (!questionText) {
                        $('#errDeThiFile').text(`Câu hỏi số ${index + 1} chưa có nội dung.`);
                        $(this).find('.question-content').focus();
                        hasEmptyField = true;
                        return false;
                    }

                    // Kiểm tra các đáp án
                    let missingAnswer = false;
                    answers.each(function () {
                        if (!$(this).val()?.trim()) {
                            missingAnswer = true;
                            $(this).focus();
                            return false;
                        }
                    });

                    if (missingAnswer) {
                        $('#errDeThiFile').text(`Câu hỏi số ${index + 1} thiếu nội dung đáp án.`);
                        hasEmptyField = true;
                        return false;
                    }

                    // Kiểm tra đáp án đúng (checkbox phải được chọn)
                    if (correctChecked.length === 0) {
                        $('#errDeThiFile').text(`Câu hỏi số ${index + 1} chưa chọn đáp án đúng.`);
                        $(this).find('.answer-checkbox').first().focus();
                        hasEmptyField = true;
                        return false;
                    }
                });

                if (hasEmptyField) {
                    hasError = true;
                }
            }
        }


        // 4. Kiểm tra ngày giờ bắt đầu/kết thúc
        // Hàm kiểm tra ngày hợp lệ định dạng dd/mm/yyyy

        // Kiểm tra ngày bắt đầu
        if (!isDate('start-date', 'errStartDate')) {
            hasError = true;
            firstErrorElement = firstErrorElement || $('#start-date');
        }

        // Kiểm tra ngày kết thúc
        if (!isDate('end-date', 'errEndDate')) {
            hasError = true;
            firstErrorElement = firstErrorElement || $('#end-date');
        }
        // Lấy giá trị từ input
        const startDate = $('#start-date').val();
        const startTime = $('#start-time').val();
        const endDate = $('#end-date').val();
        const endTime = $('#end-time').val();
        const now = new Date();
        // Kiểm tra giờ bắt đầu
        if (!startTime) {
            $('#errStartTime').text(' Vui lòng nhập giờ bắt đầu.');
            hasError = true;
            firstErrorElement = firstErrorElement || $('#start-time');
        } else {
            $('#errStartTime').text('');
        }

        // Kiểm tra giờ kết thúc
        if (!endTime) {
            $('#errEndTime').text(' Vui lòng nhập giờ kết thúc.');
            hasError = true;
            firstErrorElement = firstErrorElement || $('#end-time');
        } else {
            $('#errEndTime').text('');
        }

        // Nếu không có lỗi ban đầu, tiến hành so sánh ngày giờ
        const gioBD = parseDateTime(startDate, startTime);
        const gioKT = parseDateTime(endDate, endTime);
        if (!hasError) {


            if (!gioBD || !gioKT || isNaN(gioBD.getTime()) || isNaN(gioKT.getTime())) {
                $('#errEndTime').text(' Ngày giờ không hợp lệ.');
                hasError = true;
            } else {
                if (gioBD < now) {
                    $('#errStartDate').text(' Thời gian bắt đầu phải lớn hơn hiện tại.');
                    hasError = true;
                }
                if (gioKT <= gioBD) {
                    $('#errEndTime').text(' Thời gian kết thúc phải sau thời gian bắt đầu.');
                    hasError = true;
                }
            }
        }


        // 5. Kiểm tra Số điểm tối đa
        const diemToiDa = $('#DiemToiDa').val();
        if (!diemToiDa || parseInt(diemToiDa) < 1) {
            $('#errMaxScore').text('Vui lòng nhập số điểm tối đa hợp lệ (ít nhất 1).');
            hasError = true;
            firstErrorElement = firstErrorElement || $('#DiemToiDa');
        }
        // Kiểm tra khi người dùng gõ vào ô ThoiGian
        // Khi gõ:
        const thoiGianLamBai = $('#ThoiGian').val().trim();
        function validateExamTime(gioBD, gioKT, showError = true) {

            if (thoiGianLamBai === '') {
                if (showError) $('#errTime').text('Vui lòng nhập thời gian làm bài');
                return false;
            }

            const intValue = parseInt(thoiGianLamBai, 10);
            if (isNaN(intValue) || intValue < 1) {
                if (showError) {
                    $('#ThoiGian').val('');
                    $('#errTime').text('Thời gian làm bài phải lớn hơn 0 phút.');
                }
                return false;
            } else if (gioKT && gioBD && intValue > ((gioKT.getTime() - gioBD.getTime()) / (1000 * 60))) {
                if (showError) {
                    $('#errTime').text('Thời gian làm bài không được lớn hơn thời gian kết thúc - bắt đầu.');
                }
                return false;
            } else {
                if (showError) $('#errTime').text('');
                return true;
            }
        }
        // Gọi khi nhập vào input:
        $('#ThoiGian').on('input', function () {
            validateExamTime(gioBD, gioKT); // Kiểm tra hợp lệ nếu có
        });

        // Gọi khi submit:
        if (!validateExamTime(gioBD, gioKT, true)) {
            hasError = true;
            firstErrorElement = firstErrorElement || $('#ThoiGian');
        }
        // Cuộn đến lỗi đầu tiên nếu có
        if (hasError) {
            if (firstErrorElement && firstErrorElement.length) {
                $('html, body').animate({
                    scrollTop: firstErrorElement.offset().top - 100
                }, 500, function () {
                    firstErrorElement.focus();
                });
            }
            return;
        }

        // ✅ Không có lỗi, hiển thị modal
        $('#modal-exam-info').html(`
        <div class= 'modal-info-row'><span class='modal-info-label'>Tiêu đề:</span><span>${tieuDe}</span></div >
        <div class='modal-info-row'><span class='modal-info-label'>Mã đề:</span><span>${maDe}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Giờ bắt đầu:</span><span>${startDate} ${startTime}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Giờ kết thúc:</span><span>${endDate} ${endTime}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Thời gian làm bài:</span><span>${thoiGianLamBai} phút</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Điểm tối đa:</span><span>${diemToiDa}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Random câu hỏi:</span><span>${$('#RandomCauHoi').is(':checked') ? '✅' : '❌'}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Random đáp án:</span><span>${$('#RandomDapAn').is(':checked') ? '✅' : '❌'}</span></div>
        <div class='modal-info-row'><span class='modal-info-label'>Hiển thị kết quả:</span><span>${$('#ShowKQ').is(':checked') ? '✅' : '❌'}</span></div>

    `);

        showModalById('confirm-modal');
    });

    // DOM delegated cho các nút trong modal
    document.addEventListener('click', function (e) {
        const target = e.target;

        if (target.closest('#modal-confirm-btn')) {

            hideModalById('confirm-modal');

            pendingSubmit = true;
            const form = document.getElementById('exam-form');
            if (form) form.requestSubmit();
            pendingSubmit = false;
        }

        if (target.closest('#modal-cancel-btn')) {
            hideModalById('confirm-modal');
        }
    });

    //xử lý tạo đề thi hoàn chỉnh
    function createQuestionItem(idx) {
        const div = $(`
   <div class="manual-question question-item" style="border:1px solid #eee; padding:1rem; margin-bottom:1rem; border-radius:6px;">
       <div class="form-group">
           <label>Câu hỏi ${idx + 1}:</label>
           <input type="text" name="question-${idx}" class="question-content answer-input" placeholder="Nhập nội dung câu hỏi...">
       </div>

       <div class="form-row" style="display:flex; gap:1rem;">
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án A:</label>
               <input type="text" name="answerA-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án B:</label>
               <input type="text" name="answerB-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án C:</label>
               <input type="text" name="answerC-${idx}" class="answer-input">
           </div>
           <div class="answer-row form-group" style="flex:1;">
               <label>Đáp án D:</label>
               <input type="text" name="answerD-${idx}" class="answer-input">
           </div>
       </div>

       <div class="form-group">
           <label>Đáp án đúng:</label>
           <div class="correct-answers">
               <label>
                   <input type="checkbox" name="correct-${idx}" value="A" class="answer-checkbox"> A
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="B" class="answer-checkbox"> B
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="C" class="answer-checkbox"> C
               </label>
               <label>
                   <input type="checkbox" name="correct-${idx}" value="D" class="answer-checkbox"> D
               </label>
           </div>
       </div>

       <button type="button" class="btn secondary remove-question-btn">Xóa câu hỏi</button>
   </div>
   `);
        return div;
    }

    //xu ly load ngan hang cau hoi theo chu de
    $(document).on("change", "#qb-topic", function () {
        let selectedId = $(this).val();

        $.ajax({
            url: '/teacher/exam/LoadQuestionFromBank',
            type: 'GET',
            data: {
                chuDeId: selectedId === 0 ? null : selectedId
            },
            success: function (res) {
                $(".listQuestionFromBank").html(res);
            },
            error: function () {
                alert("Lỗi khi load câu hỏi!");
            }
        });
    });

    //thao tac voi tao de bang ngan hang cau hoi
    $(document).on("click", "button.btnUncheckQB", function () {
        $(".qb-chkAdd").prop("checked", false);
        $("#qb-count").text("Đã chọn: 0");
        $(".chosenQuestionNumber").text(0);
    });

    //dem so cau hoi duoc check trong ngan hang cau hoi
    $(document).on("change", ".qb-chkAdd", function () {
        let count = $(".qb-chkAdd:checked").length;
        $("#qb-count").text("Đã chọn: " + count);
        $(".chosenQuestionNumber").text(count);
    });

    //xu ly submit de thi
    $(document).on('click', '#modal-confirm-btn', function (e) {

        e.preventDefault();

        const fd = new FormData();

        const giangVienId = $('#IdGiangVien').val();
        fd.append('IdGiangVien', giangVienId);

        $('[name]').each(function () {
            const name = $(this).attr('name');
            const type = $(this).attr('type');

            if (name === 'exam-create-method' && type === 'radio') {
                return;
            }
            if (name === 'DiemToiDa') {
                return;
            }
            if (type === 'file') {
                const files = $(this)[0].files;
                if (files.length > 0) {
                    fd.append(name, files[0]);
                }
            } else if (type === 'checkbox') {
                return;
            } else {
                fd.append(name, $(this).val());
            }
        });
        const startDate = $('#start-date').val();
        const startTime = $('#start-time').val();
        const endDate = $('#end-date').val();
        const endTime = $('#end-time').val();
        const randomCauHoi = $('#RandomCauHoi').prop('checked');
        const randomDapAn = $('#RandomDapAn').prop('checked');
        const showKQ = $('#ShowKQ').prop('checked');
        const gioBD = parseDateTime(startDate, startTime);
        const gioKT = parseDateTime(endDate, endTime);
        const diemToiDa = parseFloat($('#DiemToiDa').val());

        fd.append('GioBD', gioBD.toISOString());
        fd.append('GioKT', gioKT.toISOString());
        fd.append('DiemToiDa', isNaN(diemToiDa) ? '0' : diemToiDa.toString());
        fd.append('RandomCauHoi', $('#RandomCauHoi').is(':checked') ? 'true' : 'false');
        fd.append('RandomDapAn', $('#RandomDapAn').is(':checked') ? 'true' : 'false');
        fd.append('ShowKQ', $('#ShowKQ').is(':checked') ? 'true' : 'false');

        const cauHoiObj = [];
        $('.question-item, .qb-item').each(function (i) {
            const isFile = $(this).find('#contentQuestion').length > 0;
            const isBank = $(this).hasClass("qb-item");

            if (isFile) {
                const noiDungCH = $(this).find('#contentQuestion').val()?.split('. ').slice(1).join('. ').trim() || '';
                const isMultipleChoice = $(this).find('#questionType').prop('checked');

                const cauHoi = {
                    NoiDung: noiDungCH,
                    Loai: isMultipleChoice ? "NhieuDapAn" : "TracNghiem",
                    DapAnList: []
                };

                $(this).find('.answer-row').each(function () {
                    const answerText = $(this).find('.answer-input').val()?.split('. ').slice(1).join('. ').trim() || '';
                    const isCorrect = $(this).find('.answer-checkbox').prop('checked');

                    cauHoi.DapAnList.push({
                        NoiDung: answerText,
                        DungSai: isCorrect
                    });
                });

                cauHoiObj.push(cauHoi);
            }
            else {
                if ($(this).hasClass("qb-item")) {
                    const isChecked = $(this).find("input[type='checkbox']").prop("checked");
                    if (!isChecked) return;

                    const noiDungCH = $(this).find('.qb-question').text().trim();

                    const cauHoi = {
                        NoiDung: noiDungCH,
                        Loai: $(this).find('.dapAnDung').length > 1 ? "NhieuDapAn" : "TracNghiem",
                        DapAnList: []
                    };

                    $(this).find('.qb-answers div').each(function () {
                        let fullText = $(this).text().trim();
                        let answerText = fullText.replace(/^([A-D]\. ?)/, '').replace(/\(Đúng\)/, '').trim();
                        const isCorrect = $(this).hasClass("dapAnDung");

                        cauHoi.DapAnList.push({
                            NoiDung: answerText,
                            DungSai: isCorrect
                        });
                    });

                    cauHoiObj.push(cauHoi);
                }

                // Nếu là câu hỏi tạo thủ công
                else if ($(this).hasClass("question-item")) {
                    const cauHoi = {
                        NoiDung: $(this).find('.question-content').val()?.trim() || '',
                        Loai: $(this).find('.correct-answers input:checked').length > 1 ? "NhieuDapAn" : "TracNghiem",
                        DapAnList: []
                    };

                    ["A", "B", "C", "D"].forEach(opt => {
                        const answer = $(this).find(`input[name='answer${opt}-${i}']`).val()?.trim();
                        const isCorrect = $(this).find(`input[name='correct-${i}'][value='${opt}']`).prop('checked');
                        if (answer) {
                            cauHoi.DapAnList.push({
                                NoiDung: answer,
                                DungSai: isCorrect
                            });
                        }
                    });

                    cauHoiObj.push(cauHoi);
                }
            }
        });

        const soCauHoi = $('.question-item').length + $(".qb-item input[type='checkbox']:checked").length;
        fd.append("SoCauHoi", soCauHoi);

        fd.append("cauHoiObj", JSON.stringify(cauHoiObj));

        //for (const [key, value] of fd.entries()) {
        //    console.log(`${key}: ${value}`);
        //}

        // Hiển thị thông báo swal
        function showAlert(title, message, icon = "info") {
            Swal.fire({
                title: title,
                text: message,
                icon: icon,
                confirmButtonColor: "#0963a3"
            });
        }

        // Gửi về server
        $.ajax({
            url: '/teacher/exam/CreateExam',
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data) {
                    hideModalById('confirm-modal');
                    showAlert('Thành công!', 'Tạo đề thi thành công', 'success');
                    resetExamForm();
                }
                else {
                    showAlert("Thất bại", "Tạo đề thi thất bại, thầy/cô vui lòng xem lại định dạng file đề thi.", "error");
                }
            },
            error: function (xhr) {
                console.error("Lỗi:", xhr.responseText);
                alert("Gửi thất bại: " + xhr.responseText);
            }
        });
    });
});