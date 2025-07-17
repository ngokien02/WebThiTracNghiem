// Đổi value label theo từng role
const role = $("#RoleName").val(); // hoặc lấy từ hidden input / session
const label = role === "Teacher" ? "Mã GV"
    : role === "Admin" ? "Mã NV"
        : "Mã SV";

$("label[for='MaSV']").text(label);
// ✅ Xử lý cập nhật thông tin cá nhân
$("#profile-form").on("submit", async function (e) {
    e.preventDefault();

    const data = {
        HoTen: $("input[name='HoTen']").val()?.trim(),
        PhoneNumber: $("input[name='PhoneNumber']").val()?.trim(),
        NgaySinh: $("input[name='NgaySinh']").val()?.trim(),
        GioiTinh: $("select[name='GioiTinh']").val(),
        CMND: $("input[name='CMND']").val()?.trim(),
        DiaChi: $("input[name='DiaChi']").val()?.trim(),
        Email: $("input[name='Email']").val()?.trim(),
        AvatarUrl: $("#avatarPreview").attr("src"),
        Khoa: $("input[name='Khoa']").val()?.trim(),
        LopHoc: $("input[name='LopHoc']").val()?.trim(),
        KhoaHoc: $("input[name='KhoaHoc']").val()?.trim(),
        MaSV: $("input[name='MaSV']").val()?.trim()
    };

    // ✅ Gọi kiểm tra trước khi gửi
    if (!(await validateProfileData(data))) return;

    fetch('/Profile/Update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    }).then(async res => {
        if (res.ok) {
            Swal.fire("✅ Cập nhật thành công!", "Thông tin cá nhân đã được lưu.", "success");
        } else {
            const msg = await res.text(); // 👈 đọc nội dung lỗi trả về
            Swal.fire("❌ Lỗi cập nhật", msg || "Không thể cập nhật thông tin.", "error");
        }

    });
});

// ✅ Đổi mật khẩu
$("#btnChangePass").on("click", function () {
    const current = $("input[name='CurrentPassword']").val().trim();
    const newPass = $("input[name='NewPassword']").val().trim();
    const confirm = $("input[name='ConfirmPassword']").val().trim();

    $(".input-error").removeClass("input-error");
    let hasError = false;

    if (!current) {
        $("input[name='CurrentPassword']").addClass("input-error");
        hasError = true;
    }
    if (!newPass || newPass.length < 6) {
        $("input[name='NewPassword']").addClass("input-error");
        hasError = true;
    }
    if (newPass !== confirm) {
        $("input[name='ConfirmPassword']").addClass("input-error");
        hasError = true;
    }

    if (hasError) {
        Swal.fire("⚠️ Lỗi dữ liệu", "Vui lòng kiểm tra các ô nhập đang bị gạch đỏ.", "warning");
        return;
    }

    Swal.fire({
        title: "🔐 Đang đổi mật khẩu...",
        allowOutsideClick: false,
        didOpen: () => Swal.showLoading()
    });

    fetch("/Profile/ChangePassword", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ CurrentPassword: current, NewPassword: newPass, ConfirmPassword: confirm })
    }).then(res => {
        if (res.ok) {
            Swal.fire("✅ Đổi mật khẩu thành công", "Bạn sẽ được đăng xuất để đăng nhập lại.", "success");
            setTimeout(() => location.href = '/Identity/Account/Logout', 3000);
        } else {
            res.json().then(r => {
                const err = r?.errors?.map(e => e.description).join("<br>") || "Lỗi không xác định.";
                Swal.fire("❌ Lỗi đổi mật khẩu", err, "error");
            });

        }
    });
});

// ✅ Đổi avatar
$(".change-avatar-btn").on("click", function () {
    $("#avatarInput").click();
});

