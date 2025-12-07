const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        box-sizing: border-box;
    }

    *, *:before, *:after {
        box-sizing: border-box;
    }

    .search-container {
        position: relative;
        width: 100%;
    }

    .icon-left {
        position: absolute;
        left: 1rem;
        top: 50%;
        transform: translateY(-50%);
        color: #9CA3AF;
        pointer-events: none;
    }

    input {
        width: 100%;
        padding: 0.75rem 1rem 0.75rem 2.75rem; /* Left padding for icon */
        border: 1px solid #E5E7EB;
        border-radius: 9999px; /* Pill shape */
        font-size: 0.875rem;
        outline: none;
        transition: all 0.2s;
        color: #111827;
        background-color: #F9FAFB;
        font-family: inherit;
    }
    
    input:focus {
        background-color: white;
        border-color: var(--primary, #FF4F5A);
        box-shadow: 0 0 0 4px rgba(255, 79, 90, 0.1);
    }

    .clear-btn {
        position: absolute;
        right: 0.75rem;
        top: 50%;
        transform: translateY(-50%);
        background: none;
        border: none;
        color: #9CA3AF;
        cursor: pointer;
        padding: 0.25rem;
        display: none;
    }
    
    input:not(:placeholder-shown) + .clear-btn {
        display: block;
    }
    
    .clear-btn:hover {
        color: #4B5563;
    }
`);

class FudieSearch extends HTMLElement {
    static get observedAttributes() { return ['placeholder', 'value']; }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
    }

    render() {
        const placeholder = this.getAttribute('placeholder') || 'Buscar...';
        const value = this.getAttribute('value') || '';

        this.shadowRoot.innerHTML = `
            <div class="search-container">
                <i class="fas fa-search icon-left"></i>
                <input type="text" placeholder="${placeholder}" value="${value}">
                <button class="clear-btn"><i class="fas fa-times"></i></button>
            </div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        const input = this.shadowRoot.querySelector('input');
        const clearBtn = this.shadowRoot.querySelector('.clear-btn');

        input.addEventListener('input', (e) => {
            if (e.target.value) clearBtn.style.display = 'block';
            else clearBtn.style.display = 'none';
            // Debounce dispatch?
            this.dispatchEvent(new CustomEvent('search', { detail: { value: e.target.value } }));
        });

        clearBtn.addEventListener('click', () => {
            input.value = '';
            clearBtn.style.display = 'none';
            input.focus();
            this.dispatchEvent(new CustomEvent('search', { detail: { value: '' } }));
            this.dispatchEvent(new CustomEvent('clear'));
        });
    }
}

customElements.define('fudie-search', FudieSearch);
