
// Khởi tạo khi trang load xong
document.addEventListener('click', (event) => {
    if (event.target.closest('a.Post')) {
        setTimeout(() => {
            initializeImageUploads();
            initializeFormValidation();
        }, 1000);
    }
});

// Khởi tạo chức năng upload ảnh
function initializeImageUploads() {
    const imageInputs = document.querySelectorAll('.admin-posts-image-input');

    imageInputs.forEach(input => {
        input.addEventListener('change', handleImageUpload);
    });
}

// Xử lý khi user chọn ảnh
function handleImageUpload(event) {
    const file = event.target.files[0];
    const slot = event.target.getAttribute('data-slot');
    const slotElement = document.querySelector(`[data-slot="${slot}"]`);

    if (!file) return;

    // Kiểm tra định dạng file
    if (!isValidImageFile(file)) {
        showError('Chỉ chấp nhận file ảnh JPG, PNG, GIF');
        return;
    }

    // Kiểm tra kích thước file (5MB)
    if (file.size > 5 * 1024 * 1024) {
        showError('Kích thước ảnh không được vượt quá 5MB');
        return;
    }

    // Tạo preview ảnh
    createImagePreview(file, slotElement, slot);
}

// Kiểm tra file ảnh hợp lệ
function isValidImageFile(file) {
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    return validTypes.includes(file.type);
}

// Tạo preview ảnh
function createImagePreview(file, slotElement, slot) {
    const reader = new FileReader();

    reader.onload = function (e) {
        // Xóa placeholder
        const placeholder = slotElement.querySelector('.admin-posts-upload-placeholder');
        if (placeholder) {
            placeholder.remove();
        }

        // Tạo container preview
        const previewContainer = document.createElement('div');
        previewContainer.className = 'admin-posts-image-preview-container';

        // Tạo ảnh preview
        const img = document.createElement('img');
        img.src = e.target.result;
        img.className = 'admin-posts-image-preview';
        img.alt = 'Preview ảnh';

        // Tạo nút xóa
        const removeBtn = document.createElement('button');
        removeBtn.className = 'admin-posts-image-remove-btn';
        removeBtn.innerHTML = '×';
        removeBtn.title = 'Xóa ảnh';
        removeBtn.onclick = function () {
            removeImage(slot);
        };

        // Thêm vào container
        previewContainer.appendChild(img);
        previewContainer.appendChild(removeBtn);

        // Thay thế nội dung slot
        slotElement.innerHTML = '';
        slotElement.appendChild(previewContainer);

        // Thêm class để thay đổi style
        slotElement.classList.add('has-image');

        // Lưu thông tin file
        slotElement.dataset.fileName = file.name;
        slotElement.dataset.fileSize = file.size;
    };

    reader.readAsDataURL(file);
}

// Xóa ảnh
function removeImage(slot) {
    const slotElement = document.querySelector(`[data-slot="${slot}"]`);

    // Tạo lại placeholder
    const placeholder = document.createElement('div');
    placeholder.className = 'admin-posts-upload-placeholder';

    // Tùy chỉnh text theo slot
    let sectionText = '';
    if (slot === 'section1') sectionText = 'cho phần 1';
    else if (slot === 'section2') sectionText = 'cho phần 2';
    else if (slot === 'section3') sectionText = 'cho phần 3';

    placeholder.innerHTML = `
        <i class="fas fa-plus"></i>
        <span>Thêm ảnh ${sectionText}</span>
    `;

    // Thêm lại input file
    const newInput = document.createElement('input');
    newInput.type = 'file';
    newInput.className = 'admin-posts-image-input';
    newInput.setAttribute('data-slot', slot);
    newInput.accept = 'image/*';
    newInput.addEventListener('change', handleImageUpload);

    // Thay thế nội dung
    slotElement.innerHTML = '';
    slotElement.appendChild(placeholder);
    slotElement.appendChild(newInput);

    // Xóa class và data
    slotElement.classList.remove('has-image');
    delete slotElement.dataset.fileName;
    delete slotElement.dataset.fileSize;
}

// Khởi tạo validation form
function initializeFormValidation() {
    const form = document.getElementById('adminPostsForm');
    const inputs = form.querySelectorAll('input, textarea');

    inputs.forEach(input => {
        input.addEventListener('blur', validateField);
        input.addEventListener('input', clearFieldError);
    });
}

// Validate từng field
function validateField(event) {
    const field = event.target;
    const value = field.value.trim();

    // Xóa lỗi cũ
    clearFieldError(event);

    // Kiểm tra required
    if (field.hasAttribute('required') && !value) {
        showFieldError(field, 'Trường này là bắt buộc');
        return false;
    }

    // Kiểm tra độ dài tiêu đề
    if (field.id === 'adminPostsTitle' && value.length < 10) {
        showFieldError(field, 'Tiêu đề phải có ít nhất 10 ký tự');
        return false;
    }

    // Kiểm tra độ dài nội dung
    if (field.id.includes('Content') && value.length < 20) {
        showFieldError(field, 'Nội dung phải có ít nhất 20 ký tự');
        return false;
    }

    return true;
}

// Hiển thị lỗi cho field
function showFieldError(field, message) {
    field.classList.add('error');

    // Tạo message lỗi
    const errorDiv = document.createElement('div');
    errorDiv.className = 'admin-posts-error-message';
    errorDiv.textContent = message;

    // Thêm vào sau field
    field.parentNode.appendChild(errorDiv);
}

