const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .tab-list {
        display: flex;
        border-bottom: 2px solid #F3F4F6;
        margin-bottom: 2rem;
        gap: 2rem;
        overflow-x: auto;
        scrollbar-width: none; /* Hide scrollbar Firefox */
    }
    
    .tab-list::-webkit-scrollbar {
        display: none; /* Hide scrollbar Chrome/Safari */
    }

    .tab-btn {
        background: none;
        border: none;
        padding: 0.75rem 0;
        cursor: pointer;
        font-family: inherit;
        font-size: 0.875rem;
        font-weight: 600;
        color: #6B7280;
        position: relative;
        white-space: nowrap;
        transition: color 0.2s;
    }

    .tab-btn:hover {
        color: #111827;
    }

    .tab-btn.active {
        color: var(--primary, #FF4F5A);
    }

    .tab-btn.active::after {
        content: '';
        position: absolute;
        bottom: -2px;
        left: 0;
        width: 100%;
        height: 2px;
        background-color: var(--primary, #FF4F5A);
        border-radius: 2px 2px 0 0;
    }

    /* Focus ring for accessibility */
    .tab-btn:focus-visible {
        outline: 2px solid var(--primary, #FF4F5A);
        outline-offset: 4px;
        border-radius: 2px;
    }
`);

class FudieTabs extends HTMLElement {
    static get observedAttributes() {
        return ['active'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._tabs = [];
        this._panels = [];
    }

    connectedCallback() {
        this.render();
        // Wait for children to be parsed
        setTimeout(() => this.linkTabs(), 0);
    }

    linkTabs() {
        const slot = this.shadowRoot.querySelector('slot');
        const children = slot.assignedElements();

        // Assume structure: <fudie-tab-item label="Tab 1">Content</fudie-tab-item>
        // Or generic divs with data-label?
        // Let's use specific child elements pattern or data attributes on children.
        // Simplest generic way: children are panels, data-tab="Label" defines the tab.

        this._panels = children;
        this._tabs = children.map((panel, index) => {
            return {
                label: panel.getAttribute('data-tab') || `Tab ${index + 1}`,
                id: `tab-${index}`,
                panelId: `panel-${index}`
            };
        });

        this.renderTabs();

        // Set initial active tab
        const initialActive = this.getAttribute('active') || '0';
        this.activateTab(parseInt(initialActive) || 0);
    }

    render() {
        this.shadowRoot.innerHTML = `
            <div class="tab-list" role="tablist"></div>
            <slot></slot>
        `;
    }

    renderTabs() {
        const tabList = this.shadowRoot.querySelector('.tab-list');
        tabList.innerHTML = this._tabs.map((tab, index) => `
            <button 
                class="tab-btn" 
                role="tab" 
                aria-selected="false"
                aria-controls="${tab.panelId}"
                id="${tab.id}"
                data-index="${index}"
            >
                ${tab.label}
            </button>
        `).join('');

        tabList.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                this.activateTab(parseInt(btn.dataset.index));
            });
        });
    }

    activateTab(index) {
        const tabs = this.shadowRoot.querySelectorAll('.tab-btn');
        const panels = this._panels;

        tabs.forEach((tab, i) => {
            const isActive = i === index;
            tab.classList.toggle('active', isActive);
            tab.setAttribute('aria-selected', isActive);
            tab.setAttribute('tabindex', isActive ? '0' : '-1');
        });

        panels.forEach((panel, i) => {
            if (i === index) {
                panel.style.display = 'block';
            } else {
                panel.style.display = 'none';
            }
        });

        this.setAttribute('active', index);
    }
}

customElements.define('fudie-tabs', FudieTabs);
