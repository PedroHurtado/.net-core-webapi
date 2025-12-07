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
        
        --toggle-width: 3rem;
        --toggle-height: 1.5rem;
        --toggle-bg: var(--gray-200);
        --toggle-checked: var(--primary);
        --thumb-size: 1.125rem;
        --thumb-color: white;
    }

    * {
        box-sizing: border-box;
    }

    .wrapper {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        cursor: pointer;
    }

    :host([disabled]) .wrapper {
        cursor: not-allowed;
        opacity: 0.6;
    }

    .switch {
        position: relative;
        width: var(--toggle-width);
        height: var(--toggle-height);
        background-color: var(--toggle-bg);
        border-radius: 9999px;
        transition: background-color 0.2s;
        flex-shrink: 0;
    }

    .thumb {
        position: absolute;
        top: 50%;
        left: 0.1875rem; /* 3px */
        width: var(--thumb-size);
        height: var(--thumb-size);
        background-color: var(--thumb-color);
        border-radius: 50%;
        transform: translateY(-50%);
        transition: transform 0.2s;
        box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    }

    input {
        position: absolute;
        opacity: 0;
        width: 0;
        height: 0;
    }

    input:checked + .switch {
        background-color: var(--toggle-checked);
    }

    input:checked + .switch .thumb {
        transform: translate(calc(var(--toggle-width) - var(--thumb-size) - 0.375rem), -50%);
    }

    input:focus-visible + .switch {
        outline: 2px solid var(--primary);
        outline-offset: 2px;
    }

    .label {
        font-size: 0.875rem;
        font-weight: 500;
        color: var(--gray-900);
        user-select: none;
    }

    .description {
        display: block;
        font-size: 0.75rem;
        color: var(--gray-500);
        margin-top: 0.125rem;
    }
`);

class FudieToggle extends HTMLElement {
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
                <div class="switch">
                    <div class="thumb"></div>
                </div>
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

customElements.define('fudie-toggle', FudieToggle);
