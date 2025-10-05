// xu ly upload anh
$(document).on("change", ".admin-posts-image-input", function () {
    const file = this.files[0];
    if (!file) return;

    const allowedTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    if (!allowedTypes.includes(file.type)) {
        Swal.fire("❌ Định dạng ảnh không hợp lệ", "Chỉ chấp nhận JPG, PNG, GIF, WEBP.", "error");
        return;
    }

    const reader = new FileReader();
    const previewId = $(this).attr("id").replace("postImgInput", "postImgPreview");

    reader.onload = function (e) {
        $("#" + previewId).attr("src", e.target.result);
    };
    reader.readAsDataURL(file);
});


// xu ly them section
$(document).on("click", ".btnAddSection", function () {
    const sectionCount = $(".admin-posts-form-section").length - 1;

    const newSection = `
        <div class="admin-posts-form-section">
            <h3 class="admin-posts-section-title">
                <i class="fas fa-edit"></i> Phần ${sectionCount}
            </h3>

            <div class="admin-posts-form-group">
                <label for="adminPostsSection${sectionCount}Title">Đề mục phần ${sectionCount}</label>
                <input type="text" id="adminPostsSection${sectionCount}Title" name="section${sectionCount}Title"
                    class="admin-posts-form-control" placeholder="Nhập đề mục phần ${sectionCount}...">
            </div>

            <div class="admin-posts-form-group">
                <label for="adminPostsSection${sectionCount}Image">Ảnh phần ${sectionCount}</label>
                <img src="" alt="imgpreview" id="postImgPreview${sectionCount}" width="300" />
                <div class="admin-posts-image-upload-slot" data-slot="section${sectionCount}">
                    <div class="admin-posts-upload-placeholder">
                        <i class="fas fa-plus"></i>
                        <span>Thêm ảnh cho phần ${sectionCount}</span>
                    </div>
                    <input type="file" id="postImgInput${sectionCount}" class="admin-posts-image-input" accept="image/*">
                </div>
            </div>

            <div class="admin-posts-form-group">
                <label for="adminPostsSection${sectionCount}Content">Nội dung phần ${sectionCount}</label>
                <textarea id="adminPostsSection${sectionCount}Content" name="section${sectionCount}Content"
                    class="admin-posts-form-control" rows="6" placeholder="Nhập nội dung phần ${sectionCount}..."></textarea>
            </div>
        </div>
    `;

    $("#postSections").append(newSection);
});


// xu ly post bai
$(document).on("submit", "#adminPostsForm", function (e) {
    e.preventDefault();

    const formData = new FormData();

    const tieuDe = $("#adminPostsTitle").val();
    formData.append("TieuDe", tieuDe);

    const tomTat = $("#adminPostSummary").val();
    formData.append("TomTat", tomTat);

    $(".admin-posts-form-section").each(function (index) {
        if (index === 0) return; 

        const sectionNumber = index;
        const tieuDeDeMuc = $(this).find("input[type='text']").val();
        const noiDung = $(this).find("textarea").val();
        const fileInput = $(this).find("input[type='file']")[0];
        const file = fileInput?.files[0];

        formData.append(`DeMucs[${sectionNumber - 1}].TieuDe`, tieuDeDeMuc);
        formData.append(`DeMucs[${sectionNumber - 1}].NoiDung`, noiDung);
        formData.append(`DeMucs[${sectionNumber - 1}].STT`, sectionNumber);

        if (file) {
            formData.append(`DeMucs[${sectionNumber - 1}].ImageFile`, file);
        }
    });

    $.ajax({
        url: "/admin/post/createpost",
        method: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            Swal.fire("✅ Thành công", "Bài viết đã được tạo!", "success");
            console.log(res);
        },
        error: function (xhr) {
            Swal.fire("❌ Lỗi", "Không thể tạo bài viết: " + xhr.responseText, "error");
        }
    });
});


// render tin tuc
$(document).on("click", "a.postById", function (e) {
    e.preventDefault();

    var url = $(this).attr("href");

    $.get(url, function (data) {
        $(".postList").fadeOut(100, function () {
            $(".postList").html(data).fadeIn(100);
        });
    });
});