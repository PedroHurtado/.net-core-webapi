const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    nav {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 0.5rem;
        font-size: 0.875rem;
        color: #6B7280;
    }

    ::slotted(a) {
        text-decoration: none;
        color: #6B7280;
        transition: color 0.2s;
    }

    ::slotted(a:hover) {
        color: var(--primary, #FF4F5A);
    }

    ::slotted(span) {
        color: #111827;
        font-weight: 500;
    }

    .separator {
        color: #9CA3AF;
        margin: 0 0.25rem;
    }
`);

class FudieBreadcrumb extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
    }

    render() {
        // We need to inject separators between children.
        // Since we can't easily slot "between" elements without changing light DOM,
        // we might rely on the user providing structure OR we parse children.
        // Parsing children is better for "Component" feel.

        // However, generic slotting is robust.
        // Let's use a simpler approach: 
        // We will assign a class to children or use a slotchange listener to re-render structure?
        // Simpler: Just render the slot, and use CSS to add separators? 
        // CSS ::slotted(text-node) isn't a thing.

        // Let's just assume the user puts items in, and we can style them.
        // Actually, let's look at the standard: <fudie-breadcrumb items='[{label, href}]'></fudie-breadcrumb> might be easier.
        // But the user might want custom HTML.

        // Hybrid: <fudie-breadcrumb-item> child.
        this.shadowRoot.innerHTML = `<nav><slot></slot></nav>`;
    }
}

class FudieBreadcrumbItem extends HTMLElement {
    static get observedAttributes() { return ['href', 'active']; }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        // Styles for item
        const itemSheet = new CSSStyleSheet();
        itemSheet.replaceSync(`
            :host {
                display: inline-flex;
                align-items: center;
            }
            
            a {
                text-decoration: none;
                color: #6B7280;
                transition: color 0.2s;
            }
            a:hover { color: var(--primary, #FF4F5A); }
            
            span {
                color: #111827;
                font-weight: 500;
            }
            
            .separator {
                margin-left: 0.5rem;
                color: #D1D5DB;
            }
            
            :host([active]) .separator { display: none; }
        `);
        this.shadowRoot.adoptedStyleSheets = [itemSheet];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback() {
        this.render();
    }

    render() {
        const href = this.getAttribute('href');
        const active = this.hasAttribute('active'); // Last item usually
        const content = `<slot></slot>`;

        // We check if this is the last item via JS if we could, but 'active' attr is explicit.
        // Separator is shown unless active? Or usually separator is after every item EXCEPT last.
        // Implementation detail: Parent could control this, but let's just make it simple.
        // We'll add a "/" separator content in CSS or a span element.

        this.shadowRoot.innerHTML = `
            ${href && !active ? `<a href="${href}">${content}</a><i class="fas fa-chevron-right separator" style="font-size: 0.75rem;"></i>` : `<span>${content}</span>`}
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-breadcrumb', FudieBreadcrumb);
customElements.define('fudie-breadcrumb-item', FudieBreadcrumbItem);