$("#avatarInput").on("change", function () {
    const file = this.files[0];
    if (!file) return;
    if (!/\.(jpg|jpeg|png|gif|webp)$/i.test(file.name)) {
        Swal.fire("❌ Định dạng ảnh không hợp lệ", "Chỉ chấp nhận ảnh .jpg, .png, .webp...", "error");
        return;
    }
    if (file.size > 2 * 1024 * 1024) {
        Swal.fire("❌ Ảnh quá lớn", "Dung lượng ảnh phải dưới 2MB.", "error");
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        $("#avatarPreview").attr("src", e.target.result);
    };
    reader.readAsDataURL(file);

    const formData = new FormData();
    formData.append("avatar", file);

    fetch("/Profile/UploadAvatar", {
        method: "POST",
        body: formData
    })
        .then(res => res.ok ? Swal.fire("✅ Ảnh đã cập nhật") : Swal.fire("❌ Lỗi upload ảnh"))
        .catch(() => Swal.fire("❌ Không thể tải ảnh lên"));
});
// Kiểm tra kiểu dữ liệu
function calculateAge(dateStr) {
    const dob = new Date(dateStr);
    const today = new Date();
    const age = today.getFullYear() - dob.getFullYear();
    const monthDiff = today.getMonth() - dob.getMonth();
    const dayDiff = today.getDate() - dob.getDate();

    return (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) ? age - 1 : age;
}
function formatDateTo_ISO(dateStr) {
    const date = new Date(dateStr);
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
}
function isNumberKey(evt) {
    const charCode = evt.which ? evt.which : evt.keyCode;
    return charCode >= 48 && charCode <= 57; // chỉ cho 0–9
}
async function checkExist(field, value) {
    const res = await fetch(`/api/profile/check-exist?type=${field}&value=${encodeURIComponent(value)}`);
    if (!res.ok) return false;
    return await res.json(); // true nếu tồn tại
}

async function validateProfileData(data) {
    let errors = [];
    $(".input-error").removeClass("input-error");
    $("[name='MaSV'], [name='CMND'], [name='PhoneNumber']").on("input", function () {
        this.value = this.value.replace(/\D/g, "");
    });
    if (!data.HoTen || data.HoTen.length < 2 || /\d/.test(data.HoTen)) {
        errors.push("Họ tên phải từ 2 ký tự, không chứa số.");
        highlightInput("HoTen");
    }

    if (!data.Email || !/^\S+@\S+\.\S+$/.test(data.Email)) {
        errors.push("Email không đúng định dạng.");
        highlightInput("Email");
    }

    const birthYear = new Date(data.NgaySinh).getFullYear();
    const currentYear = new Date().getFullYear();

    if (!data.NgaySinh || currentYear - birthYear < 18) {
        errors.push("Năm sinh phải đủ 18 tuổi trong năm hiện tại.");
        highlightInput("NgaySinh");
    }

    data.NgaySinh = formatDateTo_ISO(data.NgaySinh); // ➜ gửi dạng ISO

    if (!data.MaSV || !/^\d{10}$/.test(data.MaSV)) {
        errors.push("Mã sinh viên phải gồm đúng 10 chữ số.");
        highlightInput("MaSV");
    }

    if (data.CMND && !/^\d{12}$/.test(data.CMND)) {
        errors.push("CCCD phải gồm đúng 12 chữ số.");
        highlightInput("CMND");
    }
    // Kiểm tra trùng mã sinh viên
    if (data.MaSV && await checkExist("MaSV", data.MaSV)) {
        errors.push("❌ Mã sinh viên đã tồn tại.");
        highlightInput("MaSV");
    }

    // Kiểm tra trùng CCCD nếu có nhập
    if (data.CMND && await checkExist("CMND", data.CMND)) {
        errors.push("❌ CCCD đã tồn tại.");
        highlightInput("CMND");
    }

    if (data.PhoneNumber && !/^0\d{9}$/.test(data.PhoneNumber)) {
        errors.push("SĐT phải gồm 10 chữ số, bắt đầu bằng 0.");
        highlightInput("PhoneNumber");
    }

    const validGenders = ["Nam", "Nữ", "Khác"];
    if (data.GioiTinh && !validGenders.includes(data.GioiTinh)) {
        errors.push("Giới tính không hợp lệ.");
        highlightInput("GioiTinh");
    }

    if (errors.length > 0) {
        Swal.fire("⚠️ Lỗi nhập liệu", errors.join("<br>"), "warning");
        return false;
    }
    return true;
}
function highlightInput(name) {
    $(`[name='${name}']`).addClass("input-error");
}

$(".input-error").removeClass("input-error"); // Reset trước khi kiểm tra
