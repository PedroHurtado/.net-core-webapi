const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --bg-color: #FFFFFF;
        --border-color: #E5E7EB; /* gray-200 */
        --shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
        --radius: 1rem; /* rounded-2xl */
        --padding: 1.5rem; /* p-6 */
    }

    .card {
        background-color: var(--bg-color);
        border: 1px solid var(--border-color);
        border-radius: var(--radius);
        box-shadow: var(--shadow);
        overflow: hidden;
    }

    .header {
        padding: var(--padding);
        padding-bottom: 1rem;
        border-bottom: 1px solid var(--border-color);
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .title {
        font-size: 1.125rem; /* text-lg */
        font-weight: 700;
        color: #111827; /* gray-900 */
        margin: 0;
    }

    .subtitle {
        font-size: 0.875rem; /* text-sm */
        color: #6B7280; /* gray-500 */
        margin-top: 0.25rem;
    }

    .content {
        padding: var(--padding);
    }

    .footer {
        padding: 1rem var(--padding);
        background-color: #F9FAFB; /* gray-50 */
        border-top: 1px solid var(--border-color);
        display: flex;
        justify-content: flex-end;
        gap: 0.75rem;
    }

    /* Modifiers */
    :host([no-padding]) .content {
        padding: 0;
    }

    :host([no-border]) .card {
        border: none;
        box-shadow: none;
    }
`);

class FudieCard extends HTMLElement {
    static get observedAttributes() {
        return ['title', 'subtitle', 'no-padding', 'no-border'];
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
        const title = this.getAttribute('title');
        const subtitle = this.getAttribute('subtitle');

        // Check internal structure
        if (this.shadowRoot.querySelector('.card')) {
            const header = this.shadowRoot.querySelector('.header');
            const titleEl = this.shadowRoot.querySelector('.title-text');
            const subTitleEl = this.shadowRoot.querySelector('.subtitle');

            if (title) {
                if (!header) {
                    // Need full re-render if header appears
                    this.shadowRoot.innerHTML = this.getTemplate(title, subtitle);
                    return;
                }
                titleEl.textContent = title;
                if (subtitle) {
                    subTitleEl.textContent = subtitle;
                    subTitleEl.style.display = 'block';
                } else {
                    subTitleEl.style.display = 'none';
                }
            } else if (header) {
                // Remove header if no title
                header.remove();
            }
            return;
        }

        this.shadowRoot.innerHTML = this.getTemplate(title, subtitle);
    }

    getTemplate(title, subtitle) {
        return `
            <div class="card">
                ${title ? `
                <div class="header">
                    <div>
                        <h3 class="title title-text">${title}</h3>
                        <p class="subtitle" ${subtitle ? '' : 'style="display:none"'}>${subtitle || ''}</p>
                    </div>
                    <div class="actions">
                        <slot name="header-actions"></slot>
                    </div>
                </div>
                ` : ''}
                
                <div class="content">
                    <slot></slot>
                </div>

                <div class="footer">
                    <slot name="footer"></slot>
                </div>
                <!-- Hide footer if empty? JS logic or CSS empty selector could handle, 
                     but slot change detection is heavier. For now, we assume user uses slot if they want footer layout. -->
            </div>
            <style>
                /* Helper to hide footer if no content projected - tricky in pure CSS without :has() support in all */
                /* For now, we leave the footer div. The user should only put content in if they want the bar. */
                .footer:empty {
                    display: none;
                }
            </style>
        `;
    }
}

customElements.define('fudie-card', FudieCard);
