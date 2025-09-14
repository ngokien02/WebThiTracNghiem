document.addEventListener("click", function (e) {
    const target = e.target;

    const resultModal = document.getElementById("resultModal");
    const qbModal = document.getElementById("qb-modal");

    // Mở
    if (target.closest(".btn-view") && resultModal) {
        resultModal.classList.add("is-open");
    }
    if (target.closest("#question-bank-btn") && qbModal) {
        qbModal.classList.add("is-open");
    }

    // Đóng tất cả modal khi click nút đóng hoặc overlay
    if (target.closest("[data-action='close']")) {
        if (resultModal) resultModal.classList.remove("is-open");
        if (qbModal) qbModal.classList.remove("is-open");
    }
});
