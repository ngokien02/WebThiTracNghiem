$(() => {
    // ve bieu do thong ke
    $(document).on("click", "a.Reports", function (e) {
        e.preventDefault();

        var url = $(this).attr("href");

        $.get(url, function (data) {
			$(".main-content").html(data);

			const stats = {
				diemTB: document.getElementById("diemTB").value,
				tyLeTrenTB: document.getElementById("tyLeTrenTB").value,
				nguoiThamGia: document.getElementById("nguoiThamGia").value
			};

			console.log('✅ Biến stats:', stats);

			const chartContainer = document.querySelector("#examChart");
			if (!chartContainer) {
				console.error("❌ Không tìm thấy phần tử #examChart trong DOM!");
				return;
			}

			const chart = new ApexCharts(chartContainer, {
				chart: {
					type: 'bar',
					height: 350
				},
				series: [{
					name: 'Thống kê',
					data: [
						parseFloat(stats.nguoiThamGia),
						parseFloat(stats.diemTB),
						parseFloat(stats.tyLeTrenTB)
					]
				}],
				plotOptions: {
					bar: {
						horizontal: true,
						distributed: true,
						barHeight: '70%'
					}
				},
				xaxis: {
					categories: ['Người tham gia', 'Điểm TB', 'Trên TB (%)'],
					max: 100
				},
				colors: ['#4E79A7', '#F28E2B', '#76B7B2'],
				dataLabels: {
					enabled: true,
					style: {
						fontSize: '14px',
						colors: ['#000']
					}
				},
				title: {
					text: 'Biểu đồ thống kê kết quả thi',
					align: 'center',
					style: {
						fontSize: '16px',
						fontWeight: 'bold'
					}
				},
				tooltip: {
					x: {
						formatter: function (val, opts) {
							return `${opts.w.config.series[0].data[opts.dataPointIndex]}`;
						}
					}
				}
			});

			chart.render();
        });
        $('a').removeClass('active');
        $('a.Reports').addClass('active');
    });
})