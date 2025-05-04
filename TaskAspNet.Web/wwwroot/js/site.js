    // Global modal handler
document.addEventListener("DOMContentLoaded", function () {
    window.openModal = function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = "flex";
        }
    };

    window.closeModal = function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = "none";
        }
    };

    document.querySelectorAll("[data-modal]").forEach(button => {
        button.addEventListener("click", function () {
            const modalId = this.getAttribute("data-modal");
            openModal(modalId);
        });
    });

   
    document.querySelectorAll(".close-modal").forEach(button => {
        button.addEventListener("click", function () {
            const modal = this.closest(".upload-modal-overlay, .modal-overlay");
            if (modal) {
                closeModal(modal.id);
            }
        });
    });

    
    window.addEventListener("click", function (event) {
        if (event.target.getAttribute("data-close-outside") === "true") {
            closeModal(event.target.id);
        }
    });


});

function toggleMenu(id) {
    const overlayMenu = document.getElementById(`overlay-${id}`);
    if (!overlayMenu) return;

    document.querySelectorAll('.overlay-menu').forEach(menu => {
        if (menu.id !== `overlay-${id}`) menu.style.display = 'none';
    });

    overlayMenu.style.display = overlayMenu.style.display === 'block' ? 'none' : 'block';
}




function initializeQuillEditors() {
    document.querySelectorAll('.quill-editor:not([data-quill-initialized])').forEach(function (editorEl) {
        const targetSelector = editorEl.getAttribute('data-target');
        const hiddenInput = document.querySelector(targetSelector);
        const toolbarSelector = editorEl.getAttribute('data-toolbar');
        const toolbarOptions = toolbarSelector ? toolbarSelector : '#custom-toolbar';

        const quill = new Quill(editorEl, {
            theme: 'snow',
            modules: {
                toolbar: toolbarOptions
            }
        });

        editorEl.setAttribute('data-quill-initialized', 'true');

        if (hiddenInput && hiddenInput.value) {
            quill.root.innerHTML = hiddenInput.value;
        }

        
        const form = editorEl.closest("form");
        if (form) {
            form.addEventListener("submit", function () {
                hiddenInput.value = quill.root.innerHTML;
            });
        }
    });
}


document.addEventListener('DOMContentLoaded', () => {
    setupDarkMode();
    initializeSignalR();
    restoreNotificationBadge();
    updateNotificationBadge();
    setupNotificationToggle();
    setupOutsideClickClose();
});

function setupDarkMode() {
    const toggle = document.getElementById('darkModeToggle');
    if (!toggle) return;

    toggle.addEventListener('change', function () {
        document.body.classList.toggle('dark-mode');
        localStorage.setItem('darkMode', this.checked);
    });

    if (localStorage.getItem('darkMode') === 'true') {
        document.body.classList.add('dark-mode');
        toggle.checked = true;
    }
}

// SignalR setup

// Made with gpt4.5 and me
// Holds the SignalR connection instance and prevents multiple simultaneous connection attempts
// Creates and handles a connection to for receiving notifications
// Builds a connection with WebSockets and LongPolling and has automatic reconnect intervals
// Handles connection events: reconnecting, reconnected, disconnect and closed 
// Gets and displays the notifications when the connection is established
// Sorts the notifications by date and adds them to the list adn updates the badge count
// Handles singel notification read and unread state as well as duplicate notifications
// Handles read notifications and also update the view also marks all as read and dissmisses all/singel notifications
// Handles the modal open and close and restores the count from local storage


let connection = null;
let isConnecting = false;

async function initializeSignalR() {
    if (isConnecting || typeof signalR === 'undefined') return;
    isConnecting = true;

    try {
        if (connection) await connection.stop();

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", {
                withCredentials: true,
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.onreconnecting(err => {
            console.warn("Reconnecting...", err);
            isConnecting = false;
        });

        connection.onreconnected(id => {
            console.log("Reconnected with ID:", id);
            isConnecting = false;
            loadNotifications();
        });

        connection.onclose(err => {
            console.warn("SignalR closed", err);
            isConnecting = false;
            setTimeout(initializeSignalR, 5000);
        });

        connection.on("ReceiveNotification", notification => {
            console.log("New notification:", notification);
            addNotification(notification);
            updateNotificationBadge();
            document.getElementById('notificationPanel').style.display = 'block';
        });

        await connection.start();
        console.log("SignalR connected:", connection.connectionId);
        isConnecting = false;

    } catch (err) {
        console.error("SignalR error:", err);
        isConnecting = false;
        setTimeout(initializeSignalR, 5000);
    }
}

document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible' && (!connection || connection.state === signalR.HubConnectionState.Disconnected)) {
        initializeSignalR();
    }
});

