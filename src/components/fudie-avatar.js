const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: inline-block;
        vertical-align: middle;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .avatar-container {
        position: relative;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        overflow: hidden;
        background-color: #E5E7EB;
        color: #6B7280;
        font-weight: 600;
        text-transform: uppercase;
        border: 2px solid white;
        box-shadow: 0 0 0 1px #F3F4F6;
    }

    img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    /* Sizes */
    .size-sm { width: 2rem; height: 2rem; font-size: 0.75rem; }
    .size-md { width: 2.5rem; height: 2.5rem; font-size: 1rem; }
    .size-lg { width: 3.5rem; height: 3.5rem; font-size: 1.25rem; }
    .size-xl { width: 5rem; height: 5rem; font-size: 1.75rem; }

    /* Shapes */
    .shape-circle { border-radius: 50%; }
    .shape-square { border-radius: 0.5rem; }

    /* Status Indicator */
    .status {
        position: absolute;
        bottom: 0;
        right: 0;
        border-radius: 50%;
        border: 2px solid white;
    }
    
    .status-online { background-color: #10B981; }
    .status-busy { background-color: #EF4444; }
    .status-away { background-color: #F59E0B; }
    .status-offline { background-color: #9CA3AF; }

    /* Status Sizes */
    .size-sm .status { width: 0.5rem; height: 0.5rem; }
    .size-md .status { width: 0.625rem; height: 0.625rem; }
    .size-lg .status { width: 0.875rem; height: 0.875rem; }
    .size-xl .status { width: 1.125rem; height: 1.125rem; }
    
    .icon-fallback {
        font-size: 1.2em;
    }
`);

class FudieAvatar extends HTMLElement {
    static get observedAttributes() {
        return ['src', 'alt', 'initials', 'size', 'shape', 'status'];
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
        const src = this.getAttribute('src');
        const alt = this.getAttribute('alt') || 'Avatar';
        const initials = this.getAttribute('initials');
        const size = this.getAttribute('size') || 'md';
        const shape = this.getAttribute('shape') || 'circle';
        const status = this.getAttribute('status'); // online, busy, away, offline

        const showImage = src;
        const showInitials = !src && initials;
        const showIcon = !src && !initials;

        this.shadowRoot.innerHTML = `
            <div class="avatar-container size-${size} shape-${shape}">
                ${showImage ? `<img src="${src}" alt="${alt}">` : ''}
                ${showInitials ? `<span>${initials.substring(0, 2)}</span>` : ''}
                ${showIcon ? `<i class="fas fa-user icon-fallback"></i>` : ''}
                ${status ? `<span class="status status-${status}"></span>` : ''}
            </div>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-avatar', FudieAvatar);
