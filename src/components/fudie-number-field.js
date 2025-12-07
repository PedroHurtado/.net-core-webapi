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
        --input-radius: 0.75rem; /* rounded-xl approx */
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
        font-size: 0.75rem; /* text-[10px] or text-xs */
        font-weight: 700;
        color: var(--gray-500); /* text-gray-400 in updated designs mostly */
        text-transform: uppercase;
        margin-bottom: 0.25rem;
    }

    .input-wrapper {
        position: relative;
    }

    input {
        width: 100%;
        background-color: var(--input-bg);
        border: 1px solid var(--input-border);
        border-radius: var(--input-radius);
        padding: 0.5rem 0.75rem; /* px-3 py-2 */
        font-size: 0.875rem; /* text-sm */
        font-weight: 500;
        color: var(--input-text);
        outline: none;
        transition: border-color 0.2s, box-shadow 0.2s;
        font-family: inherit;
        text-align: right; /* Default for numbers based on admin-menu-item */
    }

    input:focus {
        border-color: var(--focus-ring);
    }

    input:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        background-color: #f3f4f6;
    }

    /* Error State */
    :host([error]) input {
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

class FudieNumberField extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'label', 'value', 'min', 'max', 'step', 'placeholder', 'required', 'disabled', 'error', 'hint'];
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
            const input = this.shadowRoot.querySelector('input');
            if (input && input.value !== newValue) {
                input.value = newValue;
            }
        } else {
            this.render();
        }
    }

    render() {
        const label = this.getAttribute('label') || '';
        const name = this.getAttribute('name') || '';
        const value = this.getAttribute('value') || '';
        const min = this.getAttribute('min') || '';
        const max = this.getAttribute('max') || '';
        const step = this.getAttribute('step') || '';
        const placeholder = this.getAttribute('placeholder') || '';
        const required = this.hasAttribute('required');
        const disabled = this.hasAttribute('disabled');
        const error = this.getAttribute('error') || '';
        const hint = this.getAttribute('hint') || '';

        // Check if structure exists
        if (this.shadowRoot.querySelector('.field-container')) {
            const input = this.shadowRoot.querySelector('input');
            const labelEl = this.shadowRoot.querySelector('label');
            const errorEl = this.shadowRoot.querySelector('.error-message');
            const hintEl = this.shadowRoot.querySelector('.hint-message');

            if (input) {
                if (input.name !== name) input.name = name;
                if (input.min !== min) input.min = min;
                if (input.max !== max) input.max = max;
                if (input.step !== step) input.step = step;
                if (input.placeholder !== placeholder) input.placeholder = placeholder;
                if (input.disabled !== disabled) input.disabled = disabled;
                if (input.required !== required) input.required = required;
                if (input.value !== value && document.activeElement !== this) input.value = value;
            }

            // Safely update or create label
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

        // Initial render
        this.shadowRoot.innerHTML = `
            <div class="field-container">
                ${label ? `<label for="input">${label}${required ? ' *' : ''}</label>` : ''}
                <div class="input-wrapper">
                    <input 
                        id="input"
                        type="number"
                        name="${name}"
                        value="${value}"
                        min="${min}"
                        max="${max}"
                        step="${step}"
                        placeholder="${placeholder}"
                        ${required ? 'required' : ''}
                        ${disabled ? 'disabled' : ''}
                    />
                    <slot name="icon-right"></slot>
                </div>
                <span class="message error-message">${error}</span>
                <span class="message hint-message">${hint}</span>
            </div>
        `;

        this.setupEventListeners();
    }

    setupEventListeners() {
        const input = this.shadowRoot.querySelector('input');
        if (!input) return;

        input.addEventListener('input', (e) => {
            this.setAttribute('value', e.target.value);
            this.dispatchEvent(new CustomEvent('input', { bubbles: true, composed: true, detail: { value: e.target.value } }));
        });

        input.addEventListener('change', (e) => {
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

customElements.define('fudie-number-field', FudieNumberField);