async function loadNotifications() {
    try {
        const response = await fetch('/api/Notification');
        if (!response.ok) throw new Error(`Status: ${response.status}`);

        const notifications = await response.json();
        const list = document.getElementById('notificationList');
        list.innerHTML = '';

        notifications.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
            .forEach(addNotification);

        updateNotificationBadge();

    } catch (err) {
        console.error("❌ Error loading notifications:", err);
    }
}

function addNotification(notification) {
    const list = document.getElementById('notificationList');
    if (document.querySelector(`.notification-item[data-id="${notification.id}"]`)) return;

    const el = document.createElement('div');
    el.className = `notification-item ${notification.isRead ? '' : 'unread'}`;
    el.setAttribute('data-id', notification.id);
    el.onclick = () => markAsRead(notification.id);
    el.innerHTML = `
        <div class="notification-item-message">${notification.message}</div>
        <div class="notification-item-time">${new Date(notification.createdAt).toLocaleString()}</div>
        <button class="delete-notification-btn" onclick="deleteNotification(${notification.id}); event.stopPropagation();">x</button>
    `;

    const noData = list.querySelector('.notification-item:not([data-id])');
    if (noData) noData.remove();

    list.prepend(el);
}

async function markAsRead(id) {
    try {
        const res = await fetch(`/api/Notification/${id}/read`, { method: 'POST' });
        if (!res.ok) throw new Error();
        document.querySelector(`.notification-item[data-id="${id}"]`)?.classList.remove('unread');
        updateNotificationBadge();
    } catch (err) {
        console.error("❌ Failed to mark as read", err);
    }
}

async function markAllAsRead() {
    try {
        const res = await fetch('/api/Notification/mark-all-read', { method: 'POST' });
        if (!res.ok) throw new Error();
        document.querySelectorAll('.notification-item.unread').forEach(el => el.classList.remove('unread'));
        updateNotificationBadge();
    } catch (err) {
        console.error("❌ Failed to mark all as read", err);
    }
}

async function clearAllNotifications() {
    try {
        const res = await fetch('/api/Notification/dismiss-all', { method: 'DELETE' });
        if (!res.ok) throw new Error();
        document.getElementById('notificationList').innerHTML = '';
        updateNotificationBadge();
    } catch (err) {
        console.error("❌ Failed to clear notifications", err);
    }
}

