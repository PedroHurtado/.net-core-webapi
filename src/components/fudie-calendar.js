const sheet = new CSSStyleSheet();
sheet.replaceSync(`
    :host {
        display: block;
        font-family: 'Plus Jakarta Sans', sans-serif;
        --primary: #FF4F5A;
        --primary-light: #FFF1F2;
        --text-main: #111827;
        --text-muted: #6B7280;
        --border: #E5E7EB;
        --bg: #FFFFFF;
        --radius: 1rem;
    }

    .calendar-container {
        background: var(--bg);
        border: 1px solid var(--border);
        border-radius: var(--radius);
        padding: 1.5rem;
        box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        max-width: 350px; /* Default constraint */
        user-select: none;
    }

    /* Header */
    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1.5rem;
    }

    .month-title {
        font-size: 1.125rem;
        font-weight: 700;
        color: var(--text-main);
        text-transform: capitalize;
    }

    .nav-btn {
        background: transparent;
        border: 1px solid var(--border);
        border-radius: 0.5rem;
        width: 2rem;
        height: 2rem;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        color: var(--text-muted);
        transition: all 0.2s;
    }

    .nav-btn:hover:not(:disabled) {
        border-color: var(--primary);
        color: var(--primary);
        background: var(--primary-light);
    }

    .nav-btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    /* Grid */
    .weekdays {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        margin-bottom: 0.5rem;
        text-align: center;
    }

    .weekday {
        font-size: 0.75rem;
        font-weight: 600;
        color: var(--text-muted);
        text-transform: uppercase;
    }

    .days {
        display: grid;
        grid-template-columns: repeat(7, 1fr);
        gap: 0.25rem;
    }

    .day {
        aspect-ratio: 1;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.9375rem;
        font-weight: 500;
        color: var(--text-main);
        border-radius: 0.75rem;
        cursor: pointer;
        transition: all 0.2s;
        border: 1px solid transparent;
    }

    /* States */
    .day:hover:not(.empty):not(.disabled):not(.selected) {
        background-color: var(--primary-light);
        color: var(--primary);
    }

    .day.selected {
        background-color: var(--primary);
        color: white;
        font-weight: 700;
    }

    .day.today:not(.selected) {
        border-color: var(--primary);
        color: var(--primary);
    }

    .day.disabled {
        color: var(--border);
        cursor: not-allowed;
    }

    .day.empty {
        cursor: default;
    }
`);

class FudieCalendar extends HTMLElement {
    static get observedAttributes() {
        return ['selected', 'min-date', 'locale']; // selected="YYYY-MM-DD"
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];

        this.state = {
            currentDate: new Date(), // For navigation state
            selectedDate: null,
            minDate: new Date().setHours(0, 0, 0, 0) // Default to today
        };
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue) return;

        if (name === 'selected' && newValue) {
            this.state.selectedDate = new Date(newValue);
            this.state.currentDate = new Date(newValue); // Jump to selected
        }
        if (name === 'min-date') {
            this.state.minDate = newValue ? new Date(newValue).setHours(0, 0, 0, 0) : null;
        }

        this.render();
    }

    get locale() {
        return this.getAttribute('locale') || 'es-ES';
    }

    changeMonth(delta) {
        const newDate = new Date(this.state.currentDate);
        newDate.setMonth(newDate.getMonth() + delta);
        this.state.currentDate = newDate;
        this.render();
    }

    selectDate(date) {
        if (this.isDisabled(date)) return;

        this.setAttribute('selected', date.toISOString().split('T')[0]);
        this.dispatchEvent(new CustomEvent('change', {
            bubbles: true,
            composed: true,
            detail: { value: this.getAttribute('selected') }
        }));
    }

    isDisabled(date) {
        if (!this.state.minDate) return false;
        return date.getTime() < this.state.minDate;
    }

    render() {
        const year = this.state.currentDate.getFullYear();
        const month = this.state.currentDate.getMonth();

        // Date Logic
        const firstDayOfMonth = new Date(year, month, 1);
        const lastDayOfMonth = new Date(year, month + 1, 0);
        const daysInMonth = lastDayOfMonth.getDate();

        // Adjust for Monday start (0=Sun, 1=Mon...). We want Mon=0, Sun=6
        let startDay = firstDayOfMonth.getDay() - 1;
        if (startDay === -1) startDay = 6;

        // Header Text
        const monthName = new Intl.DateTimeFormat(this.locale, { month: 'long', year: 'numeric' }).format(this.state.currentDate);

        // Weekdays Header
        const weekdays = ['L', 'M', 'X', 'J', 'V', 'S', 'D']; // Hardcoded for ES mainly, or derive from Intl
        // Better dynamic approach for weekdays? For now simple array to match 'es'.

        // Initial HTML Skeleton
        this.shadowRoot.innerHTML = `
            <div class="calendar-container">
                <div class="header">
                    <button class="nav-btn prev-btn"><i class="fas fa-chevron-left"></i></button>
                    <span class="month-title">${monthName}</span>
                    <button class="nav-btn next-btn"><i class="fas fa-chevron-right"></i></button>
                </div>
                
                <div class="weekdays">
                    ${weekdays.map(d => `<div class="weekday">${d}</div>`).join('')}
                </div>

                <div class="days" id="days-grid"></div>
            </div>
             <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        const grid = this.shadowRoot.getElementById('days-grid');

        // Empty cells before start
        for (let i = 0; i < startDay; i++) {
            const div = document.createElement('div');
            div.className = 'day empty';
            grid.appendChild(div);
        }

        // Days
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        for (let i = 1; i <= daysInMonth; i++) {
            const date = new Date(year, month, i);
            const dateStr = date.toISOString().split('T')[0];
            const div = document.createElement('div');
            div.textContent = i;
            div.className = 'day';

            // Check states
            if (this.isDisabled(date)) {
                div.classList.add('disabled');
            } else {
                div.onclick = () => this.selectDate(date);
            }

            if (this.state.selectedDate && date.getTime() === this.state.selectedDate.getTime()) {
                div.classList.add('selected');
            }

            if (date.getTime() === today.getTime()) {
                div.classList.add('today');
            }

            grid.appendChild(div);
        }

        // Event Listeners for Nav
        this.shadowRoot.querySelector('.prev-btn').onclick = () => this.changeMonth(-1);
        this.shadowRoot.querySelector('.next-btn').onclick = () => this.changeMonth(1);
    }
}

customElements.define('fudie-calendar', FudieCalendar);
