const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --primary-light: #FFF1F2;
        --gray-100: #F3F4F6;
        --gray-200: #E5E7EB;
        --gray-500: #6B7280;
        --gray-900: #111827;
        --radius: 1rem;
    }

    .picker-container {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .label {
        font-size: 0.75rem; 
        font-weight: 700; 
        color: var(--gray-500); 
        text-transform: uppercase; 
        letter-spacing: 0.05em; 
        display: block;
        margin-bottom: 0.25rem;
    }

    .grid {
        display: grid;
        gap: 0.75rem;
        /* Columns default to 4 for simple items, typically overridden */
        grid-template-columns: repeat(var(--columns, 4), minmax(0, 1fr));
    }

    .picker-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 0.75rem;
        border-radius: var(--radius);
        border: 2px solid var(--gray-100);
        background-color: transparent;
        cursor: pointer;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        color: var(--gray-500);
        font-weight: 600;
        aspect-ratio: var(--aspect, 1/1);
        text-align: center;
        position: relative;
    }

    /* Hover */
    .picker-item:hover:not(.selected):not([disabled]) {
        border-color: rgba(255, 79, 90, 0.3); /* Primary with opacity */
        background-color: var(--primary-light);
        color: var(--primary);
    }

    /* Selected State */
    .picker-item.selected {
        border-color: var(--primary);
        background-color: var(--primary-light);
        color: var(--primary);
    }

    /* Selected State Override for 'fill' variant (like people numbers) */
    :host([variant="fill"]) .picker-item.selected {
        background-color: var(--primary);
        color: white;
        border-color: var(--primary);
        box-shadow: 0 10px 15px -3px rgba(255, 79, 90, 0.3);
        transform: scale(1.05);
    }

    .picker-item i {
        font-size: 1.25rem;
        margin-bottom: 0.25rem;
        transition: color 0.2s;
    }

    /* Specific Item Styling */
    .item-label {
        font-size: 0.75rem;
        line-height: 1.1;
    }

    :host([disabled]) .picker-item {
        opacity: 0.5;
        cursor: not-allowed;
    }
`);

class FudieGridPicker extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'label', 'options', 'value', 'multiple', 'columns', 'variant'];
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

    get options() {
        try {
            return JSON.parse(this.getAttribute('options') || '[]');
        } catch (e) {
            console.error('Invalid JSON for options', e);
            return [];
        }
    }

    get isMultiple() {
        return this.hasAttribute('multiple');
    }

    get currentValue() {
        const val = this.getAttribute('value');
        if (this.isMultiple) {
            try {
                return JSON.parse(val || '[]');
            } catch {
                return [];
            }
        }
        return val;
    }

    handleSelect(optionValue) {
        if (this.hasAttribute('disabled')) return;

        let newValue;
        if (this.isMultiple) {
            const current = this.currentValue || [];
            if (current.includes(optionValue)) {
                newValue = current.filter(v => v !== optionValue);
            } else {
                newValue = [...current, optionValue];
            }
            this.setAttribute('value', JSON.stringify(newValue));
        } else {
            // Toggle off if clicking selected? Typically radio behavior is "must select another".
            // But let's stick to standard radio behavior: clicking selected does nothing or stays selected.
            newValue = optionValue;
            this.setAttribute('value', newValue);
        }

        this.dispatchEvent(new CustomEvent('change', {
            bubbles: true,
            composed: true,
            detail: { value: newValue, name: this.getAttribute('name') }
        }));
    }

    render() {
        const label = this.getAttribute('label');
        const options = this.options;
        const currentVal = this.currentValue;
        const columns = this.getAttribute('columns') || '4'; // Default 4 cols
        const isFillVariant = this.getAttribute('variant') === 'fill';

        // Set CSS var for columns
        this.style.setProperty('--columns', columns);

        this.shadowRoot.innerHTML = `
            <div class="picker-container">
                ${label ? `<label class="label">${label}</label>` : ''}
                <div class="grid">
                    ${options.map(opt => {
            const isSelected = this.isMultiple
                ? (Array.isArray(currentVal) && currentVal.includes(opt.value))
                : String(currentVal) === String(opt.value);

            const iconClass = opt.icon ? `<i class="fas fa-${opt.icon}"></i>` : '';

            return `
                            <div class="picker-item ${isSelected ? 'selected' : ''}" 
                                 onclick="this.getRootNode().host.handleSelect('${opt.value}')">
                                ${iconClass}
                                ${opt.label ? `<span class="item-label">${opt.label}</span>` : ''}
                            </div>
                        `;
        }).join('')}
                </div>
            </div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-grid-picker', FudieGridPicker);
