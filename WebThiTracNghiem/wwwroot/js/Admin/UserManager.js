
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

//xu ly modal add user thu cong
window.showAddUserModal = function () {
    const modal = document.getElementById('userModal');
    modal.querySelector('h2').textContent = 'Thêm người dùng mới';
    document.getElementById('userForm').reset();
    modal.style.display = 'block';
};

window.closeModal = function (modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.style.display = 'none';
};

function showEditUserModal(userData) {
    const modal = document.getElementById('userModal');
    modal.querySelector('h2').textContent = 'Chỉnh sửa người dùng';
    const form = modal.querySelector('form');
    form.elements['role'].value = userData.role;
    form.elements['id'].value = userData.id;
    form.elements['username'].value = userData.username;
    form.elements['hoTen'].value = userData.hoTen;
    modal.style.display = 'block';
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
    $.get(`/admin/user/getEditUser/${userId}`, function (data) {
        showEditUserModal(data);
        $("button.handleButton").addClass("editUserButton");
    });
}

let loadPageReusable = (url) => {
    $.get(url, function (data) {
        $(".main-content").fadeOut(100, function () {
            $(".main-content").html(data).fadeIn(100);
        });
    });
}

$(document).on("click", "button.editUserButton", function (e) {
    e.preventDefault();
    var fd = $("#userForm").serialize();

    $.ajax({
        url: "/admin/user/edituser",
        type: "POST",
        data: fd,
        success: function (res) {
            if (res.success) {
                showNotification("Cập nhật thành công!", "success");
                closeModal('userModal');
                loadPageReusable('/admin/home/UserManager?page=1');
            }
            else {
                alert(res.message);
            }
        }
    })
})

function deleteUser(userId) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        $.post(`/admin/user/deleteUser/${userId}`, function (data) {
            if (data.success === true) {
                showNotification('Xóa người dùng thành công!', 'success');
                loadPageReusable('/admin/home/UserManager?page=1');
            }
            else {
                showNotification(data.message, 'error');
            }
        });
    }
}

// ====== Filter & Select ======
function filterUsers() {
    const role = roleFilter.value;
    const status = statusFilter.value;
    const search = searchInput.value.toLowerCase();
}

function toggleSelectAll() {
    document.querySelectorAll('.user-select').forEach(cb => {
        cb.checked = selectAllCheckbox.checked;
    });
}

//import user from excel
let usersFromExcel = [];

function previewExcel(event) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const data = new Uint8Array(e.target.result);
        const workbook = XLSX.read(data, { type: "array" });
        const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
        const rows = XLSX.utils.sheet_to_json(firstSheet, { header: 1 });

        usersFromExcel = [];
        const tbody = document.getElementById("userTableBody");
        tbody.innerHTML = "";

        for (let i = 1; i < rows.length; i++) { // bỏ qua header
            const [username, password, role] = rows[i];
            if (!username || !password) continue;

            usersFromExcel.push({ username, password, role: role || "Student" });

            const tr = document.createElement("tr");
            tr.innerHTML = `<td>${username}</td><td>${password}</td><td>${role || "Student"}</td>`;
            tbody.appendChild(tr);
        }

        openPreviewModal();
    };
    reader.readAsArrayBuffer(file);
}

// Hàm mở modal excel
function openPreviewModal() {
    $("#previewModal").show();
    $("#modalBackdrop").show();
}

// Hàm đóng modal excel
function closePreviewModal() {
    $("#previewModal").hide();
    $("#modalBackdrop").hide();
}

$(document).on("click", "#modalBackdrop", function () {
    closePreviewModal();
});

$(document).on("click", "#previewModal .close", function () {
    closePreviewModal();
});
// ====== Load lại bảng user ======
function loadUserTable() {
    const keyword = $("#searchInput").val().trim();
    const role = $("#roleFilter").val();
    const status = $("#statusFilter").val();

    $.ajax({
        url: "/Admin/Home/UserManager",
        type: "GET",
        data: {
            page: 1,
            keyword: keyword,
            role: role,
            status: status
        },
        success: function (data) {
            // Dùng jQuery parse chính xác HTML (tạo DOM tạm)
            const tempDom = $("<div>").html(data);

            // Tách phần table và phân trang từ DOM tạm
            const newTable = tempDom.find(".table-container").html();
            const newPagination = tempDom.find(".pagination").html();

            // Nếu có thì cập nhật lại giao diện hiện tại
            if (newTable) $(".table-container").html(newTable);
            if (newPagination) $(".pagination").html(newPagination);
        },
        error: function () {
            showNotification("Không thể tải lại bảng user.", "error");
        }
    });
}

