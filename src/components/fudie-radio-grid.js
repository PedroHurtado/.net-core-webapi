const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --gray-200: #E5E7EB;
        --gray-50: #F9FAFB;
        --gray-900: #111827;
        --gray-500: #6B7280;
    }

    .field-container {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    label.main-label {
        font-size: 0.875rem;
        font-weight: 600;
        color: var(--gray-900);
    }

    .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
        gap: 1rem;
    }

    .radio-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 1.25rem;
        border: 2px solid var(--gray-200);
        border-radius: 1rem;
        cursor: pointer;
        transition: all 0.2s;
        background-color: white;
        position: relative;
        text-align: center;
        gap: 0.75rem;
    }

    .radio-card:hover {
        border-color: var(--gray-500);
        background-color: var(--gray-50);
    }

    .radio-card.selected {
        border-color: var(--primary);
        background-color: #FFF1F2; /* primary-50 approx */
        color: var(--primary);
    }

    /* Hidden Native Input */
    input[type="radio"] {
        display: none;
    }

    .icon {
        font-size: 1.5rem;
        color: var(--gray-500);
        transition: color 0.2s;
    }

    .radio-card.selected .icon {
        color: var(--primary);
    }

    .card-label {
        font-weight: 600;
        font-size: 0.875rem;
        color: var(--gray-900);
    }

    .radio-card.selected .card-label {
        color: var(--primary);
    }

    .check-badge {
        position: absolute;
        top: 0.5rem;
        right: 0.5rem;
        width: 1.25rem;
        height: 1.25rem;
        background-color: var(--primary);
        color: white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.75rem;
        opacity: 0;
        transform: scale(0.5);
        transition: all 0.2s;
    }

    .radio-card.selected .check-badge {
        opacity: 1;
        transform: scale(1);
    }
`);

class FudieRadioGrid extends HTMLElement {
    static get observedAttributes() {
        return ['name', 'label', 'value', 'options'];
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

    getOptions() {
        try {
            return JSON.parse(this.getAttribute('options') || '[]');
        } catch (e) {
            console.error('Invalid options JSON', e);
            return [];
        }
    }

    render() {
        const options = this.getOptions();
        const label = this.getAttribute('label') || '';
        const name = this.getAttribute('name') || 'radio-group';
        const currentValue = this.getAttribute('value');

        // Check if options changed or structure missing
        // For simplicity in this grid which involves many children, we might re-render innerHTML 
        // unless we want to do fine-grained DOM updates. Given the list size is usually small, innerHTML is fine.

        this.shadowRoot.innerHTML = `
            <div class="field-container">
                ${label ? `<label class="main-label">${label}</label>` : ''}
                <div class="grid">
                    ${options.map((opt) => {
            const isSelected = String(opt.value) === String(currentValue);
            return `
                        <div class="radio-card ${isSelected ? 'selected' : ''}" data-value="${opt.value}" onclick="this.getRootNode().host.handleSelection('${opt.value}')">
                            ${opt.icon ? `<i class="${opt.icon.startsWith('fa') ? 'fas ' + opt.icon : 'fas fa-' + opt.icon} icon"></i>` : ''}
                            <span class="card-label">${opt.label}</span>
                            <div class="check-badge"><i class="fas fa-check"></i></div>
                        </div>
                        `;
        }).join('')}
                </div>
            </div>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }

    handleSelection(val) {
        if (this.getAttribute('disabled') !== null) return;

        this.setAttribute('value', val);
        this.dispatchEvent(new CustomEvent('change', {
            bubbles: true,
            composed: true,
            detail: { value: val }
        }));
    }
}

customElements.define('fudie-radio-grid', FudieRadioGrid);