// Xóa lỗi của field
function clearFieldError(event) {
    const field = event.target;
    field.classList.remove('error');

    // Xóa message lỗi
    const errorMessage = field.parentNode.querySelector('.admin-posts-error-message');
    if (errorMessage) {
        errorMessage.remove();
    }
}

// Lưu bài viết
function savePost() {
    // Validate tất cả fields
    const form = document.getElementById('adminPostsForm');
    const inputs = form.querySelectorAll('input, textarea');
    let isValid = true;

    inputs.forEach(input => {
        if (!validateField({ target: input })) {
            isValid = false;
        }
    });

    if (!isValid) {
        showError('Vui lòng kiểm tra lại các trường bắt buộc');
        return;
    }

    // Thu thập dữ liệu form
    const formData = collectFormData();

    // Hiển thị loading
    showLoading(true);

    // Giả lập gửi dữ liệu (thay thế bằng API call thực tế)
    setTimeout(() => {
        showLoading(false);
        showSuccessModal();
        console.log('Dữ liệu bài viết:', formData);
    }, 2000);
}

// Thu thập dữ liệu form
function collectFormData() {
    const formData = {
        postTitle: document.getElementById('adminPostsTitle').value.trim(),
        section1: {
            title: document.getElementById('adminPostsSection1Title').value.trim(),
            content: document.getElementById('adminPostsSection1Content').value.trim(),
            image: getImageData('section1')
        },
        section2: {
            title: document.getElementById('adminPostsSection2Title').value.trim(),
            content: document.getElementById('adminPostsSection2Content').value.trim(),
            image: getImageData('section2')
        },
        section3: {
            title: document.getElementById('adminPostsSection3Title').value.trim(),
            content: document.getElementById('adminPostsSection3Content').value.trim(),
            image: getImageData('section3')
        }
    };

    return formData;
}

// Lấy thông tin ảnh của section
function getImageData(sectionSlot) {
    const slotElement = document.querySelector(`[data-slot="${sectionSlot}"]`);

    if (slotElement && slotElement.classList.contains('has-image')) {
        return {
            fileName: slotElement.dataset.fileName,
            fileSize: slotElement.dataset.fileSize,
            hasImage: true
        };
    }

    return {
        hasImage: false
    };
}

// Reset form
function resetForm() {
    if (confirm('Bạn có chắc muốn làm mới form? Tất cả dữ liệu sẽ bị mất.')) {
        const form = document.getElementById('adminPostsForm');
        form.reset();

        // Xóa tất cả ảnh
        const sections = ['section1', 'section2', 'section3'];
        sections.forEach(section => {
            removeImage(section);
        });

        // Xóa tất cả lỗi
        const errorMessages = form.querySelectorAll('.admin-posts-error-message');
        errorMessages.forEach(error => error.remove());

        // Xóa class error
        const errorFields = form.querySelectorAll('.admin-posts-form-control.error');
        errorFields.forEach(field => field.classList.remove('error'));

        showSuccess('Form đã được làm mới');
    }
}

// Hiển thị modal thành công
function showSuccessModal() {
    const modal = document.getElementById('adminPostsSuccessModal');
    modal.style.display = 'block';
}

// Đóng modal thành công
function closeSuccessModal() {
    const modal = document.getElementById('adminPostsSuccessModal');
    modal.style.display = 'none';
}

// Hiển thị loading
function showLoading(show) {
    const form = document.getElementById('adminPostsForm');
    const submitBtn = form.querySelector('button[onclick="savePost()"]');

    if (show) {
        form.classList.add('admin-posts-loading');
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
        submitBtn.disabled = true;
    } else {
        form.classList.remove('admin-posts-loading');
        submitBtn.innerHTML = '<i class="fas fa-save"></i> Lưu bài viết';
        submitBtn.disabled = false;
    }
}

// Hiển thị thông báo lỗi
function showError(message) {
    // Tạo toast notification
    const toast = document.createElement('div');
    toast.className = 'admin-posts-toast admin-posts-toast-error';
    toast.innerHTML = `
        <i class="fas fa-exclamation-circle"></i>
        <span>${message}</span>
    `;

    // Thêm vào body
    document.body.appendChild(toast);

    // Hiển thị
    setTimeout(() => toast.classList.add('show'), 100);

    // Tự động ẩn sau 5 giây
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// Hiển thị thông báo thành công
function showSuccess(message) {
    const toast = document.createElement('div');
    toast.className = 'admin-posts-toast admin-posts-toast-success';
    toast.innerHTML = `
        <i class="fas fa-check-circle"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    setTimeout(() => toast.classList.add('show'), 100);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Chuyển đến danh sách bài viết
function showPostsList() {
    // TODO: Chuyển đến trang danh sách bài viết
    alert('Chức năng này sẽ được phát triển sau');
}

// Đóng modal khi click bên ngoài
window.addEventListener('click', function (event) {
    const modal = document.getElementById('adminPostsSuccessModal');
    if (event.target === modal) {
        modal.style.display = 'none';
    }
});

// Xử lý phím tắt
document.addEventListener('keydown', function (event) {
    // Ctrl + S để lưu
    if (event.ctrlKey && event.key === 's') {
        event.preventDefault();
        savePost();
    }

    // Esc để đóng modal
    if (event.key === 'Escape') {
        const modal = document.getElementById('adminPostsSuccessModal');
        if (modal.style.display === 'block') {
            modal.style.display = 'none';
        }
    }
});
