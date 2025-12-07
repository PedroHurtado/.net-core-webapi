const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .card {
        background: white;
        border-radius: 1.5rem;
        padding: 1.5rem;
        border: 1px solid #F3F4F6;
        display: flex;
        align-items: flex-start;
        gap: 1rem;
    }

    .icon-box {
        width: 3rem;
        height: 3rem;
        border-radius: 1rem;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.25rem;
        background-color: var(--bg-color, #F3F4F6);
        color: var(--text-color, #6B7280);
    }

    .content {
        flex: 1;
    }

    .label {
        font-size: 0.875rem;
        color: #6B7280;
        margin-bottom: 0.25rem;
        font-weight: 500;
    }

    .value {
        font-size: 1.5rem;
        font-weight: 800;
        color: #111827;
        line-height: 1.2;
        margin-bottom: 0.25rem;
    }

    .trend {
        font-size: 0.75rem;
        font-weight: 600;
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
    }

    .trend.up { color: #059669; }
    .trend.down { color: #DC2626; }
    .trend.neutral { color: #6B7280; }
`);

class FudieStatCard extends HTMLElement {
    static get observedAttributes() {
        return ['label', 'value', 'trend', 'trend-label', 'icon', 'variant'];
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
        const label = this.getAttribute('label') || '';
        const value = this.getAttribute('value') || '0';
        const trend = parseFloat(this.getAttribute('trend') || '0'); // numeric percentage
        const trendLabel = this.getAttribute('trend-label') || 'vs. mes pasado';
        const icon = this.getAttribute('icon') || 'chart-line';
        const variant = this.getAttribute('variant') || 'primary'; // Determines icon color

        // Determine colors for icon
        let bg = '#FEF2F2';
        let text = '#F87171';

        if (variant === 'primary') { bg = '#FEF2F2'; text = '#FF4F5A'; }
        else if (variant === 'success') { bg = '#DEF7EC'; text = '#059669'; }
        else if (variant === 'info') { bg = '#E1EFFE'; text = '#3F83F8'; }
        else if (variant === 'warning') { bg = '#FDF6B2'; text = '#D03801'; }

        // Determine trend direction
        let trendClass = 'neutral';
        let trendIcon = 'minus';
        let trendText = trend + '%';
        if (trend > 0) { trendClass = 'up'; trendIcon = 'arrow-up'; trendText = '+' + trend + '%'; }
        if (trend < 0) { trendClass = 'down'; trendIcon = 'arrow-down'; }

        this.shadowRoot.innerHTML = `
            <div class="card" style="--bg-color: ${bg}; --text-color: ${text}">
                <div class="icon-box">
                    <i class="fas fa-${icon}"></i>
                </div>
                <div class="content">
                    <div class="label">${label}</div>
                    <div class="value">${value}</div>
                    <div class="trend ${trendClass}">
                        <i class="fas fa-${trendIcon}"></i>
                        <span>${trendText} ${trendLabel}</span>
                    </div>
                </div>
            </div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;
    }
}

customElements.define('fudie-stat-card', FudieStatCard);
