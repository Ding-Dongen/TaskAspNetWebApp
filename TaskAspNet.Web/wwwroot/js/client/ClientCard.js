
let currentOpenMenu = null;

function toggleClientMenu(clientId) {
    const menu = document.getElementById('overlay-' + clientId);
    if (!menu) return;

    if (currentOpenMenu && currentOpenMenu !== menu) {
        currentOpenMenu.style.display = 'none';
        document.removeEventListener('click', handleClickOutside);
    }

    const isVisible = menu.style.display === 'block';

    if (isVisible) {
        menu.style.display = 'none';
        document.removeEventListener('click', handleClickOutside);
        currentOpenMenu = null;
    } else {
        menu.style.display = 'block';
        currentOpenMenu = menu;

        setTimeout(() => {
            document.addEventListener('click', handleClickOutside);
        }, 0);
    }

    function handleClickOutside(event) {
        if (!menu.contains(event.target)) {
            menu.style.display = 'none';
            document.removeEventListener('click', handleClickOutside);
            currentOpenMenu = null;
        }
    }
}


function openDetailsModal(clientId) {
    toggleClientMenu(clientId);
    const modal = document.getElementById('detailsModal-' + clientId);
    if (!modal) {
        console.error('Details modal not found:', clientId);
        return;
    }
    modal.style.display = 'flex';
}

function openEditClientModal(clientId) {
    toggleClientMenu(clientId);
    const modal = document.getElementById('editClientModal-' + clientId);
    const container = document.getElementById('editClientContainer-' + clientId);
    if (!modal || !container) {
        console.error('Edit modal or container missing for', clientId);
        return;
    }
    fetch(`/Client/Edit/${clientId}`)
        .then(r => r.text())
        .then(html => {
            container.innerHTML = html;

            initializeAddressAndPhoneHandlers();
            initializeAccordionHandlers();
            if (typeof initializeQuillEditors === 'function') {
                initializeQuillEditors();
            }

            modal.style.display = 'flex';
        })
        .catch(err => console.error('Error loading edit form:', err));
}

document.querySelectorAll('.close-modal').forEach(btn => {
    btn.addEventListener('click', () => {
        btn.closest('.upload-modal-overlay').style.display = 'none';
    });
});



document.addEventListener("DOMContentLoaded", function () {
    const alert = document.querySelector(".alert-danger, .alert-success");
    if (alert) {
        setTimeout(() => {
            alert.classList.add("fade-out");
            setTimeout(() => alert.remove(), 1000); 
        }, 4000);
    }
});