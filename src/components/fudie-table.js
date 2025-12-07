const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        width: 100%;
        overflow-x: auto;
    }

    table {
        width: 100%;
        border-collapse: separate;
        border-spacing: 0;
        text-align: left;
    }

    th {
        background-color: #F9FAFB;
        color: #6B7280;
        font-weight: 600;
        font-size: 0.75rem;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        padding: 0.75rem 1.5rem;
        border-bottom: 1px solid #E5E7EB;
        white-space: nowrap;
    }

    /* First and last Header rounded corners if standalone? 
       Usually on the container. We'll keep it simple. */
    
    td {
        padding: 1rem 1.5rem;
        color: #111827;
        font-size: 0.875rem;
        border-bottom: 1px solid #F3F4F6;
        vertical-align: middle;
    }

    tr:last-child td {
        border-bottom: none;
    }

    tr:hover td {
        background-color: #F9FAFB;
    }

    img {
        width: 2.5rem;
        height: 2.5rem;
        border-radius: 50%;
        object-fit: cover;
    }

    .badge {
        display: inline-flex;
        align-items: center;
        padding: 0.125rem 0.5rem;
        border-radius: 9999px;
        font-size: 0.75rem;
        font-weight: 500;
    }

    .badge-success { background-color: #DEF7EC; color: #03543F; }
    .badge-warning { background-color: #FDF6B2; color: #723B13; }
    .badge-danger { background-color: #FDE8E8; color: #9B1C1C; }
    .badge-gray { background-color: #F3F4F6; color: #374151; }
    
    .actions {
        display: flex;
        gap: 0.5rem;
        justify-content: flex-end;
    }
`);

class FudieTable extends HTMLElement {
    static get observedAttributes() {
        return ['columns', 'data']; // Expecting JSON strings
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

    get columns() {
        try {
            return JSON.parse(this.getAttribute('columns') || '[]');
        } catch (e) {
            return [];
        }
    }

    get data() {
        try {
            return JSON.parse(this.getAttribute('data') || '[]');
        } catch (e) {
            return [];
        }
    }

    renderValue(item, col) {
        const val = item[col.key];

        if (col.type === 'image') {
            return `<img src="${val}" alt="Avatar">`;
        }

        if (col.type === 'badge') {
            // Expecting val to be { label: 'Active', variant: 'success' } or just string
            let label = val;
            let variant = 'gray';

            if (typeof val === 'object') {
                label = val.label;
                variant = val.variant;
            } else if (val === 'Active' || val === true) {
                variant = 'success';
            } else if (val === 'Inactive' || val === false) {
                variant = 'danger';
            } else if (val === 'Pending') {
                variant = 'warning';
            }

            return `<span class="badge badge-${variant}">${label}</span>`;
        }

        if (col.type === 'actions') {
            // Typically would need event handling here, complicated for JSON.
            // We'll dispatch events on click.
            return `
                <div class="actions">
                    <button class="action-btn edit-btn" data-id="${item.id}" style="color: #4F46E5; background: none; border: none; cursor: pointer;">Edit</button>
                    <button class="action-btn delete-btn" data-id="${item.id}" style="color: #EF4444; background: none; border: none; cursor: pointer;">Delete</button>
                </div>
            `;
        }

        return val || '-';
    }

    render() {
        const cols = this.columns;
        const data = this.data;

        if (!cols.length) return;

        this.shadowRoot.innerHTML = `
            <table>
                <thead>
                    <tr>
                        ${cols.map(col => `<th>${col.label}</th>`).join('')}
                    </tr>
                </thead>
                <tbody>
                    ${data.map(item => `
                        <tr>
                            ${cols.map(col => `
                                <td>${this.renderValue(item, col)}</td>
                            `).join('')}
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        // Add listeners for actions
        this.shadowRoot.querySelectorAll('.edit-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.dispatchEvent(new CustomEvent('edit', { detail: { id: e.target.dataset.id } }));
            });
        });

        this.shadowRoot.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.dispatchEvent(new CustomEvent('delete', { detail: { id: e.target.dataset.id } }));
            });
        });
    }
}

customElements.define('fudie-table', FudieTable);
