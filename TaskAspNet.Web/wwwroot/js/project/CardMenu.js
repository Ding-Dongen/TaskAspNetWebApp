// card-menu.js

let currentOpenMenu = null;

function toggleMenu(projectId) {
    const menu = document.getElementById('overlay-' + projectId);
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

// The rest of these functions has gtp4.5 done and it does
// Set up initial modal state and show it
// Gets members that are assign to the project ID
// It makes a list of checkboxes for the member modal
// It creates the line under seach input with a checlbox, avaratar for each member searched for
// It handles the results for when a search is made
// Fetches to the search endpoint and gets the members
// List all members for choise
// Styles the displayed members and adds them to the checkbox list
// Clears the search input and results after adding a member

// Opens the modal and loads member checkboxes
async function openAddRemoveMemberModal(projectId) {
    console.log(`Opening modal for project ID: ${projectId}`);

    const modal = document.getElementById('addRemoveMemberModal');
    const projectInput = document.getElementById('modalProjectId');
    const listEl = document.getElementById('modalMemberList');
    const searchInput = document.getElementById('modalSearchInput');
    const searchRes = document.getElementById('modalSearchResults');

    projectInput.value = projectId;
    modal.style.display = 'flex';
    searchInput.value = '';
    searchRes.innerHTML = '';
    listEl.innerHTML = '<p>Loading members…</p>';

    try {
        const url = `${window.location.origin}/Member/GetMembers?projectId=${projectId}`;
        console.log(`Fetching assigned members from: ${url}`);
        const resp = await fetch(url);
        if (!resp.ok) throw new Error(`Status ${resp.status}`);

        const members = await resp.json();
        console.log("Current Members:", members);

        renderCheckboxList(members);
    }
    catch (err) {
        console.error("Error loading members:", err);
        listEl.innerHTML = '<p>No members added to this project.</p>';
    }
}

// Renders the checkboxes (or fallback) into the modal
function renderCheckboxList(members) {
    const container = document.getElementById('modalMemberList');
    container.innerHTML = '';

    // Fallback if there are no members
    if (!Array.isArray(members) || members.length === 0) {
        container.innerHTML = '<p>No members added to this project.</p>';
        return;
    }

    // Otherwise build the checkbox list
    const html = members.map(m => {
        const avatar = m.imageData?.currentImage || '/images/membericon/default-avatar.png';
        return `
          <label class="member-label">
            <input type="checkbox" name="MemberIds" value="${m.id}" checked>
            <img src="${avatar}"
                 alt="${m.firstName} ${m.lastName}"
                 style="width:30px;height:30px;border-radius:50%;margin-right:10px;">
            ${m.firstName} ${m.lastName}
          </label>
        `;
    }).join('');

    container.innerHTML = html;
    console.log("Members rendered in modal.");
}

function closeAddRemoveMemberModal() {
    document.getElementById('addRemoveMemberModal').style.display = 'none';
}

document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('modalSearchInput');
    const searchResults = document.getElementById('modalSearchResults');
    if (!searchInput || !searchResults) return;

    searchInput.addEventListener('input', async () => {
        const query = searchInput.value.trim();
        if (query.length < 2) {
            searchResults.innerHTML = '';
            return;
        }
        try {
            const urlSearch = `${window.location.origin}/Member/Search?term=${encodeURIComponent(query)}`;
            console.log(`Searching members from: ${urlSearch}`);
            const resp = await fetch(urlSearch);
            if (!resp.ok) throw new Error(`HTTP error! Status: ${resp.status}`);
            const matches = await resp.json();
            searchResults.innerHTML = '';
            matches.forEach(m => {
                console.log("Member data:", m);
                console.log("Image path:", m.imageData?.currentImage);
                const li = document.createElement('li');
                const img = document.createElement('img');
                img.src = m.imageData?.currentImage || '/images/membericon/default-avatar.png';
                console.log("Final image src:", img.src);
                img.alt = `${m.firstName} ${m.lastName}`;
                img.style.width = '30px';
                img.style.height = '30px';
                img.style.borderRadius = '50%';
                img.style.marginRight = '10px';
                li.appendChild(img);
                li.appendChild(document.createTextNode(`${m.firstName}`));
                const space = document.createTextNode(' ');
                li.appendChild(space);
                li.appendChild(document.createTextNode(`${m.lastName}`));
                li.style.display = 'flex';
                li.style.alignItems = 'center';
                li.style.padding = '5px';
                li.style.cursor = 'pointer';
                li.addEventListener('click', () => addMemberFromSearch(m));
                searchResults.appendChild(li);
            });
        } catch (err) {
            console.error("Search error:", err);
        }
    });
});

// When a search result is clicked, add that member to the checkbox list
function addMemberFromSearch(member) {
    console.log("Member object:", member);
    console.log("Member image data:", member.imageData);
    console.log("Member image path:", member.imageData?.currentImage);

    const modalMemberList = document.getElementById('modalMemberList');
    let checkbox = modalMemberList.querySelector(`input[name="MemberIds"][value="${member.id}"]`);
    if (checkbox) {
        checkbox.checked = true;
        checkbox.scrollIntoView({ behavior: "smooth", block: "center" });
        console.log(`Checkbox for member ${member.id} checked.`);
    } else {
        const avatarUrl = member.imageData?.currentImage || '/images/membericon/default-avatar.png';
        console.log("Using avatar URL:", avatarUrl);
        const label = document.createElement('label');
        label.classList.add("member-label");

        const input = document.createElement('input');
        input.type = 'checkbox';
        input.name = 'MemberIds';
        input.value = member.id;
        input.checked = true;

        const img = document.createElement('img');
        img.src = avatarUrl;
        img.alt = `${member.firstName} ${member.lastName}`;
        img.style.width = '30px';
        img.style.height = '30px';
        img.style.borderRadius = '50%';
        img.style.marginRight = '10px';

        const nameSpan = document.createElement('span');
        nameSpan.textContent = `${member.firstName} ${member.lastName}`;

        label.appendChild(input);
        label.appendChild(img);
        label.appendChild(nameSpan);

        modalMemberList.appendChild(label);
        console.log(`Checkbox for member ${member.id} created and checked.`);
    }

    document.getElementById('modalSearchInput').value = '';
    document.getElementById('modalSearchResults').innerHTML = '';
}


