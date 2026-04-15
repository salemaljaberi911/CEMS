document.addEventListener("DOMContentLoaded", () => {
    initializeAutoDismissAlerts();
    initializeConfirmActions();
    initializeAnimatedCounters();
    initializeTableEnhancements();
    initializeCardSearch();
    initializeRevealOnScroll();
});

/* =========================
   1) Auto dismiss alerts
========================= */
function initializeAutoDismissAlerts() {
    const alerts = document.querySelectorAll(".alert");

    alerts.forEach((alert) => {
        setTimeout(() => {
            alert.style.transition = "opacity 0.4s ease, transform 0.4s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-6px)";

            setTimeout(() => {
                alert.remove();
            }, 400);
        }, 3500);
    });
}

/* =========================
   2) Confirm actions with Bootstrap modal
   Add data-confirm="message"
========================= */
function initializeConfirmActions() {
    const confirmElements = document.querySelectorAll("[data-confirm]");
    const modalElement = document.getElementById("confirmActionModal");
    const modalMessage = document.getElementById("confirmActionMessage");
    const confirmButton = document.getElementById("confirmActionButton");

    if (!modalElement || !modalMessage || !confirmButton || typeof bootstrap === "undefined") {
        confirmElements.forEach((element) => {
            element.addEventListener("click", (event) => {
                const message = element.getAttribute("data-confirm") || "Are you sure?";
                const confirmed = window.confirm(message);

                if (!confirmed) {
                    event.preventDefault();
                }
            });
        });
        return;
    }

    const confirmModal = new bootstrap.Modal(modalElement);
    let pendingAction = null;

    confirmElements.forEach((element) => {
        element.addEventListener("click", (event) => {
            event.preventDefault();

            const message = element.getAttribute("data-confirm") || "Are you sure you want to continue?";
            modalMessage.textContent = message;
            pendingAction = element;

            confirmModal.show();
        });
    });

    confirmButton.addEventListener("click", () => {
        if (!pendingAction) return;

        const element = pendingAction;
        pendingAction = null;
        confirmModal.hide();

        const form = element.closest("form");
        if (form) {
            form.submit();
            return;
        }

        const href = element.getAttribute("href");
        if (href) {
            window.location.href = href;
            return;
        }

        element.click();
    });

    modalElement.addEventListener("hidden.bs.modal", () => {
        pendingAction = null;
    });
}

/* =========================
   3) Animated counters
   Add data-counter to numbers
========================= */
function initializeAnimatedCounters() {
    const counters = document.querySelectorAll("[data-counter]");

    if (!("IntersectionObserver" in window)) {
        counters.forEach(counter => animateCounter(counter));
        return;
    }

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;

            animateCounter(entry.target);
            obs.unobserve(entry.target);
        });
    }, { threshold: 0.35 });

    counters.forEach((counter) => observer.observe(counter));
}

function animateCounter(counter) {
    if (counter.dataset.animated === "true") return;

    const target = parseInt(counter.textContent.trim(), 10);
    if (isNaN(target)) return;

    counter.dataset.animated = "true";

    let current = 0;
    const duration = 1000;
    const steps = 40;
    const increment = Math.max(1, Math.ceil(target / steps));
    const stepTime = Math.max(20, Math.floor(duration / steps));

    counter.textContent = "0";

    const timer = setInterval(() => {
        current += increment;

        if (current >= target) {
            counter.textContent = target.toString();
            clearInterval(timer);
        } else {
            counter.textContent = current.toString();
        }
    }, stepTime);
}

/* =========================
   4) Unified table search + filter
========================= */
function initializeTableEnhancements() {
    const tables = document.querySelectorAll("table[id]");

    tables.forEach((table) => {
        const tableId = table.id;
        const searchInput = document.querySelector(`[data-table-search="${tableId}"]`);
        const statusSelect = document.querySelector(`[data-status-filter="${tableId}"]`);

        const updateTableRows = () => {
            const rows = table.querySelectorAll("tbody tr");
            const query = (searchInput?.value || "").trim().toLowerCase();
            const selectedStatus = (statusSelect?.value || "all").trim().toLowerCase();

            rows.forEach((row) => {
                const rowText = row.textContent.toLowerCase();
                const statusCell = row.querySelector("[data-status]");
                const rowStatus = statusCell?.getAttribute("data-status")?.trim().toLowerCase() || "";

                const matchesSearch = query === "" || rowText.includes(query);
                const matchesStatus =
                    selectedStatus === "" ||
                    selectedStatus === "all" ||
                    rowStatus === selectedStatus;

                row.style.display = matchesSearch && matchesStatus ? "" : "none";
            });
        };

        if (searchInput) {
            searchInput.addEventListener("input", updateTableRows);
        }

        if (statusSelect) {
            statusSelect.addEventListener("change", updateTableRows);
        }
    });
}

/* =========================
   5) Card/container search
   Requires:
   - input with data-card-search="containerId"
   - container with id="containerId"
   - child cards with class "search-card"
========================= */
function initializeCardSearch() {
    const searchInputs = document.querySelectorAll("[data-card-search]");

    searchInputs.forEach((input) => {
        input.addEventListener("input", () => {
            const containerId = input.getAttribute("data-card-search");
            const container = document.getElementById(containerId);

            if (!container) return;

            const cards = container.querySelectorAll(".search-card");
            const query = input.value.trim().toLowerCase();

            cards.forEach((card) => {
                const cardText = card.textContent.toLowerCase();
                card.style.display = query === "" || cardText.includes(query) ? "" : "none";
            });
        });
    });
}

/* =========================
   6) Reveal on scroll
========================= */
function initializeRevealOnScroll() {
    const items = document.querySelectorAll(".reveal-on-scroll");

    if (!items.length) return;

    if (!("IntersectionObserver" in window)) {
        items.forEach((item) => item.classList.add("revealed"));
        return;
    }

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;

            entry.target.classList.add("revealed");
            obs.unobserve(entry.target);
        });
    }, { threshold: 0.15 });

    items.forEach((item) => observer.observe(item));
}