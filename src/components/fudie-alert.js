const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        margin-bottom: 1rem;
    }

    .alert {
        display: flex;
        align-items: flex-start;
        padding: 1rem;
        border-radius: 0.75rem;
        border: 1px solid transparent;
        position: relative;
    }

    .icon-wrapper {
        flex-shrink: 0;
        margin-right: 0.75rem;
        font-size: 1.25rem;
    }

    .content {
        flex: 1;
        font-size: 0.875rem;
        line-height: 1.5;
    }

    .title {
        font-weight: 700;
        margin-bottom: 0.25rem;
        display: block;
    }

    .close-btn {
        background: transparent;
        border: none;
        cursor: pointer;
        padding: 0.25rem;
        margin-left: 0.5rem;
        opacity: 0.6;
        transition: opacity 0.2s;
        font-size: 1rem;
        display: none;
    }

    .close-btn:hover {
        opacity: 1;
    }

    :host([dismissible]) .close-btn {
        display: block;
    }

    /* Variants */
    .variant-info {
        background-color: #EFF6FF;
        border-color: #BFDBFE;
        color: #1E3A8A;
    }
    .variant-info .icon-wrapper { color: #3B82F6; }

    .variant-success {
        background-color: #ECFDF5;
        border-color: #A7F3D0;
        color: #064E3B;
    }
    .variant-success .icon-wrapper { color: #10B981; }

    .variant-warning {
        background-color: #FFFBEB;
        border-color: #FDE68A;
        color: #78350F;
    }
    .variant-warning .icon-wrapper { color: #F59E0B; }

    .variant-danger {
        background-color: #FEF2F2;
        border-color: #FECACA;
        color: #7F1D1D;
    }
    .variant-danger .icon-wrapper { color: #EF4444; }

    /* Hide animation */
    :host(.hidden) {
        display: none;
    }
`);

class FudieAlert extends HTMLElement {
    static get observedAttributes() {
        return ['variant', 'title', 'icon', 'dismissible'];
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

    dismiss() {
        this.style.opacity = '0';
        this.style.transition = 'opacity 0.3s ease';
        setTimeout(() => {
            this.classList.add('hidden');
            this.dispatchEvent(new CustomEvent('dismiss', { bubbles: true, composed: true }));
        }, 300);
    }

    render() {
        const variant = this.getAttribute('variant') || 'info';
        const title = this.getAttribute('title');
        const icon = this.getAttribute('icon');
        const dismissible = this.hasAttribute('dismissible');

        // Default icons if not provided
        let iconClass = icon;
        if (!icon) {
            switch (variant) {
                case 'success': iconClass = 'check-circle'; break;
                case 'warning': iconClass = 'exclamation-triangle'; break;
                case 'danger': iconClass = 'exclamation-circle'; break;
                default: iconClass = 'info-circle';
            }
        }

        this.shadowRoot.innerHTML = `
            <div class="alert variant-${variant}">
                <div class="icon-wrapper">
                    <i class="fas fa-${iconClass}"></i>
                </div>
                <div class="content">
                    ${title ? `<span class="title">${title}</span>` : ''}
                    <slot></slot>
                </div>
                <button class="close-btn" aria-label="Close">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        // Event listener for dismiss
        if (dismissible) {
            const btn = this.shadowRoot.querySelector('.close-btn');
            if (btn) btn.onclick = () => this.dismiss();
        }
    }
}

customElements.define('fudie-alert', FudieAlert);
