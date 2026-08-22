const API_BASE = '/api';

const errorBox = document.getElementById('errorBox');
const clientsBody = document.getElementById('clientsBody');
const walletsBody = document.getElementById('walletsBody');
const selectedClientMid = document.getElementById('selectedClientMid');

const loadClientsBtn = document.getElementById('loadClientsBtn');
const searchTerm = document.getElementById('searchTerm');
const prevPageBtn = document.getElementById('prevPageBtn');
const nextPageBtn = document.getElementById('nextPageBtn');
const pageInfo = document.getElementById('pageInfo');

const platformForm = document.getElementById('platformForm');
const statusForm = document.getElementById('statusForm');

let currentPage = 1;
const pageSize = 10;
let totalPages = 1;

function showError(message) {
    errorBox.textContent = message;
    errorBox.classList.remove('hidden');
}
function clearError() {
    errorBox.classList.add('hidden');
}

async function apiFetch(url, options = {}) {
    clearError();
    try {
        const response = await fetch(url, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...(options.headers || {})
            }
        });
        if (!response.ok) {
            let errorMessage = `Ошибка ${response.status}`;
            try {
                const errorData = await response.json();
                if (errorData.message) errorMessage = errorData.message;
                else if (errorData.title) errorMessage = errorData.title;
            } catch (e) { /* ignore */ }
            throw new Error(errorMessage);
        }
        if (response.status === 204) return null;
        return await response.json();
    } catch (error) {
        showError(error.message);
        throw error;
    }
}

async function loadClients(page = 1) {
    const term = searchTerm.value.trim();
    const url = `${API_BASE}/Clients?pageNumber=${page}&pageSize=${pageSize}${term ? `&searchTerm=${encodeURIComponent(term)}` : ''}`;
    try {
        const data = await apiFetch(url);
        if (!data || !data.items) {
            clientsBody.innerHTML = '<tr><td colspan="3">Нет данных</td></tr>';
            return;
        }
        renderClients(data.items);

        totalPages = Math.ceil(data.totalCount / pageSize) || 1;
        currentPage = page;
        updatePagination();
    } catch (error) {
        clientsBody.innerHTML = `<tr><td colspan="3">Ошибка загрузки: ${error.message}</td></tr>`;
    }
}

function renderClients(clients) {
    if (!clients.length) {
        clientsBody.innerHTML = '<tr><td colspan="3">Клиенты не найдены</td></tr>';
        return;
    }
    clientsBody.innerHTML = clients.map(client => `
        <tr class="clickable" data-mid="${client.mid}">
            <td>${client.mid}</td>
            <td>${client.fullName}</td>
            <td>${client.participantDRId || ''}</td>
        </tr>
    `).join('');

    document.querySelectorAll('#clientsBody tr.clickable').forEach(row => {
        row.addEventListener('click', function () {
            const mid = this.dataset.mid;
            loadWallets(mid);
        });
    });
}

function updatePagination() {
    pageInfo.textContent = `Страница ${currentPage} из ${totalPages}`;
    prevPageBtn.disabled = currentPage === 1;
    nextPageBtn.disabled = currentPage === totalPages;
}

loadClientsBtn.addEventListener('click', () => loadClients(1));
prevPageBtn.addEventListener('click', () => {
    if (currentPage > 1) loadClients(currentPage - 1);
});
nextPageBtn.addEventListener('click', () => {
    if (currentPage < totalPages) loadClients(currentPage + 1);
});
searchTerm.addEventListener('change', () => loadClients(1));

async function loadWallets(mid) {
    selectedClientMid.textContent = mid;
    const url = `${API_BASE}/Clients/${mid}/wallets`;
    try {
        const data = await apiFetch(url);
        if (!data || !data.length) {
            walletsBody.innerHTML = '<tr><td colspan="3">Кошельки не найдены</td></tr>';
            return;
        }
        walletsBody.innerHTML = data.map(w => `
            <tr>
                <td>${w.code}</td>
                <td>${w.status}</td>
                <td>${w.accountNumber || ''}</td>
            </tr>
        `).join('');
    } catch (error) {
        walletsBody.innerHTML = `<tr><td colspan="3">Ошибка загрузки: ${error.message}</td></tr>`;
    }
}

platformForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    const mid = document.getElementById('pMid').value.trim();
    const participantDRId = document.getElementById('pParticipantId').value.trim() || null;
    const walletCode = document.getElementById('pCode').value.trim();
    const status = document.getElementById('pStatus').value;
    const accountNumber = document.getElementById('pAccount').value.trim() || null;

    if (!mid || !walletCode) {
        showError('mid и код кошелька обязательны');
        return;
    }

    const payload = { mid, participantDRId, walletCode, status, accountNumber };
    try {
        const result = await apiFetch(`${API_BASE}/platform/wallet`, {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        alert(result?.message || 'Успешно отправлено');
        platformForm.reset();

        const currentMid = selectedClientMid.textContent;
        if (currentMid && currentMid !== '(не выбран)') {
            loadWallets(currentMid);
        }
    } catch (error) {
        // ошибка уже показана
    }
});

statusForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    const code = document.getElementById('sCode').value.trim();
    const mid = document.getElementById('sMid').value.trim();
    const newStatus = document.getElementById('sNewStatus').value;
    const accountNumber = document.getElementById('sAccount').value.trim() || null;

    if (!code || !mid) {
        showError('Код кошелька и mid обязательны');
        return;
    }

    // Формируем payload, включая статус только если он выбран
    const payload = { mid, accountNumber };
    if (newStatus) {
        payload.newStatus = newStatus;
    }

    try {
        const result = await apiFetch(`${API_BASE}/platform/wallet/${encodeURIComponent(code)}`, {
            method: 'PUT',
            body: JSON.stringify(payload)
        });
        alert(result?.message || 'Данные кошелька обновлены');
        statusForm.reset();
        document.getElementById('sNewStatus').value = '';

        const currentMid = selectedClientMid.textContent;
        if (currentMid && currentMid !== '(не выбран)') {
            loadWallets(currentMid);
        }
    } catch (error) {
        // ошибка уже показана
    }
});

loadClients(1);