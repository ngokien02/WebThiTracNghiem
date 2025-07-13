$(() => {
    document.addEventListener('DOMContentLoaded', function () {
        // Tạo chatbox
        function createChatbox() {
            // Tạo container chính
            const chatbox = document.createElement('div');
            chatbox.id = 'chatbox';
            chatbox.className = 'chatbox';

            // Tạo header
            const header = document.createElement('div');
            header.className = 'chatbox-header';

            const title = document.createElement('h3');
            title.textContent = 'Hỗ trợ trực tuyến';

            const closeBtn = document.createElement('button');
            closeBtn.id = 'close-chat';
            closeBtn.className = 'close-btn';

            const closeIcon = document.createElement('i');
            closeIcon.className = 'fas fa-times';
            closeBtn.appendChild(closeIcon);

            header.appendChild(title);
            header.appendChild(closeBtn);

            // Tạo body
            const body = document.createElement('div');
            body.className = 'chatbox-body';

            // Tạo phần messages
            const messages = document.createElement('div');
            messages.className = 'chat-messages';

            const initialMessage = createMessage('Xin chào! Vui lòng cho biết vai trò của bạn:', 'bot');
            messages.appendChild(initialMessage);

            // Tạo role buttons
            const roleButtons = document.createElement('div');
            roleButtons.className = 'role-buttons';

            const roles = [
                { role: 'sinhvien', icon: 'user-graduate', text: 'Sinh viên' },
                { role: 'giangvien', icon: 'chalkboard-teacher', text: 'Giảng viên' },
                { role: 'quanly', icon: 'user-shield', text: 'Quản lý' }
            ];

            roles.forEach(role => {
                const button = createRoleButton(role.role, role.icon, role.text);
                roleButtons.appendChild(button);
            });

            // Tạo answer section
            const answerSection = document.createElement('div');
            answerSection.id = 'answer';
            answerSection.className = 'answer-section';

            // Tạo action buttons
            const actionButtons = document.createElement('div');
            actionButtons.className = 'chat-actions';
            actionButtons.style.display = 'none';

            const actions = [
                { action: 'thi', icon: 'pencil-alt', text: 'Tham gia thi' },
                { action: 'ketqua', icon: 'chart-bar', text: 'Xem kết quả thi' }
            ];

            actions.forEach(action => {
                const button = createActionButton(action.action, action.icon, action.text);
                actionButtons.appendChild(button);
            });

            // Ghép các phần lại với nhau
            body.appendChild(messages);
            body.appendChild(roleButtons);
            body.appendChild(answerSection);
            body.appendChild(actionButtons);

            chatbox.appendChild(header);
            chatbox.appendChild(body);

            return chatbox;
        }

        // Hàm tạo tin nhắn
        function createMessage(text, type) {
            const message = document.createElement('div');
            message.className = `message ${type}`;

            const content = document.createElement('div');
            content.className = 'message-content';
            content.textContent = text;

            message.appendChild(content);
            return message;
        }

        // Hàm tạo nút vai trò
        function createRoleButton(role, icon, text) {
            const button = document.createElement('button');
            button.className = 'role-btn';
            button.dataset.role = role;

            const iconElement = document.createElement('i');
            iconElement.className = `fas fa-${icon}`;

            button.appendChild(iconElement);
            button.appendChild(document.createTextNode(text));

            return button;
        }

        // Hàm tạo nút hành động
        function createActionButton(action, icon, text) {
            const button = document.createElement('button');
            button.className = 'action-btn';
            button.dataset.action = action;

            const iconElement = document.createElement('i');
            iconElement.className = `fas fa-${icon}`;

            button.appendChild(iconElement);
            button.appendChild(document.createTextNode(text));

            return button;
        }

        // Thêm chatbox vào body
        document.body.appendChild(createChatbox());

        // Xử lý sự kiện click cho contact-fab
        const contactFab = document.querySelector('.contact-fab');
        if (contactFab) {
            contactFab.addEventListener('click', function (e) {
                e.preventDefault();
                const chatbox = document.getElementById('chatbox');
                chatbox.classList.add('active');
            });
        }

        // Xử lý sự kiện đóng chatbox
        document.addEventListener('click', function (e) {
            if (e.target.id === 'close-chat' || e.target.closest('#close-chat')) {
                const chatbox = document.getElementById('chatbox');
                chatbox.classList.remove('active');
            }
        });

        // Xử lý sự kiện chọn vai trò
        document.addEventListener('click', function (e) {
            const roleBtn = e.target.closest('.role-btn');
            if (roleBtn) {
                const role = roleBtn.dataset.role;
                const chatMessages = document.querySelector('.chat-messages');
                const chatActions = document.querySelector('.chat-actions');

                // Thêm tin nhắn của user
                const userMessage = createMessage(roleBtn.textContent.trim(), 'user');
                chatMessages.appendChild(userMessage);

                // Thêm tin nhắn của bot và hiển thị các tùy chọn tương ứng
                let botResponse = '';
                switch (role) {
                    case 'sinhvien':
                        botResponse = 'Bạn muốn tham gia thi hay xem kết quả thi?';
                        chatActions.style.display = 'flex';
                        break;
                    case 'giangvien':
                        botResponse = 'Bạn cần hỗ trợ về việc gì?\n- Tạo đề thi\n- Quản lý câu hỏi\n- Xem kết quả thi của sinh viên';
                        break;
                    case 'quanly':
                        botResponse = 'Bạn cần hỗ trợ về việc gì?\n- Quản lý tài khoản\n- Quản lý kỳ thi\n- Xem báo cáo thống kê';
                        break;
                }

                const botMessage = createMessage(botResponse, 'bot');
                chatMessages.appendChild(botMessage);

                // Ẩn các nút vai trò
                document.querySelector('.role-buttons').style.display = 'none';
            }
        });

        // Xử lý sự kiện chọn hành động (cho sinh viên)
        document.addEventListener('click', function (e) {
            const actionBtn = e.target.closest('.action-btn');
            if (actionBtn) {
                const action = actionBtn.dataset.action;
                const chatMessages = document.querySelector('.chat-messages');

                // Thêm tin nhắn của user
                const userMessage = createMessage(actionBtn.textContent.trim(), 'user');
                chatMessages.appendChild(userMessage);

                // Thêm tin nhắn của bot
                let botResponse = '';
                switch (action) {
                    case 'thi':
                        botResponse = 'Bạn có thể truy cập vào mục "Kỳ thi" trên menu để xem danh sách các kỳ thi đang diễn ra.';
                        break;
                    case 'ketqua':
                        botResponse = 'Bạn có thể xem kết quả thi của mình trong mục "Lịch sử thi" trên menu.';
                        break;
                }

                const botMessage = createMessage(botResponse, 'bot');
                chatMessages.appendChild(botMessage);

                // Ẩn các nút hành động
                document.querySelector('.chat-actions').style.display = 'none';
            }
        });
    });
})