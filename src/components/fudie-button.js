const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --primary-dark: #E63E49;
        --gray-50: #F9FAFB;
        --gray-200: #E5E7EB;
        --gray-700: #374151;
        --white: #FFFFFF;
        --danger: #EF4444;
        --danger-light: #FEF2F2;
    }

    button {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        cursor: pointer;
        font-weight: 600;
        font-family: inherit;
        border-radius: 0.75rem; /* rounded-xl */
        transition: all 0.2s;
        border: none;
        outline: none;
        width: 100%;
    }

    /* Variants */
    .variant-primary {
        background-color: var(--primary);
        color: var(--white);
    }
    .variant-primary:hover:not(:disabled) {
        background-color: var(--primary-dark);
    }

    .variant-secondary {
        background-color: var(--white);
        border: 1px solid var(--gray-200);
        color: var(--gray-700);
    }
    .variant-secondary:hover:not(:disabled) {
        background-color: var(--gray-50);
        border-color: var(--gray-200);
    }

    .variant-danger {
        background-color: var(--danger-light);
        color: var(--danger);
    }
    .variant-danger:hover:not(:disabled) {
        background-color: #FEE2E2;
    }

    .variant-ghost {
        background-color: transparent;
        color: var(--gray-700);
    }
    .variant-ghost:hover:not(:disabled) {
        background-color: var(--gray-50);
    }

    /* Sizes */
    .size-sm {
        padding: 0.375rem 0.75rem;
        font-size: 0.875rem;
    }

    .size-md {
        padding: 0.625rem 1.25rem;
        font-size: 1rem;
    }

    .size-lg {
        padding: 0.75rem 1.5rem;
        font-size: 1.125rem;
    }
    
    .size-icon {
        padding: 0.5rem;
        aspect-ratio: 1;
    }

    /* Disabled */
    button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
    
    /* Full width option */
    :host([full-width]) button {
        width: 100%;
    }
    
    :host(:not([full-width])) button {
        width: auto;
    }
`);

class FudieButton extends HTMLElement {
    static get observedAttributes() {
        return ['variant', 'size', 'disabled', 'type', 'full-width'];
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
        if (oldValue === newValue) return;
        this.render();
    }

    render() {
        const variant = this.getAttribute('variant') || 'primary';
        const size = this.getAttribute('size') || 'md';
        const type = this.getAttribute('type') || 'button';
        const disabled = this.hasAttribute('disabled');

        // Check if structure exists
        let button = this.shadowRoot.querySelector('button');
        if (!button) {
            this.shadowRoot.innerHTML = `
                <button part="button">
                    <slot name="icon-left"></slot>
                    <slot></slot>
                    <slot name="icon-right"></slot>
                </button>
            `;
            button = this.shadowRoot.querySelector('button');
            this.updateButton(button, variant, size, type, disabled);

            // Re-bind click event usually handled by light DOM usage, 
            // but if we are a button wrapper, we might want to ensure form submission works if type=submit.
            // For native web components without FormAssociated mixed-in fully, specific logic is needed for 'submit'.
            // For now, simple button style.
            if (type === 'submit') {
                button.addEventListener('click', () => {
                    const form = this.closest('form');
                    if (form) form.requestSubmit();
                });
            }
        } else {
            this.updateButton(button, variant, size, type, disabled);
        }
    }

    updateButton(button, variant, size, type, disabled) {
        button.className = `variant-${variant} size-${size}`;
        button.disabled = disabled;
        button.type = type; // Note: 'type' on internal button doesn't affect parent form unless we handle it.
    }
}

customElements.define('fudie-button', FudieButton);
