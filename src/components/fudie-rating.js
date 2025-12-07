const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .rating-container {
        display: inline-flex;
        gap: 0.25rem;
    }

    .star {
        font-size: 1.25rem;
        color: #D1D5DB; /* Gray 300 */
        cursor: pointer;
        transition: color 0.1s;
    }

    /* Interactive state using DOM manipulation is easier than pure CSS for specific fill */
    .star.filled {
        color: #F59E0B; /* Amber 500 */
    }

    :host([readonly]) .star {
        cursor: default;
    }

    :host([size="sm"]) .star { font-size: 1rem; }
    :host([size="lg"]) .star { font-size: 1.75rem; }
`);

class FudieRating extends HTMLElement {
    static get observedAttributes() {
        return ['value', 'max', 'readonly', 'size'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._hoverValue = 0;
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue !== newValue) {
            this.render();
        }
    }

    get value() { return parseInt(this.getAttribute('value')) || 0; }
    set value(val) { this.setAttribute('value', val); }

    handleHover(val) {
        if (this.hasAttribute('readonly')) return;
        this._hoverValue = val;
        this.updateStars();
    }

    handleLeave() {
        if (this.hasAttribute('readonly')) return;
        this._hoverValue = 0;
        this.updateStars();
    }

    handleClick(val) {
        if (this.hasAttribute('readonly')) return;
        this.value = val;
        this.dispatchEvent(new CustomEvent('change', {
            bubbles: true,
            composed: true,
            detail: { value: val }
        }));
    }

    updateStars() {
        const max = parseInt(this.getAttribute('max')) || 5;
        const current = this._hoverValue > 0 ? this._hoverValue : this.value;
        const container = this.shadowRoot.querySelector('.rating-container');

        if (!container) return; // Not rendered yet

        Array.from(container.children).forEach((star, index) => {
            if (index < current) {
                star.classList.add('filled');
                star.classList.remove('far'); // regular
                star.classList.add('fas');    // solid
            } else {
                star.classList.remove('filled');
                star.classList.add('far');
                star.classList.remove('fas');
            }
        });
    }

    render() {
        const max = parseInt(this.getAttribute('max')) || 5;

        this.shadowRoot.innerHTML = `
            <div class="rating-container">
                ${Array.from({ length: max }, (_, i) => `
                    <i class="star far fa-star" data-value="${i + 1}"></i>
                `).join('')}
            </div>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        const stars = this.shadowRoot.querySelectorAll('.star');
        stars.forEach(star => {
            const val = parseInt(star.dataset.value);
            star.onmouseenter = () => this.handleHover(val);
            star.onclick = () => this.handleClick(val);
        });

        this.shadowRoot.querySelector('.rating-container').onmouseleave = () => this.handleLeave();

        this.updateStars();
    }
}

customElements.define('fudie-rating', FudieRating);
