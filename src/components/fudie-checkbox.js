const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --gray-200: #E5E7EB;
        --gray-400: #9CA3AF;
        --gray-500: #6B7280;
        --gray-900: #111827;
        
        --checkbox-size: 1.25rem;
        --checkbox-bg: white;
        --checkbox-border: var(--gray-200);
        --checkbox-checked: var(--primary);
        --checkmark-color: white;
    }

    * {
        box-sizing: border-box;
    }

    .wrapper {
        display: flex;
        align-items: flex-start; /* Align top for long text */
        gap: 0.75rem;
        cursor: pointer;
    }

    :host([disabled]) .wrapper {
        cursor: not-allowed;
        opacity: 0.6;
    }

    .checkbox {
        position: relative;
        width: var(--checkbox-size);
        height: var(--checkbox-size);
        background-color: var(--checkbox-bg);
        border: 2px solid var(--checkbox-border);
        border-radius: 0.375rem; /* rounded-md */
        transition: all 0.2s;
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        margin-top: 0.125rem; /* Align with text baseline approx */
    }

    .checkbox::after {
        content: '\\f00c'; /* FontAwesome check */
        font-family: 'Font Awesome 6 Free';
        font-weight: 900;
        color: var(--checkmark-color);
        font-size: 0.75rem;
        opacity: 0;
        transform: scale(0.5);
        transition: all 0.2s;
    }

    input {
        position: absolute;
        opacity: 0;
        width: 0;
        height: 0;
    }

    input:checked + .checkbox {
        background-color: var(--checkbox-checked);
        border-color: var(--checkbox-checked);
    }

    input:checked + .checkbox::after {
        opacity: 1;
        transform: scale(1);
    }

    input:focus-visible + .checkbox {
        outline: 2px solid var(--primary);
        outline-offset: 2px;
    }

    .label {
        font-size: 0.875rem;
        font-weight: 500;
        color: var(--gray-900);
        line-height: 1.5;
        user-select: none;
    }

    .description {
        display: block;
        font-size: 0.75rem;
        color: var(--gray-500);
        margin-top: 0.125rem;
    }
`);

class FudieCheckbox extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'checked', 'disabled', 'value', 'label', 'description'];
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

        if (name === 'checked') {
            const input = this.shadowRoot.querySelector('input');
            if (input) {
                input.checked = this.hasAttribute('checked');
            }
        } else {
            this.render();
        }
    }

    render() {
        const label = this.getAttribute('label') || '';
        const description = this.getAttribute('description') || '';
        const name = this.getAttribute('name') || '';
        const value = this.getAttribute('value') || 'on';
        const checked = this.hasAttribute('checked');
        const disabled = this.hasAttribute('disabled');

        // Check if structure exists
        if (this.shadowRoot.querySelector('.wrapper')) {
            const input = this.shadowRoot.querySelector('input');
            const labelEl = this.shadowRoot.querySelector('.label-text');
            const descEl = this.shadowRoot.querySelector('.description');

            if (input) {
                if (input.name !== name) input.name = name;
                if (input.value !== value) input.value = value;
                if (input.checked !== checked) input.checked = checked;
                if (input.disabled !== disabled) input.disabled = disabled;
            }

            if (labelEl) labelEl.textContent = label;
            if (descEl) {
                if (description) {
                    descEl.style.display = 'block';
                    descEl.textContent = description;
                } else {
                    descEl.style.display = 'none';
                }
            }
            return;
        }

        this.shadowRoot.innerHTML = `
            <label class="wrapper">
                <input 
                    type="checkbox"
                    name="${name}"
                    value="${value}"
                    ${checked ? 'checked' : ''}
                    ${disabled ? 'disabled' : ''}
                >
                <div class="checkbox"></div>
                <div class="label-container" ${label || description ? '' : 'style="display:none"'}>
                    <div class="label label-text">${label}</div>
                    <div class="description" ${description ? '' : 'style="display:none"'}>${description}</div>
                </div>
            </label>
        `;

        this.setupEventListeners();
    }

    setupEventListeners() {
        const input = this.shadowRoot.querySelector('input');
        if (!input) return;

        input.addEventListener('change', (e) => {
            if (e.target.checked) {
                this.setAttribute('checked', '');
            } else {
                this.removeAttribute('checked');
            }
            this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true, detail: { checked: e.target.checked, value: this.getAttribute('value') } }));
        });
    }

    get checked() {
        return this.hasAttribute('checked');
    }

    set checked(val) {
        if (val) {
            this.setAttribute('checked', '');
        } else {
            this.removeAttribute('checked');
        }
    }
}

customElements.define('fudie-checkbox', FudieCheckbox);
