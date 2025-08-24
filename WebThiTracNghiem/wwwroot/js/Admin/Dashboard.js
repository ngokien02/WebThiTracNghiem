// Initialize charts when the document is ready
document.addEventListener('DOMContentLoaded', function () {
    if (window.location.pathname.startsWith('/Admin')) {
        initializeAccessChart();
        initializeExamChart();
        setupEventListeners();
    }
});


// Initialize access statistics chart
function initializeAccessChart() {
    const ctx = document.getElementById('accessChart').getContext('2d');
    const accessChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['00:00', '03:00', '06:00', '09:00', '12:00', '15:00', '18:00', '21:00'],
            datasets: [{
                label: 'Sinh viên',
                data: [30, 25, 20, 150, 280, 320, 280, 120],
                borderColor: '#1976D2',
                backgroundColor: 'rgba(25, 118, 210, 0.1)',
                tension: 0.4,
                fill: true
            }, {
                label: 'Giảng viên',
                data: [5, 3, 2, 15, 25, 30, 25, 10],
                borderColor: '#4CAF50',
                backgroundColor: 'rgba(76, 175, 80, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        display: true,
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

// Initialize exam distribution chart
function initializeExamChart() {
    const ctx = document.getElementById('examChart').getContext('2d');
    const examChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Đang diễn ra', 'Sắp tới', 'Đã kết thúc', 'Đã hủy'],
            datasets: [{
                data: [23, 45, 85, 3],
                backgroundColor: [
                    '#4CAF50',
                    '#1976D2',
                    '#9E9E9E',
                    '#F44336'
                ],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'right',
                },
                title: {
                    display: false
                }
            },
            cutout: '70%'
        }
    });
}

// Setup event listeners
function setupEventListeners() {
    // Time range selector
    const timeRange = document.getElementById('timeRange');
    if (timeRange) {
        timeRange.addEventListener('change', function () {
            updateDashboardData(this.value);
        });
    }

    // Alert action buttons
    const investigateButtons = document.querySelectorAll('.btn-investigate');
    investigateButtons.forEach(button => {
        button.addEventListener('click', function () {
            const alertItem = this.closest('.alert-item');
            investigateAlert(alertItem);
        });
    });

    const blockButtons = document.querySelectorAll('.btn-block');
    blockButtons.forEach(button => {
        button.addEventListener('click', function () {
            const alertItem = this.closest('.alert-item');
            blockIP(alertItem);
        });
    });
}

// Update dashboard data based on time range
function updateDashboardData(timeRange) {
    // Simulate API call
    console.log('Updating dashboard data for:', timeRange);
    // Here you would typically make an API call to get new data
    // Then update the charts and statistics accordingly
}

// Investigate system alert
function investigateAlert(alertItem) {
    const alertId = alertItem.dataset.alertId;
    console.log('Investigating alert:', alertId);
    // Here you would typically open a detailed view or make an API call

    // Show investigation in progress
    const statusIcon = alertItem.querySelector('.alert-icon i');
    statusIcon.className = 'fas fa-spinner fa-spin';

    // Simulate investigation process
    setTimeout(() => {
        statusIcon.className = 'fas fa-check';
        alertItem.style.backgroundColor = 'rgba(76, 175, 80, 0.1)';
    }, 2000);
}

// Block IP address
function blockIP(alertItem) {
    const ip = alertItem.querySelector('.alert-details p').textContent.match(/\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b/)[0];
    console.log('Blocking IP:', ip);

    // Show blocking in progress
    const button = alertItem.querySelector('.btn-block');
    button.textContent = 'Đang chặn...';
    button.disabled = true;

    // Simulate API call to block IP
    setTimeout(() => {
        button.textContent = 'Đã chặn';
        button.style.backgroundColor = '#9E9E9E';
        alertItem.style.opacity = '0.7';
    }, 1500);
}

// Add new system alert
function addSystemAlert(type, title, message) {
    const alertList = document.querySelector('.alert-list');
    const alertItem = document.createElement('div');
    alertItem.className = 'alert-item warning';
    alertItem.innerHTML = `
        <div class="alert-icon"><i class="fas fa-exclamation-triangle"></i></div>
        <div class="alert-details">
            <h4>${title}</h4>
            <p>${message}</p>
            <span class="alert-time">Vừa xong</span>
        </div>
        <div class="alert-actions">
            <button class="btn-investigate">Kiểm tra</button>
        </div>
    `;

    // Add to the beginning of the list
    alertList.insertBefore(alertItem, alertList.firstChild);

    // Remove oldest alert if more than 5
    if (alertList.children.length > 5) {
        alertList.removeChild(alertList.lastChild);
    }

    // Setup event listener for new alert
    const investigateButton = alertItem.querySelector('.btn-investigate');
    investigateButton.addEventListener('click', () => investigateAlert(alertItem));
}

// Helper function to format relative time
function getRelativeTimeString(date) {
    const now = new Date();
    const diffMinutes = Math.floor((now - date) / 60000);

    if (diffMinutes < 1) return 'Vừa xong';
    if (diffMinutes < 60) return `${diffMinutes} phút trước`;

    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) return `${diffHours} giờ trước`;

    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} ngày trước`;
}
