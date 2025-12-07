const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    button {
        display: flex;
        align-items: center;
        justify-content: center;
        min-width: 2.5rem;
        height: 2.5rem;
        padding: 0 0.5rem;
        border-radius: 0.5rem;
        border: 1px solid #E5E7EB;
        background: white;
        color: #374151;
        cursor: pointer;
        font-size: 0.875rem;
        font-weight: 500;
        transition: all 0.2s;
    }

    button:hover:not(:disabled) {
        border-color: #D1D5DB;
        background-color: #F9FAFB;
        color: #111827;
    }

    button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
        background-color: #F9FAFB;
    }

    button.active {
        background-color: var(--primary-light, #FFF1F2);
        color: var(--primary, #FF4F5A);
        border-color: var(--primary-light, #FFF1F2);
    }
`);

class FudiePagination extends HTMLElement {
    static get observedAttributes() {
        return ['current', 'total'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback() {
        this.render();
    }

    get currentPage() {
        return parseInt(this.getAttribute('current') || '1');
    }

    get totalPages() {
        return parseInt(this.getAttribute('total') || '1');
    }

    changePage(page) {
        if (page < 1 || page > this.totalPages) return;
        this.setAttribute('current', page);
        this.dispatchEvent(new CustomEvent('change', { detail: { page } }));
    }

    render() {
        const current = this.currentPage;
        const total = this.totalPages;

        // Logic to show truncated pages (1 ... 4 5 6 ... 10)
        // Simplified for now: Show all if small, or just Prev/Next and Current.

        let pages = [];
        // Add pages based on logic. For this demo, let's keep it simple: 1, 2, 3 ... last
        // or just explicit if total < 7.

        if (total <= 7) {
            pages = Array.from({ length: total }, (_, i) => i + 1);
        } else {
            // Complex truncation logic can be added here.
            // Implemented simple version: 1 ... current-1 current current+1 ... total
            if (current > 3) pages.push(1, '...');
            else pages.push(1, 2, 3);

            if (current > 3 && current < total - 2) {
                if (current - 1 > 1) pages.push(current - 1);
                pages.push(current);
                if (current + 1 < total) pages.push(current + 1);
            }

            if (current < total - 2) pages.push('...', total);
            else {
                if (total > 3) {
                    // If we didn't push these already
                    if (!pages.includes(total - 2)) pages.push(total - 2);
                    if (!pages.includes(total - 1)) pages.push(total - 1);
                    if (!pages.includes(total)) pages.push(total);
                }
            }
            // Dedup just in case
            pages = [...new Set(pages)].sort((a, b) => (typeof a === 'number' && typeof b === 'number') ? a - b : 0);
            // Re-fix order if sort messed up '...'
            // Actually, simplified approach is better:
            // [1, '...', current-1, current, current+1, '...', total]
            // Filter logic is better.
            pages = [];
            pages.push(1);
            if (current > 3) pages.push('...');

            let start = Math.max(2, current - 1);
            let end = Math.min(total - 1, current + 1);

            for (let i = start; i <= end; i++) pages.push(i);

            if (current < total - 2) pages.push('...');
            if (total > 1) pages.push(total);
        }

        this.shadowRoot.innerHTML = `
            <button class="prev-btn" ${current === 1 ? 'disabled' : ''}>
                <i class="fas fa-chevron-left"></i>
            </button>
            
            ${pages.map(p => {
            if (p === '...') return `<span style="color: #9CA3AF;">...</span>`;
            return `<button class="page-btn ${p === current ? 'active' : ''}" data-page="${p}">${p}</button>`;
        }).join('')}

            <button class="next-btn" ${current === total ? 'disabled' : ''}>
                <i class="fas fa-chevron-right"></i>
            </button>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        this.shadowRoot.querySelector('.prev-btn').addEventListener('click', () => this.changePage(current - 1));
        this.shadowRoot.querySelector('.next-btn').addEventListener('click', () => this.changePage(current + 1));
        this.shadowRoot.querySelectorAll('.page-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.changePage(parseInt(e.target.dataset.page)));
        });
    }
}

customElements.define('fudie-pagination', FudiePagination);
