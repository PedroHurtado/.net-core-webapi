const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .accordion-group {
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }
`);

class FudieAccordion extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.shadowRoot.innerHTML = `<div class="accordion-group"><slot></slot></div>`;
    }
}

const itemSheet = new CSSStyleSheet();
itemSheet.replaceSync(`
    :host {
        display: block;
        border: 1px solid #E5E7EB;
        border-radius: 1rem;
        overflow: hidden;
        background: white;
        transition: border-color 0.2s;
    }

    :host([open]) {
        border-color: #F3F4F6;
        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    }

    details {
        width: 100%;
    }

    summary {
        padding: 1rem 1.5rem;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: space-between;
        list-style: none; /* Hide default marker */
        font-weight: 600;
        color: #111827;
        user-select: none;
    }

    summary::-webkit-details-marker {
        display: none;
    }

    .icon {
        color: #9CA3AF;
        transition: transform 0.2s;
    }

    details[open] summary .icon {
        transform: rotate(180deg);
        color: var(--primary, #FF4F5A);
    }

    .content {
        padding: 0 1.5rem 1.5rem 1.5rem;
        color: #6B7280;
        line-height: 1.5;
        border-top: 1px solid transparent;
        transition: border-color 0.2s;
    }
    
    details[open] .content {
        border-top-color: #F3F4F6;
        padding-top: 1rem; /* Add padding when open */
    }
`);

class FudieAccordionItem extends HTMLElement {
    static get observedAttributes() { return ['title', 'open', 'icon']; }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [itemSheet];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue) return;
        if (name === 'open') {
            const details = this.shadowRoot.querySelector('details');
            if (details) {
                if (newValue !== null) details.setAttribute('open', '');
                else details.removeAttribute('open');
            }
        }
        if (name === 'title' || name === 'icon') {
            this.render();
        }
    }

    render() {
        const title = this.getAttribute('title') || '';
        const icon = this.getAttribute('icon');
        const isOpen = this.hasAttribute('open');

        this.shadowRoot.innerHTML = `
            <details ${isOpen ? 'open' : ''}>
                <summary>
                    <div style="display: flex; align-items: center; gap: 0.5rem;">
                        ${icon ? `<i class="fas fa-${icon}" style="color: var(--primary, #FF4F5A); width: 1.5rem; text-align: center;"></i>` : ''}
                        <span class="title-text">${title}</span>
                    </div>
                    <i class="fas fa-chevron-down icon"></i>
                </summary>
                <div class="content">
                    <slot></slot>
                </div>
            </details>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        this.shadowRoot.querySelector('details').addEventListener('toggle', (e) => {
            if (e.target.open) {
                this.setAttribute('open', '');
            } else {
                this.removeAttribute('open');
            }
        });
    }
}

customElements.define('fudie-accordion', FudieAccordion);
customElements.define('fudie-accordion-item', FudieAccordionItem);
