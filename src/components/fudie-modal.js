const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: contents;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --overlay-bg: rgba(0, 0, 0, 0.5);
        --modal-bg: #ffffff;
        --radius: 1.5rem;
        --z-index: 50;
    }

    .overlay {
        position: fixed;
        inset: 0;
        background: var(--overlay-bg);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: var(--z-index);
        opacity: 0;
        visibility: hidden;
        transition: all 0.2s ease-in-out;
        backdrop-filter: blur(4px);
        padding: 1rem;
    }

    :host([open]) .overlay {
        opacity: 1;
        visibility: visible;
    }

    .modal {
        background: var(--modal-bg);
        border-radius: var(--radius);
        width: 100%;
        max-height: 90vh;
        display: flex;
        flex-direction: column;
        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
        transform: scale(0.95) translateY(10px);
        opacity: 0;
        transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
        overflow: hidden;
    }

    :host([open]) .modal {
        transform: scale(1) translateY(0);
        opacity: 1;
    }

    /* Sizes */
    :host([size="sm"]) .modal { max-width: 24rem; }
    :host([size="lg"]) .modal { max-width: 48rem; }
    :host([size="full"]) .modal { max-width: 100%; height: 100%; border-radius: 0; }
    :host(:not([size])) .modal, :host([size="md"]) .modal { max-width: 32rem; }

    /* Header */
    .header {
        padding: 1.5rem;
        border-bottom: 1px solid #F3F4F6;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .title {
        font-size: 1.25rem;
        font-weight: 700;
        color: #111827;
        margin: 0;
        line-height: 1.2;
    }

    .close-btn {
        background: transparent;
        border: none;
        color: #9CA3AF;
        cursor: pointer;
        padding: 0;
        border-radius: 50%;
        transition: all 0.2s;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
    }

    .close-btn:hover {
        background-color: #F3F4F6;
        color: var(--primary, #FF4F5A);
    }

    /* Body */
    .body {
        padding: 1.5rem;
        overflow-y: auto;
        color: #4B5563; /* text-gray-600 */
        line-height: 1.5;
    }

    /* Footer */
    .footer {
        padding: 1.25rem 1.5rem;
        border-top: 1px solid #F3F4F6;
        display: flex;
        justify-content: flex-end;
        gap: 0.75rem;
        background-color: #F9FAFB;
    }
`);

class FudieModal extends HTMLElement {
    static get observedAttributes() {
        return ['open', 'title', 'size', 'close-on-overlay'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
    }

    connectedCallback() {
        this.render();
        this.setupEvents();
    }

    setupEvents() {
        this.shadowRoot.addEventListener('click', (e) => {
            if (e.target.classList.contains('overlay')) {
                // Check if close-on-overlay is explicitly disabled
                if (this.getAttribute('close-on-overlay') !== 'false') {
                    this.close();
                }
            }
        });

        // Close on escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isOpen) {
                this.close();
            }
        });
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue) return;

        if (name === 'open') {
            // Handle body scroll lock? 
            if (newValue !== null) {
                document.body.style.overflow = 'hidden';
                this.dispatchEvent(new CustomEvent('fudie-open'));
            } else {
                document.body.style.overflow = '';
                this.dispatchEvent(new CustomEvent('fudie-close'));
            }
        }

        // Re-render only for non-state attributes if expensive, but here full render is cheap enough for now
        // or just update parts. Ideally simply update title.
        const titleEl = this.shadowRoot.querySelector('.title');
        if (name === 'title' && titleEl) {
            titleEl.textContent = newValue;
        }
    }

    get isOpen() {
        return this.hasAttribute('open');
    }

    open() {
        this.setAttribute('open', '');
    }

    close() {
        this.removeAttribute('open');
    }

    render() {
        const title = this.getAttribute('title') || '';

        this.shadowRoot.innerHTML = `
            <div class="overlay">
                <div class="modal">
                    ${title ? `
                    <div class="header">
                        <h3 class="title">${title}</h3>
                        <button class="close-btn" onclick="this.getRootNode().host.close()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    ` : ''}
                    
                    <div class="body">
                        <slot></slot>
                    </div>
                    
                    <div class="footer">
                        <slot name="footer"></slot>
                    </div>
                </div>
            </div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-modal', FudieModal);