async function deleteNotification(id) {
    try {
        const res = await fetch(`/api/Notification/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error();
        document.querySelector(`.notification-item[data-id="${id}"]`)?.remove();
        updateNotificationBadge();
    } catch (err) {
        console.error("❌ Failed to delete notification", err);
    }
}

function updateNotificationBadge() {
    const badge = document.getElementById('notificationBadge');
    const unread = document.querySelectorAll('.notification-item.unread');
    const count = unread.length;

    if (count > 0) {
        badge.textContent = count;
        badge.style.display = 'block';
        localStorage.setItem("notificationBadgeCount", count);
    } else {
        badge.style.display = 'none';
        localStorage.removeItem("notificationBadgeCount");
    }
}

function restoreNotificationBadge() {
    const badge = document.getElementById('notificationBadge');
    const saved = localStorage.getItem("notificationBadgeCount");
    if (saved && parseInt(saved) > 0) {
        badge.textContent = saved;
        badge.style.display = 'block';
    }
}

function setupNotificationToggle() {
    const icon = document.querySelector('.notification-container .icon-link');
    if (!icon) return;

    icon.addEventListener('click', function (e) {
        e.preventDefault();
        const panel = document.getElementById('notificationPanel');
        panel.style.display = panel.style.display === 'block' ? 'none' : 'block';
    });
}

function setupOutsideClickClose() {
    document.addEventListener('click', (event) => {
        const panel = document.getElementById('notificationPanel');
        const container = document.querySelector('.notification-container');
        if (!container.contains(event.target)) {
            panel.style.display = 'none';
        }
    });
}


// form validation

document.addEventListener("DOMContentLoaded", function () {
    const forms = document.querySelectorAll("form[data-validate='true']");

    forms.forEach(form => {
        const fields = form.querySelectorAll("[data-validate-rule]");

        fields.forEach(field => {
            const wrapper = field.closest(".input-wrapper");
            const errorIcon = wrapper?.querySelector(".error-icon");
            let touched = false;

            const validateField = () => {
                const rule = field.getAttribute("data-validate-rule");
                const value = field.value.trim();
                let isValid = true;

                switch (rule) {
                    case "required":
                        isValid = value !== "";
                        break;
                    case "email":
                        isValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
                        break;
                    case "number":
                        isValid = !isNaN(value) && value !== "";
                        break;
                    case "password":
                        isValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/.test(value);
                        break;
                    case "confirm-password":
                        const passwordInput = form.querySelector("input[data-validate-rule='password']");
                        isValid = value === passwordInput?.value.trim();
                        break;
                    default:
                        break;
                }

                if (!isValid && (touched || rule === "email" || rule === "confirm-password")) {
                    field.classList.add("input-error");
                    if (errorIcon) errorIcon.style.display = "block";

                    let msg = wrapper.querySelector(".error-message");
                    if (!msg) {
                        msg = document.createElement("div");
                        msg.className = "error-message";
                        wrapper.appendChild(msg);
                    }
                    msg.textContent = field.getAttribute("data-validate-message");
                } else {
                    field.classList.remove("input-error");
                    if (errorIcon) errorIcon.style.display = "none";

                    const msg = wrapper.querySelector(".error-message");
                    if (msg) msg.remove();
                }
            };

            field.addEventListener("input", () => {
                if (field.value.trim() !== "") touched = true;
                validateField();
            });

            field.addEventListener("blur", () => {
                touched = true;
                validateField();
            });
        });

        form.addEventListener("submit", e => {
            let hasError = false;

            const fields = form.querySelectorAll("[data-validate-rule]");
            fields.forEach(field => {
                const wrapper = field.closest(".input-wrapper");
                const errorIcon = wrapper?.querySelector(".error-icon");

                const rule = field.getAttribute("data-validate-rule");
                const value = field.value.trim();
                let isValid = true;

                switch (rule) {
                    case "required":
                        isValid = value !== "";
                        break;
                    case "email":
                        isValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
                        break;
                    case "number":
                        isValid = !isNaN(value) && value !== "";
                        break;
                    case "password":
                        isValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/.test(value);
                        break;
                    case "confirm-password":
                        const passwordInput = form.querySelector("input[data-validate-rule='password']");
                        isValid = value === passwordInput?.value.trim();
                        break;
                    default:
                        break;
                }

                if (!isValid) {
                    field.classList.add("input-error");
                    if (errorIcon) errorIcon.style.display = "block";

                    let msg = wrapper.querySelector(".error-message");
                    if (!msg) {
                        msg = document.createElement("div");
                        msg.className = "error-message";
                        wrapper.appendChild(msg);
                    }
                    msg.textContent = field.getAttribute("data-validate-message");
                    hasError = true;
                } else {
                    field.classList.remove("input-error");
                    if (errorIcon) errorIcon.style.display = "none";

                    const msg = wrapper.querySelector(".error-message");
                    if (msg) msg.remove();
                }
            });

            if (hasError) e.preventDefault();
        });
    });
});



// toogle password visibility


document.addEventListener("DOMContentLoaded", () => {
    const toggleIcon = document.getElementById("togglePassword");
    const passwordInput = document.getElementById("password");

    if (toggleIcon && passwordInput) {
        toggleIcon.addEventListener("click", () => {
            const type = passwordInput.getAttribute("type") === "password" ? "text" : "password";
            passwordInput.setAttribute("type", type);
            toggleIcon.classList.toggle("fa-eye");
            toggleIcon.classList.toggle("fa-eye-slash");
        });
    }
});



// Attach an event listener to open the modal on button click
var openButton = document.getElementById('openConsentModalButton');
if (openButton) {
    openButton.addEventListener('click', function () {
        var modal = document.getElementById('consentModal');
        if (modal) {
            modal.style.display = 'flex';  
        }
    });
}


document.addEventListener("DOMContentLoaded", function () {
    let functionalConsent = localStorage.getItem("functionalCookies");
    functionalConsent = functionalConsent ? JSON.parse(functionalConsent) : false;
    
    var darkModeToggle = document.getElementById("darkModeToggle");
    
    if (darkModeToggle) {
        if (!functionalConsent) {
            darkModeToggle.disabled = true;
            
            darkModeToggle.title = "Dark mode is disabled because you declined functional cookies.";
            
            document.body.classList.remove("dark-mode");
        } else {
            darkModeToggle.disabled = false;
        }
    }
});

// Got a start from stackoverflow and then i made some and debugged with gpt4.5
// It runs when the page is loaded
// It handles the cookie consent modal
// It has a error message bock
// It handles the form submission with token needed to submit
// Adds all the checkboxes values to the form data
// Handles if the consent was given or not
// Adds the interaction with the modal and user to localstorage
// Stores the catergories user selected in local storage
// Has a fallback if the submission fails
// Then has listners for accept all and decline all buttons
// Checks if the user already given consent
// Show the modal manually and globally
// The code above disables darkmode if the user declines functional cookies


// cookie consent modal

document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('cookieConsentModal');
    const form = document.getElementById('cookieConsentForm');
    const errorDiv = document.getElementById('consentError');
    const acceptAllBtn = document.getElementById('btnAcceptAll');
    const declineAllBtn = document.getElementById('btnDeclineAll');

    if (!modal || !form) {
        console.error('Cookie consent modal or form not found.');
        return;
    }

    function showError(message) {
        if (errorDiv) {
            errorDiv.textContent = message;
            errorDiv.style.display = 'block';
            setTimeout(() => {
                errorDiv.style.display = 'none';
            }, 5000);
        }
    }

    function handleFormSubmit(formData) {
        const submitButton = form.querySelector('button[type="submit"]');
        const originalText = submitButton.textContent;
        submitButton.textContent = 'Saving...';
        submitButton.disabled = true;

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const data = new FormData();
        data.append('__RequestVerificationToken', token);

        const checkboxes = form.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(checkbox => {
            data.append(checkbox.name, checkbox.checked);
        });

        const anyChecked = Array.from(checkboxes).some(cb => cb.checked);
        data.append('IsConsentGiven', anyChecked);

        localStorage.setItem('cookieConsentSubmitted', 'true');

        fetch(form.action, {
            method: 'POST',
            body: data,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (!response.ok) {
                    return response.text().then(text => {
                        throw new Error(text || 'Network response was not ok');
                    });
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    const functionalValue = document.querySelector('input[name="FunctionalCookies"]').checked;
                    localStorage.setItem("functionalCookies", JSON.stringify(functionalValue));

                    if (modal) {
                        modal.style.display = 'none';
                    }
                } else {
                    throw new Error(data.message || 'Failed to save preferences');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                let errorMessage = 'Failed to save preferences. Please try again.';
                try {
                    const errorData = JSON.parse(error.message);
                    if (errorData.message) {
                        errorMessage = errorData.message;
                    }
                } catch (e) {
                    if (error.message) {
                        errorMessage = error.message;
                    }
                }
                showError(errorMessage);
            })
            .finally(() => {
                submitButton.textContent = originalText;
                submitButton.disabled = false;
            });
    }

    function toggleInfoPanel(event) {
        const target = event.currentTarget;
        const infoId = target.getAttribute('data-target');
        const infoPanel = document.getElementById(infoId);
        if (infoPanel) {
            infoPanel.classList.toggle('show');
        }
    }

    document.querySelectorAll('[data-toggle="cookie-info"]').forEach(toggle => {
        toggle.addEventListener('click', toggleInfoPanel);
    });

    if (acceptAllBtn) {
        acceptAllBtn.addEventListener('click', function () {
            const checkboxes = form.querySelectorAll('input[type="checkbox"]:not([disabled])');
            checkboxes.forEach(cb => cb.checked = true);
            handleFormSubmit(new FormData(form));
        });
    }

    if (declineAllBtn) {
        declineAllBtn.addEventListener('click', function () {
            const checkboxes = form.querySelectorAll('input[type="checkbox"]:not([disabled])');
            checkboxes.forEach(cb => cb.checked = false);
            handleFormSubmit(new FormData(form));
        });
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        handleFormSubmit(new FormData(form));
    });

    if (localStorage.getItem('cookieConsentSubmitted') === 'true') {
        if (modal) {
            modal.style.display = 'none';
        }
    }

    window.showCookieModal = function () {
        if (modal) {
            modal.style.display = 'flex';
        }
    };

    window.hideCookieModal = function () {
        if (modal) {
            modal.style.display = 'none';
        }
    };
});

// seach filter

const debounce = (fn, ms = 200) => {
    let t;
    return (...args) => { clearTimeout(t); t = setTimeout(() => fn(...args), ms); };
};

document.addEventListener('DOMContentLoaded', () => {
    const box = document.getElementById('searchBox');
    if (!box || typeof Fuse === 'undefined') return;  

    const nodes = [...document.querySelectorAll('.search-card, .search-row')];
    if (!nodes.length) return;

    const fuse = new Fuse(
        nodes.map(n => ({ node: n, text: n.dataset.search })),
        {
            keys: ['text'],
            threshold: 0.30, 
            ignoreLocation: true
        });

    const showAll = () => nodes.forEach(n => n.style.display = '');

    const filter = () => {
        const term = box.value.trim().toLowerCase();
        if (!term) { showAll(); return; }

        const short = term.length < 4;

        const fuseHits = new Set(fuse.search(term).map(r => r.item.node));

        nodes.forEach(n => {
            const plainHit = short && n.dataset.search.includes(term);
            const isVisible = fuseHits.has(n) || plainHit;
            n.style.display = isVisible ? '' : 'none';
        });
    };


    box.addEventListener('input', debounce(filter));
});
