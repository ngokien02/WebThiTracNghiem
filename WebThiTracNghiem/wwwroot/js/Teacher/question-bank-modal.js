document.addEventListener("click", function (e) {
    const target = e.target;
    const modal = document.querySelector("#qb-modal");

    // Mở modal
    if (target.closest("#question-bank-btn")) {
        modal.classList.add("is-open");
    }

    // Đóng modal (nút close hoặc overlay)
    if (target.closest("[data-action='close']")) {
        modal.classList.remove("is-open");
    }
});
