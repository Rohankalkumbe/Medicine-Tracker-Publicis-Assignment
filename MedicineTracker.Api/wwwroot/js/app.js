const API_BASE = "/api";
const LOW_STOCK_THRESHOLD = 10;
const EXPIRY_WARNING_DAYS = 30;

const els = {
  tableBody: document.getElementById("medicineTableBody"),
  emptyState: document.getElementById("emptyState"),
  salesBody: document.getElementById("salesTableBody"),
  searchBox: document.getElementById("searchBox"),
  addMedicineBtn: document.getElementById("addMedicineBtn"),
  addModal: document.getElementById("addModal"),
  addForm: document.getElementById("addMedicineForm"),
  cancelAddBtn: document.getElementById("cancelAddBtn"),
  sellModal: document.getElementById("sellModal"),
  sellForm: document.getElementById("sellForm"),
  sellMedicineName: document.getElementById("sellMedicineName"),
  cancelSellBtn: document.getElementById("cancelSellBtn"),
  toast: document.getElementById("toast"),
};

let searchDebounceTimer = null;

// ---------- API helpers ----------

async function fetchMedicines(search = "") {
  const url = search
    ? `${API_BASE}/medicines?search=${encodeURIComponent(search)}`
    : `${API_BASE}/medicines`;
  const res = await fetch(url);
  if (!res.ok) throw new Error("Failed to load medicines");
  return res.json();
}

async function fetchSales() {
  const res = await fetch(`${API_BASE}/sales`);
  if (!res.ok) throw new Error("Failed to load sales");
  return res.json();
}

async function createMedicine(payload) {
  const res = await fetch(`${API_BASE}/medicines`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || "Failed to add medicine");
  }
  return res.json();
}

async function deleteMedicine(id) {
  const res = await fetch(`${API_BASE}/medicines/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error("Failed to delete medicine");
}

async function recordSale(medicineId, quantity) {
  const res = await fetch(`${API_BASE}/sales`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ medicineId, quantity }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || "Failed to record sale");
  }
  return res.json();
}

// ---------- Rendering ----------

function daysUntil(dateStr) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const expiry = new Date(dateStr);
  expiry.setHours(0, 0, 0, 0);
  const diffMs = expiry - today;
  return Math.ceil(diffMs / (1000 * 60 * 60 * 24));
}

function formatDate(dateStr) {
  const d = new Date(dateStr);
  return d.toLocaleDateString();
}

function formatCurrency(amount) {
  return `Rs. ${Number(amount).toFixed(2)}`;
}

function renderMedicines(medicines) {
  els.tableBody.innerHTML = "";

  if (!medicines || medicines.length === 0) {
    els.emptyState.style.display = "block";
    return;
  }
  els.emptyState.style.display = "none";

  medicines.forEach((m) => {
    const tr = document.createElement("tr");

    const isExpiringSoon = daysUntil(m.expiryDate) < EXPIRY_WARNING_DAYS;
    const isLowStock = m.quantity < LOW_STOCK_THRESHOLD;

    if (isExpiringSoon) tr.classList.add("row-expiring");
    if (isLowStock) tr.classList.add("row-low-stock");

    tr.innerHTML = `
      <td>${m.id}</td>
      <td>${escapeHtml(m.fullName)}</td>
      <td>${formatDate(m.expiryDate)}</td>
      <td>${m.quantity}</td>
      <td>${formatCurrency(m.price)}</td>
      <td>${escapeHtml(m.brand)}</td>
      <td>
        <button class="action-btn sell" data-action="sell" data-id="${m.id}" data-name="${escapeHtml(m.fullName)}">Sell</button>
        <button class="action-btn delete" data-action="delete" data-id="${m.id}">Delete</button>
      </td>
    `;

    els.tableBody.appendChild(tr);
  });
}

function renderSales(sales) {
  els.salesBody.innerHTML = "";
  sales.forEach((s) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${s.id}</td>
      <td>${escapeHtml(s.medicineName)}</td>
      <td>${s.quantitySold}</td>
      <td>${formatCurrency(s.unitPrice)}</td>
      <td>${formatCurrency(s.totalAmount)}</td>
      <td>${formatDate(s.saleDate)}</td>
    `;
    els.salesBody.appendChild(tr);
  });
}

function escapeHtml(str) {
  if (str == null) return "";
  return String(str)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function showToast(message, isError = false) {
  els.toast.textContent = message;
  els.toast.classList.remove("hidden");
  els.toast.classList.toggle("error", isError);
  setTimeout(() => els.toast.classList.add("hidden"), 3000);
}

// ---------- Data loading ----------

async function loadMedicines(search = "") {
  try {
    const medicines = await fetchMedicines(search);
    renderMedicines(medicines);
  } catch (e) {
    showToast(e.message, true);
  }
}

async function loadSales() {
  try {
    const sales = await fetchSales();
    renderSales(sales);
  } catch (e) {
    showToast(e.message, true);
  }
}

async function refreshAll() {
  await Promise.all([loadMedicines(els.searchBox.value.trim()), loadSales()]);
}

// ---------- Event wiring ----------

els.searchBox.addEventListener("input", () => {
  clearTimeout(searchDebounceTimer);
  searchDebounceTimer = setTimeout(() => {
    loadMedicines(els.searchBox.value.trim());
  }, 250);
});

els.addMedicineBtn.addEventListener("click", () => {
  els.addForm.reset();
  els.addModal.classList.remove("hidden");
});

els.cancelAddBtn.addEventListener("click", () => {
  els.addModal.classList.add("hidden");
});

els.addForm.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const formData = new FormData(els.addForm);

  const payload = {
    fullName: formData.get("fullName"),
    notes: formData.get("notes"),
    expiryDate: formData.get("expiryDate"),
    quantity: Number(formData.get("quantity")),
    price: Number(formData.get("price")),
    brand: formData.get("brand"),
  };

  try {
    await createMedicine(payload);
    els.addModal.classList.add("hidden");
    showToast("Medicine added successfully");
    await refreshAll();
  } catch (e) {
    showToast(e.message, true);
  }
});

els.cancelSellBtn.addEventListener("click", () => {
  els.sellModal.classList.add("hidden");
});

els.sellForm.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const formData = new FormData(els.sellForm);
  const medicineId = Number(formData.get("medicineId"));
  const quantity = Number(formData.get("quantity"));

  try {
    await recordSale(medicineId, quantity);
    els.sellModal.classList.add("hidden");
    showToast("Sale recorded successfully");
    await refreshAll();
  } catch (e) {
    showToast(e.message, true);
  }
});

els.tableBody.addEventListener("click", async (evt) => {
  const btn = evt.target.closest("button[data-action]");
  if (!btn) return;

  const id = Number(btn.dataset.id);
  const action = btn.dataset.action;

  if (action === "sell") {
    els.sellForm.reset();
    els.sellForm.elements["medicineId"].value = id;
    els.sellMedicineName.textContent = btn.dataset.name;
    els.sellModal.classList.remove("hidden");
  } else if (action === "delete") {
    if (!confirm("Delete this medicine?")) return;
    try {
      await deleteMedicine(id);
      showToast("Medicine deleted");
      await refreshAll();
    } catch (e) {
      showToast(e.message, true);
    }
  }
});

// ---------- Init ----------

refreshAll();
