const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-flex;
        vertical-align: middle;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.25rem;
        padding: 0.25rem 0.5rem;
        border-radius: 9999px; /* Pill shape default */
        font-size: 0.75rem; /* text-xs */
        font-weight: 600;
        line-height: 1;
        white-space: nowrap;
        transition: all 0.2s;
        border: 1px solid transparent;
    }

    /* Variants */
    .variant-default {
        background-color: #F3F4F6;
        color: #374151;
        border-color: #E5E7EB;
    }

    .variant-primary {
        background-color: #FFF1F2;
        color: #E11D48; /* Darker version of primary */
        border-color: #FECDD3;
    }

    .variant-success {
        background-color: #ECFDF5;
        color: #059669;
        border-color: #A7F3D0;
    }

    .variant-warning {
        background-color: #FFFBEB;
        color: #D97706;
        border-color: #FDE68A;
    }

    .variant-danger {
        background-color: #FEF2F2;
        color: #DC2626;
        border-color: #FECACA;
    }

    .variant-info {
        background-color: #EFF6FF;
        color: #2563EB;
        border-color: #BFDBFE;
    }

    /* Sizes */
    .size-sm {
        padding: 0.125rem 0.375rem;
        font-size: 0.625rem;
    }

    .size-md {
        padding: 0.25rem 0.625rem;
        font-size: 0.75rem;
    }
    
    .size-lg {
        padding: 0.375rem 0.75rem;
        font-size: 0.875rem;
    }
    
    /* Shapes */
    .shape-square {
        border-radius: 0.375rem;
    }
`);

class FudieBadge extends HTMLElement {
    static get observedAttributes() {
        return ['variant', 'size', 'shape', 'label', 'icon'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue !== newValue) {
            this.render();
        }
    }

    render() {
        const variant = this.getAttribute('variant') || 'default';
        const size = this.getAttribute('size') || 'md';
        const shape = this.getAttribute('shape') || 'pill';
        const label = this.getAttribute('label') || this.textContent;
        const icon = this.getAttribute('icon');

        this.shadowRoot.innerHTML = `
            <span class="badge variant-${variant} size-${size} shape-${shape}">
                ${icon ? `<i class="fas fa-${icon}"></i>` : ''}
                ${label}
                <slot></slot>
            </span>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-badge', FudieBadge);
