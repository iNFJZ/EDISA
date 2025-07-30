class NotificationClient {
    constructor() {
        this.API_BASE_URL = '/api/Notification';
        this.notifications = [];
        this.unreadCount = 0;
        this.currentPage = 1;
        this.pageSize = 50;
        this.hasMoreNotifications = true;
        this.isLoading = false;
        this.isExpanded = false;
        this.languageData = {};
        this.connection = null;
        this.isInitialized = false;
    }

    async init() {
        if (this.isInitialized) {
            return;
        }
        
        await this.loadLanguageData();
        await this.loadSignalR();
        await this.setupConnection();
        this.setupConnectionHandlers();
        
        const token = this.getAuthToken();
        if (token) {
            await this.connect();
            await this.loadUnreadCount();
            await this.loadNotifications();
        } else {
        }
        
        this.setupEventListeners();
        this.setupInfiniteScroll();
        this.isInitialized = true;
    }

    async loadLanguageData() {
        try {
            const currentLang = this.getCurrentLanguage();
            const response = await fetch(`/Shared/LanguageFiles/${currentLang}.json`);
            if (response.ok) {
                this.languageData = await response.json();
            } else {
                const enResponse = await fetch('/Shared/LanguageFiles/en.json');
                if (enResponse.ok) {
                    this.languageData = await enResponse.json();
                }
            }
        } catch (error) {
            this.languageData = {};
        }
    }

    getCurrentLanguage() {
        // Get language from localStorage (i18nextLng)
        const storedLang = localStorage.getItem('i18nextLng');
        return storedLang || 'en';
    }

    async getText(key, fallback = key) {
        const language = this.getCurrentLanguage();
        
        if (!this.languageData[language]) {
            await this.loadLanguageData();
        }
        
        return this.languageData[language]?.[key] || this.languageData['en']?.[key] || fallback;
    }

    getTextSync(key, fallback = key) {
        const language = this.getCurrentLanguage();
        return this.languageData[language]?.[key] || this.languageData['en']?.[key] || fallback;
    }

    async loadSignalR() {
        if (typeof signalR === 'undefined') {
            const script = document.createElement('script');
            script.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js';
            document.head.appendChild(script);
            await new Promise(resolve => script.onload = resolve);
        }
    }

    setupConnection() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/notificationhub', {
                accessTokenFactory: () => this.getAuthToken()
            })
            .withAutomaticReconnect()
            .build();
    }

    setupConnectionHandlers() {
        this.connection.on('ReceiveNotification', (notification) => {
            this.handleNewNotification(notification);
        });

        this.connection.on('NotificationMarkedAsRead', (notificationId) => {
            const notification = this.notifications.find(n => n.id === notificationId);
            if (notification) {
                notification.isRead = true;
                this.unreadCount = Math.max(0, this.unreadCount - 1);
                this.updateNotificationBadge();
            }
        });

        this.connection.on('NotificationDeleted', (notificationId) => {
            this.notifications = this.notifications.filter(n => n.id !== notificationId);
            this.updateNotificationBadge();
        });
    }

    async connect() {
        try {
            const token = this.getAuthToken();
            if (!token) {
                return;
            }
            
            await this.connection.start();
        } catch (err) {
            setTimeout(() => this.attemptReconnect(), 5000);
        }
    }

    async attemptReconnect() {
        try {
            await this.connection.start();
        } catch (err) {
            setTimeout(() => this.attemptReconnect(), 5000);
        }
    }

    getAuthToken() {
        return localStorage.getItem('authToken');
    }

    setupEventListeners() {
        const notificationToggle = document.getElementById('notificationToggle');
        if (notificationToggle && typeof bootstrap !== 'undefined') {
            new bootstrap.Dropdown(notificationToggle);
        }

        const markAllReadBtn = document.getElementById('markAllReadBtn');
        if (markAllReadBtn) {
            markAllReadBtn.addEventListener('click', () => this.markAllAsRead());
        }

        const deleteAllBtn = document.getElementById('deleteAllBtn');
        if (deleteAllBtn) {
            deleteAllBtn.addEventListener('click', () => this.deleteAllNotifications());
        }

        const viewAllBtn = document.querySelector('a[href="javascript:void(0);"] span[data-i18n="viewAllNotifications"]');
        if (viewAllBtn) {
            const parentLink = viewAllBtn.closest('a');
            if (parentLink) {
                parentLink.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.expandNotifications();
                });
            }
        }

        document.addEventListener('click', (e) => {
            const notificationItem = e.target.closest('.dropdown-notifications-item');
            if (notificationItem && !e.target.closest('.delete-notification-btn')) {
                const notificationId = notificationItem.dataset.notificationId;
                if (notificationId) {
                    const notification = this.notifications.find(n => n.id === parseInt(notificationId));
                    if (notification && !notification.isRead) {
                        this.markAsRead(parseInt(notificationId), notificationItem);
                    }
                }
            }
        });
    }

    setupInfiniteScroll() {
        const notificationList = document.querySelector('.dropdown-notifications-list');
        if (notificationList) {
            notificationList.addEventListener('scroll', () => {
                if (this.isExpanded && !this.isLoading && this.hasMoreNotifications) {
                    const { scrollTop, scrollHeight, clientHeight } = notificationList;
                    if (scrollTop + clientHeight >= scrollHeight - 50) {
                        this.loadMoreNotifications();
                    }
                }
            });
        }
    }

    expandNotifications() {
        this.isExpanded = true;
        const notificationList = document.querySelector('.dropdown-notifications-list');
        
        if (notificationList) {
            notificationList.style.maxHeight = '400px';
            notificationList.style.overflowY = 'scroll';
            notificationList.style.overflowX = 'hidden';
            
            const viewAllBtn = document.querySelector('a[href="javascript:void(0);"] span[data-i18n="viewAllNotifications"]');
            if (viewAllBtn) {
                viewAllBtn.textContent = this.getTextSync('showLess', 'Show less');
                const parentLink = viewAllBtn.closest('a');
                if (parentLink) {
                    parentLink.removeEventListener('click', this.expandHandler);
                    parentLink.addEventListener('click', (e) => {
                        e.preventDefault();
                        this.collapseNotifications();
                    });
                }
            }
            
            this.loadAllNotifications().then(() => {
                this.renderNotifications();
            });
        }
    }

    collapseNotifications() {
        this.isExpanded = false;
        const notificationList = document.querySelector('.dropdown-notifications-list');
        
        if (notificationList) {
            notificationList.style.maxHeight = '300px';
            notificationList.style.overflowY = 'auto';
            notificationList.style.overflowX = 'hidden';

            const viewAllBtn = document.querySelector('a[href="javascript:void(0);"] span[data-i18n="viewAllNotifications"]');
            if (viewAllBtn) {
                viewAllBtn.textContent = this.getTextSync('viewAllNotifications', 'View all notifications');
                const parentLink = viewAllBtn.closest('a');
                if (parentLink) {
                    parentLink.removeEventListener('click', this.collapseHandler);
                    parentLink.addEventListener('click', (e) => {
                        e.preventDefault();
                        this.expandNotifications();
                    });
                }
            }
            
            this.renderNotifications();
            
            setTimeout(() => {
                notificationList.scrollTop = 0;
                notificationList.style.maxHeight = 'none';
                notificationList.offsetHeight;
                notificationList.style.maxHeight = '300px';
            }, 50);
        }
    }

    async loadAllNotifications() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.currentPage = 1;
        this.hasMoreNotifications = true;
        
        try {
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}?page=1&pageSize=50`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                const allNotifications = data.items || [];
                
                if (allNotifications.length > 0) {
                    this.notifications = allNotifications;
                    this.hasMoreNotifications = allNotifications.length >= 50;
                }
            }
        } catch (error) {
            console.error('Failed to load all notifications:', error);
        } finally {
            this.isLoading = false;
        }
    }

    async loadMoreNotifications() {
        if (this.isLoading || !this.hasMoreNotifications) return;
        
        this.isLoading = true;
        this.currentPage++;
        
        const existingLoadingIndicator = document.getElementById('loading-indicator');
        if (existingLoadingIndicator) {
            existingLoadingIndicator.remove();
        }
        
        try {
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}?page=${this.currentPage}&pageSize=${this.pageSize}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                const newNotifications = data.items || [];
                
                if (newNotifications.length === 0) {
                    this.hasMoreNotifications = false;
                } else {
                    this.notifications.push(...newNotifications);
                    this.renderNotifications();
                }
            }
        } catch (error) {
            console.error('Failed to load more notifications:', error);
        } finally {
            this.isLoading = false;
        }
    }

    handleNewNotification(notification) {
        this.notifications.unshift(notification);
        this.unreadCount++;
        this.addNotificationToList(notification);
        this.updateNotificationBadge();
        this.showToastNotification(notification);
    }

    addNotificationToList(notification) {
        const notificationList = document.querySelector('.dropdown-notifications-list ul.list-group');
        if (!notificationList) return;

        const notificationHtml = this.createNotificationHtml(notification);
        notificationList.insertAdjacentHTML('afterbegin', notificationHtml);

        const newNotification = notificationList.firstElementChild;
        if (newNotification) {
            const deleteBtn = newNotification.querySelector('.delete-notification-btn');
            
            if (deleteBtn) {
                deleteBtn.addEventListener('click', () => this.deleteNotification(notification.id, newNotification));
            }
        }
    }

    formatTimeAgo(timeAgo) {
        if (!timeAgo) return this.getTextSync('justNow', 'Just now');
        const lang = this.getCurrentLanguage();
        if ([
            'Vừa xong', 'Just now', '今'
        ].includes(timeAgo)) {
            return this.getTextSync('justNow', 'Just now');
        }
        let match = timeAgo.match(/^(\d+)\s*(giờ|hours|時間) trước?$/);
        if (match) {
            return this.getTextSync('hoursAgo', '{0} hours ago').replace('{0}', match[1]);
        }
        match = timeAgo.match(/^(\d+)\s*(phút|minutes|分) trước?$/);
        if (match) {
            return this.getTextSync('minutesAgo', '{0} minutes ago').replace('{0}', match[1]);
        }
        match = timeAgo.match(/^(\d+)\s*(ngày|days|日) trước?$/);
        if (match) {
            return this.getTextSync('daysAgo', '{0} days ago').replace('{0}', match[1]);
        }
        return timeAgo;
    }

    createNotificationHtml(notification) {
        const isUnread = !notification.isRead;
        const unreadClass = isUnread ? 'unread' : '';
        const unreadStyle = isUnread ? 'background-color: rgba(13, 110, 253, 0.05);' : '';
        
        const translatedTitle = this.translateNotificationContent(notification.title);
        const translatedMessage = this.translateNotificationContent(notification.message);
        
        return `
            <li class="list-group-item dropdown-notifications-item ${unreadClass}" data-notification-id="${notification.id}" style="${unreadStyle}">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <div class="avatar avatar-sm me-3">
                            <span class="avatar-initial rounded-circle bg-label-${this.getIconBgColor(notification.type)}">
                                ${this.getNotificationIcon(notification.type)}
                            </span>
                        </div>
                    </div>
                    <div class="flex-grow-1">
                        <h6 class="mb-1">${this.escapeHtml(translatedTitle)}</h6>
                        <p class="mb-0">${this.escapeHtml(translatedMessage)}</p>
                        <small class="text-muted">${this.formatTimeAgo(notification.timeAgo)}</small>
                        <div class="mt-2">
                            <button class="btn btn-sm btn-outline-danger delete-notification-btn">
                                ${this.getTextSync('delete', 'Delete')}
                            </button>
                        </div>
                    </div>
                </div>
            </li>
        `;
    }

    getNotificationIcon(type) {
        const icons = {
            'user': 'ti ti-user',
            'system': 'ti ti-bell',
            'info': 'ti ti-info-circle',
            'warning': 'ti ti-alert-triangle',
            'error': 'ti ti-alert-circle',
            'success': 'ti ti-check-circle'
        };
        return `<i class="${icons[type] || icons['info']}"></i>`;
    }

    getAvatarHtml(notification) {
        return `<span class="avatar-initial rounded-circle bg-label-${this.getIconBgColor(notification.type)}">${this.getNotificationIcon(notification.type)}</span>`;
    }

    getIconBgColor(type) {
        const colors = {
            'user': 'primary',
            'system': 'secondary',
            'info': 'info',
            'warning': 'warning',
            'error': 'danger',
            'success': 'success'
        };
        return colors[type] || 'info';
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    translateNotificationContent(content) {
        if (!content) return content;
        
        const currentLang = this.getCurrentLanguage();
        if (currentLang === 'en') return content;
        
        const possibleKeys = [
            'welcomeToEdisa',
            'welcomeMessage', 
            'profileUpdated',
            'profileUpdatedMessage',
            'userCreated',
            'userCreatedMessage',
            'userUpdated',
            'userUpdatedMessage',
            'userDeleted',
            'userDeletedMessage',
            'fileUploaded',
            'fileUploadedMessage',
            'fileDeleted',
            'fileDeletedMessage'
        ];
        
        for (const key of possibleKeys) {
            const keyValue = this.getTextSync(key, '');
            if (keyValue && keyValue === content) {
                return this.getTextSync(key, content);
            }
        }
        
        return content;
    }

    updateNotificationBadge() {
        const badge = document.querySelector('.badge-notifications');
        if (badge) {
            badge.textContent = this.unreadCount;
            badge.style.display = this.unreadCount > 0 ? 'inline' : 'none';
            badge.classList.toggle('d-none', this.unreadCount === 0);
        }
    }

    showToastNotification(notification) {
        if (typeof toastr !== 'undefined') {
            const toastType = this.getToastType(notification.type);
            toastr[toastType](notification.message, notification.title, {
                timeOut: 5000,
                extendedTimeOut: 2000,
                closeButton: true,
                progressBar: true
            });
        }
    }

    getToastType(type) {
        const types = {
            'user': 'info',
            'system': 'info',
            'info': 'info',
            'warning': 'warning',
            'error': 'error',
            'success': 'success'
        };
        return types[type] || 'info';
    }

    async loadNotifications() {
        try {
            const token = this.getAuthToken();
            if (!token) {
                return;
            }

            const response = await fetch(`${this.API_BASE_URL}?page=${this.currentPage}&pageSize=${this.pageSize}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.notifications = data || [];
                this.hasMoreNotifications = this.notifications.length === this.pageSize;
                this.renderNotifications();
            } else if (response.status === 401) {
            } else {
            }
        } catch (error) {
        }
    }

    async loadUnreadCount() {
        try {
            const token = this.getAuthToken();
            if (!token) {
                return;
            }

            const response = await fetch(`${this.API_BASE_URL}/unread-count`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const count = await response.json();
                this.unreadCount = count || 0;
                this.updateNotificationBadge();
            } else if (response.status === 401) {
                console.log('Unauthorized access to unread count');
            } else {
                console.error('Failed to load unread count:', response.status);
            }
        } catch (error) {
            console.error('Error loading unread count:', error);
        }
    }

    renderNotifications() {
        const notificationList = document.querySelector('.dropdown-notifications-list ul.list-group');
        if (!notificationList) {
            return;
        }

        const currentScrollTop = notificationList.scrollTop;

        notificationList.innerHTML = '';
        
        if (this.notifications.length === 0) {
            notificationList.innerHTML = `
                <li class="list-group-item dropdown-notifications-item text-center py-4">
                    <i class="ti ti-bell-off text-muted" style="font-size: 2rem;"></i>
                    <p class="text-muted mt-2">${this.getTextSync('noNotifications', 'No notifications')}</p>
                </li>
            `;
            return;
        }

        const notificationsToShow = this.isExpanded ? this.notifications : this.notifications.slice(0, 5);
        
        notificationsToShow.forEach((notification, index) => {
            const notificationHtml = this.createNotificationHtml(notification);
            notificationList.insertAdjacentHTML('beforeend', notificationHtml);
            
            const lastItem = notificationList.lastElementChild;
            if (lastItem) {
                lastItem.style.opacity = '0';
                lastItem.style.transform = 'translateY(10px)';
                lastItem.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                
                setTimeout(() => {
                    lastItem.style.opacity = '1';
                    lastItem.style.transform = 'translateY(0)';
                }, index * 50);
            }
        });

        const notificationItems = notificationList.querySelectorAll('.dropdown-notifications-item');
        notificationItems.forEach(item => {
            const notificationId = item.dataset.notificationId;
            const notification = this.notifications.find(n => n.id.toString() === notificationId);
            
            if (notification) {
                const deleteBtn = item.querySelector('.delete-notification-btn');
                
                if (deleteBtn) {
                    deleteBtn.addEventListener('click', () => this.deleteNotification(notification.id, item));
                }
            }
        });

        if (!this.isExpanded && currentScrollTop > 0) {
            setTimeout(() => {
                notificationList.scrollTop = Math.min(currentScrollTop, notificationList.scrollHeight);
            }, 50);
        }
    }

    async markAsRead(notificationId, notificationElement) {
        try {
            const notification = this.notifications.find(n => n.id === notificationId);
            if (!notification) return;
            
            if (notification.isRead) return;
            
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}/${notificationId}/mark-as-read`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                notification.isRead = true;
                this.unreadCount = Math.max(0, this.unreadCount - 1);
                
                if (notificationElement) {
                    notificationElement.classList.remove('unread');
                    notificationElement.classList.add('read');
                    notificationElement.style.backgroundColor = '';
                    
                    const unreadIndicator = notificationElement.querySelector('.unread-indicator');
                    if (unreadIndicator) {
                        unreadIndicator.remove();
                    }
                }
                
                this.updateNotificationBadge();
            }
        } catch (error) {
        }
    }

    async markAllAsRead() {
        try {
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}/mark-all-as-read`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.notifications.forEach(notification => {
                    notification.isRead = true;
                });
                this.unreadCount = 0;
                this.renderNotifications();
                this.updateNotificationBadge();
            }
        } catch (error) {
        }
    }

    async deleteNotification(notificationId, notificationElement) {
        try {
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}/${notificationId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const deletedNotification = this.notifications.find(n => n.id === notificationId);
                if (deletedNotification && !deletedNotification.isRead) {
                    this.unreadCount = Math.max(0, this.unreadCount - 1);
                }
                
                this.notifications = this.notifications.filter(n => n.id !== notificationId);
                
                if (notificationElement) {
                    notificationElement.style.transition = 'all 0.3s ease';
                    notificationElement.style.opacity = '0';
                    notificationElement.style.transform = 'translateX(-100%)';
                    notificationElement.style.height = '0';
                    notificationElement.style.margin = '0';
                    notificationElement.style.padding = '0';
                    
                    setTimeout(() => {
                        notificationElement.remove();
                    }, 300);
                }
                this.renderNotifications();
                this.updateNotificationBadge();
            }
        } catch (error) {       
        }
    }

    async deleteAllNotifications() {
        if (!confirm(this.getTextSync('deleteAllConfirm', 'Are you sure you want to delete all notifications?'))) {
            return;
        }

        try {
            const token = this.getAuthToken();
            const response = await fetch(`${this.API_BASE_URL}/delete-all`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const notificationItems = document.querySelectorAll('.dropdown-notifications-item');
                notificationItems.forEach((item, index) => {
                    setTimeout(() => {
                        item.style.transition = 'all 0.3s ease';
                        item.style.opacity = '0';
                        item.style.transform = 'translateX(-100%)';
                        item.style.height = '0';
                        item.style.margin = '0';
                        item.style.padding = '0';
                    }, index * 50);
                });

                setTimeout(() => {
                    this.notifications = [];
                    this.unreadCount = 0;
                    this.renderNotifications();
                    this.updateNotificationBadge();
                }, notificationItems.length * 50 + 300);

                if (window.toastr) {
                    window.toastr.success(this.getTextSync('allNotificationsDeleted', 'All notifications deleted'));
                }
            } else {
                if (window.toastr) {
                    window.toastr.error(this.getTextSync('failedToDeleteAllNotifications', 'Failed to delete all notifications'));
                }
            }
        } catch (error) {
            console.error('Error deleting all notifications:', error);
            if (window.toastr) {
                window.toastr.error(this.getTextSync('failedToDeleteAllNotifications', 'Failed to delete all notifications'));
            }
        }
    }

    showAllNotifications() {
        this.expandNotifications();
    }

    async reinitialize() {
        this.isInitialized = false;
        await this.init();
    }

    async reloadLanguage() {
        await this.loadLanguageData();
        this.renderNotifications();
        this.updateNotificationBadge();
    }

    async changeLanguage(newLang) {
        localStorage.setItem('i18nextLng', newLang);
        await this.reloadLanguage();
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const notificationClient = new NotificationClient();
    notificationClient.init();
    
    let currentLang = notificationClient.getCurrentLanguage();
    
    window.addEventListener('storage', (e) => {
        if (e.key === 'i18nextLng') {
            const newLang = e.newValue || 'en';
            if (newLang !== currentLang) {
                currentLang = newLang;
                notificationClient.reloadLanguage();
            }
        } else if (e.key === 'authToken' || e.key === 'access_token' || e.key === 'token') {
            if (e.newValue) {
                notificationClient.reinitialize();
            } else {
                notificationClient.notifications = [];
                notificationClient.unreadCount = 0;
                notificationClient.updateNotificationBadge();
            }
        }
    });
    
    setInterval(() => {
        const token = notificationClient.getAuthToken();
        if (token && !notificationClient.isInitialized) {
            notificationClient.reinitialize();
        }
    }, 5000);
}); 