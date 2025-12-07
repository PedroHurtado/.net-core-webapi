const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-block;
        vertical-align: middle;
    }

    .spinner {
        display: inline-block;
        border-radius: 50%;
        border-style: solid;
        border-color: currentColor;
        border-bottom-color: transparent;
        animation: spin 1s linear infinite;
    }

    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }

    /* Sizes */
    .size-sm {
        width: 1rem;
        height: 1rem;
        border-width: 2px;
    }

    .size-md {
        width: 1.5rem;
        height: 1.5rem;
        border-width: 2px;
    }

    .size-lg {
        width: 2.5rem;
        height: 2.5rem;
        border-width: 3px;
    }

    /* If variant is provided, use specific colors, otherwise inherit currentColor */
    .variant-primary { color: #FF4F5A; }
    .variant-white { color: #FFFFFF; }
    .variant-secondary { color: #1F2937; }
`);

class FudieSpinner extends HTMLElement {
    static get observedAttributes() {
        return ['size', 'variant'];
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
        if (oldValue !== newValue) {
            this.render();
        }
    }

    render() {
        const size = this.getAttribute('size') || 'md';
        const variant = this.getAttribute('variant') || '';

        this.shadowRoot.innerHTML = `
            <div class="spinner size-${size} variant-${variant}"></div>
        `;
    }
}

customElements.define('fudie-spinner', FudieSpinner);
