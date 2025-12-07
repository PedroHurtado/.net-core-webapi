const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --primary-light: #FFF1F2;
        --gray-500: #6B7280;
        --gray-700: #374151;
        --gray-900: #111827;
        --bg-hover: #F9FAFB;
    }

    a {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem 1rem;
        border-radius: 0.75rem;
        text-decoration: none;
        color: var(--gray-700);
        font-weight: 500;
        transition: all 0.2s;
        cursor: pointer;
    }

    a:hover {
        background-color: var(--bg-hover);
        color: var(--gray-900);
    }

    /* Active State */
    :host([active]) a {
        background-color: var(--primary-light);
        color: var(--primary);
    }

    .icon {
        width: 1.25rem;
        height: 1.25rem;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.125rem;
        transition: color 0.2s;
        color: var(--gray-500);
    }

    :host([active]) .icon {
        color: var(--primary);
    }
    
    a:hover .icon {
        color: var(--gray-700);
    }
    
    :host([active]) a:hover .icon {
        color: var(--primary);
    }

    .label {
        flex: 1;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    /* Slot for badge/count */
    .end-slot {
        font-size: 0.75rem;
        font-weight: 600;
    }
`);

class FudieNavItem extends HTMLElement {
    static get observedAttributes() {
        return ['href', 'icon', 'label', 'active'];
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
        const href = this.getAttribute('href') || '#';
        const icon = this.getAttribute('icon') || '';
        const label = this.getAttribute('label') || '';
        const active = this.hasAttribute('active');

        // FontAwesome icon handling
        // If icon is just a name like "home", assume "fas fa-home". 
        // If it starts with "fa-", assume "fas " + icon
        let iconClass = icon;
        if (icon && !icon.includes(' ')) {
            iconClass = `fas fa-${icon}`;
        }

        // Check internal structure
        if (this.shadowRoot.querySelector('a')) {
            const anchor = this.shadowRoot.querySelector('a');
            const iconEl = this.shadowRoot.querySelector('i');
            const labelEl = this.shadowRoot.querySelector('.label');

            if (anchor.getAttribute('href') !== href) anchor.setAttribute('href', href);
            if (labelEl) labelEl.textContent = label;
            if (iconEl) iconEl.className = `${iconClass} icon`;

            return;
        }

        this.shadowRoot.innerHTML = `
            <a href="${href}">
                <i class="${iconClass} icon"></i>
                <span class="label">${label}</span>
                <span class="end-slot"><slot></slot></span>
            </a>
            <!-- Load FontAwesome only if needed for shadow DOM isolation, though usually global styles are better. 
                 But here we assume FontAwesome is loaded in the main doc page or we use a stylesheet link. 
                 Since we can't easily link external CSS in shadow without knowing the path, we rely on 
                 the assumption that the icon font is globally available or we use SVGs.
                 Component libraries often use SVGs. For this legacy extraction, we rely on <i> tags behaving 
                 with external font definitions if we don't encapsulate strict. 
                 Since we use ShadowDOM, external CSS for fonts won't apply to <i> inside unless we import it. 
                 To fix this for FontAwesome in ShadowDOM, we need the link.
            -->
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-nav-item', FudieNavItem);