//xu ly import users
// Hàm gửi import
function confirmImport() {
    if (!usersFromExcel || usersFromExcel.length === 0) {
        showNotification("Không có dữ liệu để import!", "error");
        return;
    }

    const dataToSend = usersFromExcel.map(u => ({
        username: u.username,
        password: u.password,
        role: u.role
    }));

    $.ajax({
        url: "/Admin/User/ImportUsers",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(dataToSend),
        success: function (res) {
            let message = "Import thành công!";
            if (res && res.message) message = res.message;

            showNotification(message, "success");

            usersFromExcel = [];
            closePreviewModal();
            loadUserTable();
        },
        error: function (xhr) {
            let msg = "Import thất bại!";
            try {
                const res = JSON.parse(xhr.responseText);
                if (res.message) msg = res.message;

                if (res.details && res.details.length > 0) {
                    const errorList = res.details.map((e, i) => `${i + 1}. ${e}`).join("\n");
                    msg += "\nChi tiết lỗi:\n" + errorList;
                }
            } catch (e) {
                console.error("Không parse được JSON lỗi:", e);
            }

            showNotification(msg, "error");
        }
    });
}

// ====== Export ======
// Xuất Excel (dùng SheetJS)
function exportUsers() {
    const keyword = $("#searchInput").val().trim();
    const role = $("#roleFilter").val();
    const status = $("#statusFilter").val();

    $.ajax({
        url: "/Admin/User/GetAllUsers",
        type: "GET",
        data: { keyword, role, status },
        success: function (users) {
            if (!users || users.length === 0) {
                showNotification('Không có dữ liệu để export!', 'error');
                return;
            }

            const data = users.map(u => ({
                UserName: u.userName,
                HoTen: u.hoTen,
                Email: u.email,
                VaiTro: u.vaiTro,
                TrangThai: u.trangThai
            }));
            const ws = XLSX.utils.json_to_sheet(data, { header: ["UserName", "HoTen", "Email", "VaiTro", "TrangThai"] });
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, "Users");
            XLSX.writeFile(wb, "users.xlsx");

            showNotification('Export Excel thành công!', 'success');
        },
        error: function () {
            showNotification('Xuất Excel thất bại!', 'error');
        }
    });
}

// ====== Utility ======

function showNotification(message, type = "info") {
    let icon = "info";
    if (type === "success") icon = "success";
    else if (type === "error") icon = "error";

    Swal.fire({
        text: message,
        icon: icon,
        showConfirmButton: true,
        timerProgressBar: true,
        allowOutsideClick: false,
        allowEscapeKey: true
    });
}
//xu ly phan trang user

var role1 = "";
$(document).on("change", "#roleFilter", function () {
    role1 = $("#roleFilter").val();
})
var status1 = "";
$(document).on("change", "#statusFilter", function () {
    status1 = $("#statusFilter").val();
})

var keyword1 = "";
$(document).on("change", "#searchInput", function () {
    keyword1 = $("#searchInput").val();
})
$(document).on("click", "button.page-userManager", function (e) {
    e.preventDefault();

    let pageUrl = $(this).attr("href") + `&role=${role1}` + `&status=${status1}` + `&keyword=${keyword1}`;

    if (pageUrl) {
        loadPage(pageUrl);
    }
});

let loadPage = (url) => {
    $.get(url, function (data) {
        const tempDom = $("<div>").html(data);

        const newTable = tempDom.find(".table-container").html();
        const newPagination = tempDom.find(".pagination").html();

        if (newTable) $(".table-container").html(newTable);
        if (newPagination) $(".pagination").html(newPagination);

        $("#roleFilter").val(role1);
        $("#statusFilter").val(status1);
        $("#searchInput").val(keyword1);
    });
}

// Xử lí tìm kiếm
$(document).on("click", "#btnTimKiem", function () {
    const keyword = $("#searchInput").val().trim();
    const role = $("#roleFilter").val();
    const status = $("#statusFilter").val();
    $.ajax({
        url: "/Admin/Home/UserManager",
        type: "GET",
        data: {
            page: 1,
            keyword: keyword,
            role: role,
            status: status
        },
        success: function (data) {

            // Dùng jQuery parse chính xác HTML (tạo DOM tạm)
            const tempDom = $("<div>").html(data);

            // Tách phần table và phân trang từ DOM tạm
            const newTable = tempDom.find(".table-container").html();
            const newPagination = tempDom.find(".pagination").html();
            // Nếu có thì cập nhật lại giao diện hiện tại
            if (newTable) $(".table-container").html(newTable);
            if (newPagination) $(".pagination").html(newPagination);

            // Giữ lại từ khóa tìm kiếm
            $("#searchInput").val(keyword);
        },
        error: function (xhr) {
            alert("Không thể tải dữ liệu tìm kiếm.");
        }
    });
});