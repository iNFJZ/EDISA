// Notification System Index
// This file exports all notification-related modules

// Import notification client for real-time notifications
import './client.js';

// Import notification page for dedicated notifications view
import './page.js';

// Export main notification client class for external use
export { default as NotificationClient } from './client.js';
export { default as NotificationsPage } from './page.js';
