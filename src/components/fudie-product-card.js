const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .card {
        background: white;
        border-radius: 1.5rem;
        border: 1px solid #F3F4F6;
        overflow: hidden;
        transition: transform 0.2s, box-shadow 0.2s;
        height: 100%;
        display: flex;
        flex-direction: column;
    }

    .card:hover {
        transform: translateY(-4px);
        box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    }

    .image-container {
        position: relative;
        padding-top: 60%; /* Aspect Ratio */
        background-color: #F3F4F6;
    }

    .image {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .badge {
        position: absolute;
        top: 1rem;
        right: 1rem;
        background: rgba(255, 255, 255, 0.9);
        padding: 0.25rem 0.75rem;
        border-radius: 9999px;
        font-size: 0.75rem;
        font-weight: 700;
        color: #111827;
        backdrop-filter: blur(4px);
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .content {
        padding: 1.5rem;
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .title {
        font-size: 1.125rem;
        font-weight: 700;
        margin: 0 0 0.5rem 0;
        color: #111827;
    }

    .desc {
        font-size: 0.875rem;
        color: #6B7280;
        margin-bottom: 1.5rem;
        line-height: 1.5;
        flex: 1; /* Push footer down */
    }

    .footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-top: auto;
    }

    .price {
        font-size: 1.25rem;
        font-weight: 800;
        color: var(--primary, #FF4F5A);
    }
`);

class FudieProductCard extends HTMLElement {
    static get observedAttributes() {
        return ['image', 'title', 'description', 'price', 'tag'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback() {
        this.render();
    }

    render() {
        const image = this.getAttribute('image') || '';
        const title = this.getAttribute('title') || 'Product Name';
        const desc = this.getAttribute('description') || '';
        const price = this.getAttribute('price') || '0.00';
        const tag = this.getAttribute('tag');

        this.shadowRoot.innerHTML = `
            <article class="card">
                <div class="image-container">
                    ${image ? `<img src="${image}" class="image" alt="${title}">` : ''}
                    ${tag ? `<span class="badge">${tag}</span>` : ''}
                </div>
                <div class="content">
                    <h3 class="title">${title}</h3>
                    <p class="desc">${desc}</p>
                    <div class="footer">
                        <span class="price">${price}€</span>
                        <slot name="action"></slot>
                    </div>
                </div>
            </article>
        `;
    }
}

customElements.define('fudie-product-card', FudieProductCard);
