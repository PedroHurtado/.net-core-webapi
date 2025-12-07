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

    .select-wrapper {
        position: relative;
    }

    /* Custom Arrow */
    .select-wrapper::after {
        content: '\\f078'; /* FontAwesome chevron-down */
        font-family: 'Font Awesome 6 Free';
        font-weight: 900;
        position: absolute;
        right: 1rem;
        top: 50%;
        transform: translateY(-50%);
        pointer-events: none;
        color: var(--gray-500);
        font-size: 0.75rem;
    }

    select {
        width: 100%;
        background-color: var(--input-bg);
        border: 1px solid var(--input-border);
        border-radius: var(--input-radius);
        padding: 0.625rem 2.5rem 0.625rem 1rem; /* Extra padding right for arrow */
        font-size: 1rem;
        font-weight: 500;
        color: var(--input-text);
        outline: none;
        transition: border-color 0.2s, box-shadow 0.2s;
        font-family: inherit;
        appearance: none; /* Remove default arrow */
        cursor: pointer;
    }

    select:focus {
        border-color: var(--focus-ring);
    }

    select:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        background-color: #f3f4f6;
    }

    /* Error State */
    :host([error]) select {
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

class FudieSelectField extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'label', 'value', 'placeholder', 'required', 'disabled', 'error', 'hint', 'options'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._internals = this.attachInternals ? this.attachInternals() : null;
        this._options = [];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue) return;

        if (name === 'value') {
            const select = this.shadowRoot.querySelector('select');
            if (select && select.value !== newValue) {
                select.value = newValue;
            }
        } else if (name === 'options') {
            try {
                this._options = JSON.parse(newValue);
                this.render(); // Re-render options
            } catch (e) {
                console.error('Invalid JSON for options attribute', e);
            }
        } else {
            this.render();
        }
    }

    /**
     * Helper to render options HTML
     */
    getOptionsHTML() {
        // If placeholder provided, add empty option
        const placeholder = this.getAttribute('placeholder');
        let html = placeholder ? `<option value="" disabled selected>${placeholder}</option>` : '';

        const currentValue = this.getAttribute('value');

        this._options.forEach(opt => {
            // Support both simple strings and objects {label, value}
            const label = typeof opt === 'object' ? opt.label : opt;
            const value = typeof opt === 'object' ? opt.value : opt;
            const isSelected = String(value) === String(currentValue) ? 'selected' : '';

            html += `<option value="${value}" ${isSelected}>${label}</option>`;
        });

        return html;
    }

    render() {
        const label = this.getAttribute('label') || '';
        const name = this.getAttribute('name') || '';
        const required = this.hasAttribute('required');
        const disabled = this.hasAttribute('disabled');
        const error = this.getAttribute('error') || '';
        const hint = this.getAttribute('hint') || '';

        // Check if structure exists
        if (this.shadowRoot.querySelector('.field-container')) {
            const select = this.shadowRoot.querySelector('select');
            const labelEl = this.shadowRoot.querySelector('label');
            const errorEl = this.shadowRoot.querySelector('.error-message');
            const hintEl = this.shadowRoot.querySelector('.hint-message');

            if (select) {
                if (select.name !== name) select.name = name;
                if (select.disabled !== disabled) select.disabled = disabled;
                if (select.required !== required) select.required = required;
                // Re-render options if needed or just update value
                // Since options might change, we might need to fully replace innerHTML of select
                const newOptionsHTML = this.getOptionsHTML();
                if (select.innerHTML !== newOptionsHTML) {
                    select.innerHTML = newOptionsHTML;
                }
                // Ensure value is set after options update
                const val = this.getAttribute('value');
                if (val && select.value !== val) select.value = val;
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
                <div class="select-wrapper">
                    <select 
                        id="input"
                        name="${name}"
                        ${required ? 'required' : ''}
                        ${disabled ? 'disabled' : ''}
                    >
                    ${this.getOptionsHTML()}
                    </select>
                </div>
                <span class="message error-message">${error}</span>
                <span class="message hint-message">${hint}</span>
            </div>
        `;

        this.setupEventListeners();
    }

    setupEventListeners() {
        const select = this.shadowRoot.querySelector('select');
        if (!select) return;

        select.addEventListener('change', (e) => {
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

    set options(val) {
        this._options = val;
        this.render();
    }
}

customElements.define('fudie-select-field', FudieSelectField);
