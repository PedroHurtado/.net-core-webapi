const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --gray-200: #E5E7EB;
        --gray-500: #6B7280;
        --gray-700: #374151;
        --gray-900: #111827;
        
        --bar-bg: var(--gray-200);
        --fill-color: var(--primary);
        --height: 0.5rem; /* h-2 default */
        --radius: 9999px;
    }

    .container {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 0.875rem; /* text-sm */
        font-weight: 500;
        color: var(--gray-700);
    }

    .label {
        color: var(--gray-900);
    }

    .value-text {
        color: var(--gray-500);
    }

    .progress-track {
        width: 100%;
        background-color: var(--bar-bg);
        border-radius: var(--radius);
        overflow: hidden;
        height: var(--height);
    }

    .progress-fill {
        height: 100%;
        background-color: var(--fill-color);
        border-radius: var(--radius);
        transition: width 0.3s ease-in-out;
        width: 0%;
    }

    /* Sizes */
    :host([size="sm"]) { --height: 0.25rem; }
    :host([size="md"]) { --height: 0.5rem; }
    :host([size="lg"]) { --height: 0.75rem; }
    :host([size="xl"]) { --height: 1rem; }

    /* Variants */
    :host([variant="primary"]) { --fill-color: #FF4F5A; }
    :host([variant="success"]) { --fill-color: #10B981; }
    :host([variant="warning"]) { --fill-color: #F59E0B; }
    :host([variant="info"]) { --fill-color: #3B82F6; }
    :host([variant="danger"]) { --fill-color: #EF4444; }

    /* Striped & Animated */
    :host([striped]) .progress-fill {
        background-image: linear-gradient(45deg,rgba(255,255,255,.15) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.15) 50%,rgba(255,255,255,.15) 75%,transparent 75%,transparent);
        background-size: 1rem 1rem;
    }

    :host([animated]) .progress-fill {
        animation: progress-bar-stripes 1s linear infinite;
    }

    @keyframes progress-bar-stripes {
        from { background-position: 1rem 0; }
        to { background-position: 0 0; }
    }
`);

class FudieProgress extends HTMLElement {
    static get observedAttributes() {
        return ['value', 'max', 'label', 'show-value', 'size', 'variant', 'striped', 'animated'];
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
        const value = parseFloat(this.getAttribute('value') || 0);
        const max = parseFloat(this.getAttribute('max') || 100);
        const label = this.getAttribute('label');
        const showValue = this.hasAttribute('show-value');

        let percentage = (value / max) * 100;
        if (percentage < 0) percentage = 0;
        if (percentage > 100) percentage = 100;

        // Clean text for display
        const displayValue = Math.round(percentage) + '%';

        // Check internal structure
        if (this.shadowRoot.querySelector('.container')) {
            const fill = this.shadowRoot.querySelector('.progress-fill');
            const valueText = this.shadowRoot.querySelector('.value-text');
            const labelEl = this.shadowRoot.querySelector('.label');
            const header = this.shadowRoot.querySelector('.header');

            if (fill) fill.style.width = `${percentage}%`;
            if (valueText) {
                valueText.textContent = displayValue;
                valueText.style.display = showValue ? 'block' : 'none';
            }
            if (labelEl) {
                labelEl.textContent = label || '';
            }

            // Toggle header visibility if no label and no showValue
            if (!label && !showValue) {
                if (header) header.style.display = 'none';
            } else {
                if (header) header.style.display = 'flex';
            }

            return;
        }

        this.shadowRoot.innerHTML = `
            <div class="container">
                <div class="header" style="${(!label && !showValue) ? 'display: none;' : ''}">
                    <span class="label">${label || ''}</span>
                    <span class="value-text" style="${showValue ? '' : 'display: none;'}">${displayValue}</span>
                </div>
                <div class="progress-track">
                    <div class="progress-fill" style="width: ${percentage}%"></div>
                </div>
            </div>
        `;
    }
}

customElements.define('fudie-progress', FudieProgress);
