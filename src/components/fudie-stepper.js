const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-flex;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .stepper {
        display: flex;
        align-items: center;
        background-color: #F9FAFB;
        border: 1px solid #E5E7EB;
        border-radius: 0.75rem;
        padding: 0.25rem;
    }

    .btn {
        width: 2rem;
        height: 2rem;
        display: flex;
        align-items: center;
        justify-content: center;
        background: white;
        border: 1px solid #E5E7EB;
        border-radius: 0.5rem;
        cursor: pointer;
        color: #374151;
        transition: all 0.2s;
        font-size: 0.75rem;
    }

    .btn:hover:not(:disabled) {
        border-color: #FF4F5A;
        color: #FF4F5A;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);
    }

    .btn:active:not(:disabled) {
        transform: translateY(1px);
    }

    .btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
        background-color: #F3F4F6;
    }

    .value-display {
        font-weight: 700;
        min-width: 2.5rem;
        text-align: center;
        font-size: 1rem;
        color: #111827;
        user-select: none;
    }

    :host([size="sm"]) .btn { width: 1.5rem; height: 1.5rem; font-size: 0.625rem; }
    :host([size="sm"]) .value-display { font-size: 0.875rem; min-width: 2rem; }

    :host([size="lg"]) .btn { width: 2.5rem; height: 2.5rem; font-size: 1rem; }
    :host([size="lg"]) .value-display { font-size: 1.25rem; min-width: 3rem; }
`);

class FudieStepper extends HTMLElement {
    static get observedAttributes() {
        return ['value', 'min', 'max', 'step', 'size', 'disabled'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._value = 0;
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue !== newValue) {
            if (name === 'value') this._value = parseFloat(newValue) || 0;
            this.render();
        }
    }

    get value() { return this._value; }
    set value(val) {
        const min = parseFloat(this.getAttribute('min')) || -Infinity;
        const max = parseFloat(this.getAttribute('max')) || Infinity;
        let newValue = Math.max(min, Math.min(max, val));

        if (this._value !== newValue) {
            this._value = newValue;
            this.setAttribute('value', newValue);
            this.dispatchEvent(new CustomEvent('change', {
                bubbles: true,
                composed: true,
                detail: { value: this._value }
            }));
            this.render();
        }
    }

    updateValue(delta) {
        if (this.hasAttribute('disabled')) return;
        const step = parseFloat(this.getAttribute('step')) || 1;
        this.value = this._value + (delta * step);
    }

    render() {
        const min = parseFloat(this.getAttribute('min')) || -Infinity;
        const max = parseFloat(this.getAttribute('max')) || Infinity;
        const disabled = this.hasAttribute('disabled');

        this.shadowRoot.innerHTML = `
            <div class="stepper">
                <button class="btn minus" ${disabled || this._value <= min ? 'disabled' : ''}>
                    <i class="fas fa-minus"></i>
                </button>
                <div class="value-display">${this._value}</div>
                <button class="btn plus" ${disabled || this._value >= max ? 'disabled' : ''}>
                    <i class="fas fa-plus"></i>
                </button>
            </div>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        this.shadowRoot.querySelector('.minus').onclick = () => this.updateValue(-1);
        this.shadowRoot.querySelector('.plus').onclick = () => this.updateValue(1);
    }
}

customElements.define('fudie-stepper', FudieStepper);
