const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --gray-50: #F9FAFB;
        --gray-200: #E5E7EB;
        --gray-400: #9CA3AF;
        --gray-500: #6B7280;
        --gray-900: #111827;
        --red-500: #EF4444;
        
        --input-bg: var(--gray-50);
        --input-border: var(--gray-200);
        --input-text: var(--gray-900);
        --input-radius: 0.75rem;
        --focus-ring: var(--primary);
    }

    * {
        box-sizing: border-box;
    }

    .field-container {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }

    label {
        display: block;
        font-size: 0.75rem;
        font-weight: 700;
        color: var(--gray-500);
        text-transform: uppercase;
        margin-bottom: 0.25rem;
    }

    textarea {
        width: 100%;
        background-color: var(--input-bg);
        border: 1px solid var(--input-border);
        border-radius: var(--input-radius);
        padding: 0.625rem 1rem;
        font-size: 1rem;
        font-weight: 500;
        color: var(--input-text);
        outline: none;
        transition: border-color 0.2s, box-shadow 0.2s;
        font-family: inherit;
        resize: none; /* As per design in admin-menu-item */
        min-height: 80px;
    }

    textarea:focus {
        border-color: var(--focus-ring);
    }

    textarea:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        background-color: #f3f4f6;
    }

    /* Error State */
    :host([error]) textarea {
        border-color: var(--red-500);
    }

    :host([error]) label {
        color: var(--red-500);
    }

    .message {
        font-size: 0.75rem;
        margin-top: 0.25rem;
        min-height: 1rem;
    }

    .error-message {
        color: var(--red-500);
        display: none;
    }

    .hint-message {
        color: var(--gray-400);
    }

    :host([error]) .error-message {
        display: block;
    }

    :host([error]) .hint-message {
        display: none;
    }
`);

class FudieTextareaField extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'label', 'value', 'placeholder', 'required', 'disabled', 'error', 'hint', 'rows'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._internals = this.attachInternals ? this.attachInternals() : null;
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue) return;

        if (name === 'value') {
            const textarea = this.shadowRoot.querySelector('textarea');
            if (textarea && textarea.value !== newValue) {
                textarea.value = newValue;
            }
        } else {
            this.render();
        }
    }

    render() {
        const label = this.getAttribute('label') || '';
        const name = this.getAttribute('name') || '';
        const value = this.getAttribute('value') || '';
        const placeholder = this.getAttribute('placeholder') || '';
        const rows = this.getAttribute('rows') || '3';
        const required = this.hasAttribute('required');
        const disabled = this.hasAttribute('disabled');
        const error = this.getAttribute('error') || '';
        const hint = this.getAttribute('hint') || '';

        // Check if structure exists
        if (this.shadowRoot.querySelector('.field-container')) {
            const textarea = this.shadowRoot.querySelector('textarea');
            const labelEl = this.shadowRoot.querySelector('label');
            const errorEl = this.shadowRoot.querySelector('.error-message');
            const hintEl = this.shadowRoot.querySelector('.hint-message');

            if (textarea) {
                if (textarea.name !== name) textarea.name = name;
                if (textarea.rows !== rows) textarea.rows = rows;
                if (textarea.placeholder !== placeholder) textarea.placeholder = placeholder;
                if (textarea.disabled !== disabled) textarea.disabled = disabled;
                if (textarea.required !== required) textarea.required = required;
                // Preserve value if focused
                if (textarea.value !== value && document.activeElement !== this) textarea.value = value;
            }

            if (label) {
                if (!labelEl) {
                    const newLabel = document.createElement('label');
                    newLabel.setAttribute('for', 'input');
                    newLabel.textContent = label + (required ? ' *' : '');
                    this.shadowRoot.querySelector('.field-container').prepend(newLabel);
                } else {
                    labelEl.textContent = label + (required ? ' *' : '');
                }
            } else if (labelEl) {
                labelEl.remove();
            }

            if (errorEl) errorEl.textContent = error;
            if (hintEl) hintEl.textContent = hint;

            return;
        }

        this.shadowRoot.innerHTML = `
            <div class="field-container">
                ${label ? `<label for="input">${label}${required ? ' *' : ''}</label>` : ''}
                <textarea 
                    id="input"
                    name="${name}"
                    rows="${rows}"
                    placeholder="${placeholder}"
                    ${required ? 'required' : ''}
                    ${disabled ? 'disabled' : ''}
                >${value}</textarea>
                <span class="message error-message">${error}</span>
                <span class="message hint-message">${hint}</span>
            </div>
        `;

        this.setupEventListeners();
    }

    setupEventListeners() {
        const textarea = this.shadowRoot.querySelector('textarea');
        if (!textarea) return;

        textarea.addEventListener('input', (e) => {
            this.setAttribute('value', e.target.value);
            this.dispatchEvent(new CustomEvent('input', { bubbles: true, composed: true, detail: { value: e.target.value } }));
        });

        textarea.addEventListener('change', (e) => {
            this.setAttribute('value', e.target.value);
            this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true, detail: { value: e.target.value } }));
        });
    }

    get value() {
        return this.getAttribute('value');
    }

    set value(val) {
        this.setAttribute('value', val);
    }
}

customElements.define('fudie-textarea-field', FudieTextareaField);
