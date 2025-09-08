
$(document).on("submit", "#userForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: "/admin/user/createuser",
        type: "POST",
        data: $(this).serialize(),
        success: function (res) {
            if (res.success) {
                alert(res.message);
            } else {
                alert(res.message);
            }
        },
        error: function () {
            alert("Lỗi không xác định");
        }
    })
})




// ====== Filter & Form Elements ======
let roleFilter, statusFilter, searchInput, selectAllCheckbox;

// ====== DOM Ready ======
document.addEventListener('DOMContentLoaded', () => {
    roleFilter = document.getElementById('roleFilter');
    statusFilter = document.getElementById('statusFilter');
    searchInput = document.getElementById('searchInput');
    selectAllCheckbox = document.getElementById('selectAll');

    roleFilter?.addEventListener('change', filterUsers);
    statusFilter?.addEventListener('change', filterUsers);
    searchInput?.addEventListener('input', filterUsers);
    selectAllCheckbox?.addEventListener('change', toggleSelectAll);

    const userForm = document.getElementById('userForm');
    userForm?.addEventListener('submit', handleUserFormSubmit);
});

// ====== Modal Functions ======
function showAddUserModal() {
    const modal = document.getElementById('userModal');
    modal.querySelector('h2').textContent = 'Thêm người dùng mới';
    document.getElementById('userForm').reset();
    modal.style.display = 'block';
}

function showEditUserModal(userData) {
    const modal = document.getElementById('userModal');
    modal.querySelector('h2').textContent = 'Chỉnh sửa người dùng';
    const form = modal.querySelector('form');
    //form.elements['role'].value = userData.role;
    //form.elements['id'].value = userData.id;
    //form.elements['name'].value = userData.name;
    //form.elements['email'].value = userData.email;
    //form.elements['password'].value = '';
    modal.style.display = 'block';
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.style.display = 'none';
}

window.onclick = (event) => {
    const userModal = document.getElementById('userModal');
    if (event.target === userModal) closeModal('userModal');
};

// ====== User Form ======
function handleUserFormSubmit(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const userData = {
        role: formData.get('role'),
        id: formData.get('id'),
        name: formData.get('name'),
        email: formData.get('email'),
        password: formData.get('password')
    };
    if (!validateUserData(userData)) return;
    console.log('Submitting user data:', userData);
    closeModal('userModal');
    event.target.reset();
    showNotification('Lưu thông tin người dùng thành công!', 'success');
}

function validateUserData(userData) {
    if (!userData.id || !userData.name || !userData.email) {
        showNotification('Vui lòng điền đầy đủ thông tin!', 'error');
        return false;
    }
    if (!isValidEmail(userData.email)) {
        showNotification('Email không hợp lệ!', 'error');
        return false;
    }
    return true;
}

function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

// ====== Dummy Edit/Delete/Toggle ======
function editUser(userId) {
    console.log('Edit user:', userId);
    const sample = { id: userId, role: 'student', name: 'Nguyễn Văn A', email: 'a@example.com' };
    showEditUserModal(sample);
}

function toggleUserStatus(userId) {
    console.log('Toggling status for user:', userId);
    showNotification('Cập nhật trạng thái người dùng thành công!', 'success');
}

function deleteUser(userId) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        console.log('Deleting user:', userId);
        showNotification('Xóa người dùng thành công!', 'success');
    }
}

// ====== Filter & Select ======
function filterUsers() {
    const role = roleFilter.value;
    const status = statusFilter.value;
    const search = searchInput.value.toLowerCase();
    console.log('Filtering users:', { role, status, search });
}

function toggleSelectAll() {
    document.querySelectorAll('.user-select').forEach(cb => {
        cb.checked = selectAllCheckbox.checked;
    });
}

// ====== Export ======
// Xuất Excel (dùng SheetJS)
function exportUsers() {
    const rows = document.querySelectorAll('.data-table tbody tr');
    const data = [];

    rows.forEach(row => {
        const cells = row.querySelectorAll('td');
        if (cells.length >= 6) {
            const roles = Array.from(cells[4].querySelectorAll('.badge')).map(b => b.textContent.trim()).join(', ');
            const status = cells[5].textContent.trim();

            data.push({
                MaSo: cells[1].textContent.trim(),
                HoTen: cells[2].textContent.trim(),
                Email: cells[3].textContent.trim(),
                VaiTro: roles,
                TrangThai: status
            });
        }
    });

    if (data.length === 0) {
        showNotification('Không có dữ liệu để export!', 'error');
        return;
    }

    const ws = XLSX.utils.json_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Users");
    XLSX.writeFile(wb, "users.xlsx");
    showNotification('Export Excel thành công!', 'success');
}

// ====== Utility ======
function showNotification(message, type = 'info') {
    alert(message);
}
