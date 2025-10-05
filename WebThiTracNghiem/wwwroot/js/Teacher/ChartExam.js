$(() => {
    let currentExam = ""; // 🔥 Lưu giá trị kỳ thi đang lọc

    // 📊 Khi click menu "Báo cáo thống kê"
    $(document).on("click", "a.Reports", function (e) {
        e.preventDefault();

        var url = $(this).attr("href");

        $.get(url, function (data) {
            $(".main-content").html(data);
            renderChartFromHiddenInputs(); // 👉 vẽ biểu đồ luôn khi load
        });

        $('a').removeClass('active');
        $('a.Reports').addClass('active');
    });

    // 📅 Khi bấm nút Lọc
    $(document).on("click", ".filter-button", function () {
        currentExam = $(".filter-select").val(); // Cập nhật giá trị kỳ thi hiện tại

        $.get("/teacher/home/Reports", { page: 1, examTitle: currentExam }, function (data) {
            $(".main-content").html(data);
            $(".main-content").find(".filter-select").val(currentExam);
            renderChartFromHiddenInputs(); // 👉 vẽ lại biểu đồ sau khi lọc
        }).fail(() => {
            showNotification("Không thể tải báo cáo thống kê.", "error");
        });
    });

    // 🔢 Khi bấm phân trang
    $(document).on("click", ".page-Reports", function () {
        const page = $(this).text();

        $.get("/teacher/home/Reports", { page, examTitle: currentExam }, function (data) {
            $(".main-content").html(data);
            $(".main-content").find(".filter-select").val(currentExam);
            renderChartFromHiddenInputs();

            // ✅ Gán lại giá trị đã lọc vào dropdown mới render ra
            $(".filter-select").val(currentExam);
        }).fail(() => {
            showNotification("Không thể tải trang.", "error");
        });
    });

    // ✅ Hàm đọc hidden input và vẽ biểu đồ
    function renderChartFromHiddenInputs() {
        const diemTB = parseFloat($("#diemTB").val() || 0);
        const tyLeTrenTB = parseFloat($("#tyLeTrenTB").val() || 0);
        const nguoiThamGia = parseFloat($("#nguoiThamGia").val() || 0);
        drawExamChart({ diemTB, tyLeTrenTB, nguoiThamGia });
    }

    // ✅ Hàm vẽ biểu đồ
    function drawExamChart(stats) {
        const chartContainer = document.querySelector("#examChart");
        if (!chartContainer) {
            console.error("❌ Không tìm thấy phần tử #examChart trong DOM!");
            return;
        }

        chartContainer.innerHTML = "";

        const chart = new ApexCharts(chartContainer, {
            chart: {
                type: 'bar',
                height: 350,
                toolbar: {
                    show: true,
                    export: {
                        csv: { filename: "ThongKe_Thi" },
                        svg: { filename: "ThongKe_Thi" },
                        png: { filename: "ThongKe_Thi" }
                    }
                }
            },
            series: [{
                name: 'Thống kê',
                data: [stats.nguoiThamGia, stats.diemTB, stats.tyLeTrenTB]
            }],
            plotOptions: {
                bar: { horizontal: true, distributed: true, barHeight: '70%' }
            },
            xaxis: {
                categories: ['Người tham gia', 'Điểm TB', 'Trên TB (%)'],
                max: 100
            },
            colors: ['#4E79A7', '#F28E2B', '#76B7B2'],
            dataLabels: {
                enabled: true,
                style: { fontSize: '14px', colors: ['#000'] }
            },
            title: {
                text: 'Biểu đồ thống kê kết quả thi',
                align: 'center',
                style: { fontSize: '16px', fontWeight: 'bold' }
            }
        });

        chart.render();
    }
});
