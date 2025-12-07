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

    .upload-zone {
        border: 2px dashed #D1D5DB;
        border-radius: 1rem;
        padding: 2rem;
        text-align: center;
        background-color: #F9FAFB;
        transition: all 0.2s;
        cursor: pointer;
        position: relative;
    }

    .upload-zone:hover, .upload-zone.dragover {
        border-color: var(--primary, #FF4F5A);
        background-color: #FFF1F2;
    }

    input[type="file"] {
        position: absolute;
        inset: 0;
        opacity: 0;
        cursor: pointer;
    }

    .icon {
        font-size: 2rem;
        color: #9CA3AF;
        margin-bottom: 1rem;
    }
    
    .upload-zone:hover .icon {
        color: var(--primary, #FF4F5A);
    }

    .text {
        font-weight: 500;
        color: #374151;
        margin-bottom: 0.25rem;
    }

    .subtext {
        font-size: 0.875rem;
        color: #9CA3AF;
    }
    
    .preview-list {
        margin-top: 1rem;
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }
    
    .file-item {
        display: flex;
        align-items: center;
        background: white;
        padding: 0.75rem;
        border-radius: 0.75rem;
        border: 1px solid #E5E7EB;
        animation: fadeIn 0.2s ease-out;
    }
    
    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(5px); }
        to { opacity: 1; transform: translateY(0); }
    }
    
    .file-preview {
        width: 3rem;
        height: 3rem;
        border-radius: 0.5rem;
        overflow: hidden;
        margin-right: 0.75rem;
        background: #F3F4F6;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }
    
    .file-preview img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .file-preview i {
        font-size: 1.5rem;
        color: #6B7280;
    }
    
    .file-info {
        flex: 1;
        text-align: left;
        min-width: 0; /* Enable truncation */
    }
    
    .file-name {
        font-size: 0.875rem;
        font-weight: 500;
        color: #374151;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
    
    .file-size {
        font-size: 0.75rem;
        color: #9CA3AF;
    }
    
    .remove-btn {
        background: none;
        border: none;
        color: #9CA3AF;
        cursor: pointer;
        padding: 0.5rem;
        border-radius: 0.375rem;
        transition: all 0.2s;
    }
    
    .remove-btn:hover {
        background-color: #FEF2F2;
        color: #EF4444;
    }
`);

class FudieFileUpload extends HTMLElement {
    static get observedAttributes() { return ['multiple', 'accept', 'label']; }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.adoptedStyleSheets = [sheet];
        this._files = [];
    }

    connectedCallback() {
        this.render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue !== newValue) {
            // Update input attributes if they change
            const input = this.shadowRoot.querySelector('input');
            const placeholder = this.shadowRoot.querySelector('.text');
            const subtext = this.shadowRoot.querySelector('.subtext');

            if (input && name === 'multiple') {
                if (newValue !== null) {
                    input.setAttribute('multiple', '');
                    if (subtext) subtext.textContent = 'Arrastra archivos aquí o haz click';
                } else {
                    input.removeAttribute('multiple');
                    if (subtext) subtext.textContent = 'Arrastra un archivo aquí o haz click';
                }
            }
            if (input && name === 'accept') {
                input.setAttribute('accept', newValue);
            }
            if (placeholder && name === 'label') {
                placeholder.textContent = newValue;
            }
        }
    }

    render() {
        const label = this.getAttribute('label') || 'Sube tus archivos';
        const subtext = this.hasAttribute('multiple') ? 'Arrastra archivos aquí o haz click' : 'Arrastra un archivo aquí o haz click';

        this.shadowRoot.innerHTML = `
            <div class="upload-zone">
                <input type="file" 
                    ${this.hasAttribute('multiple') ? 'multiple' : ''} 
                    accept="${this.getAttribute('accept') || '*'}">
                <i class="fas fa-cloud-upload-alt icon"></i>
                <div class="text">${label}</div>
                <div class="subtext">${subtext}</div>
            </div>
            <div class="preview-list"></div>
            <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
        `;

        const zone = this.shadowRoot.querySelector('.upload-zone');
        const input = this.shadowRoot.querySelector('input');

        input.addEventListener('change', (e) => {
            if (e.target.files.length > 0) {
                this.handleFiles(e.target.files);
            }
            // Reset input so same file can be selected again if deleted
            e.target.value = '';
        });

        zone.addEventListener('dragover', (e) => {
            e.preventDefault();
            zone.classList.add('dragover');
        });

        zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));

        zone.addEventListener('drop', (e) => {
            e.preventDefault();
            zone.classList.remove('dragover');
            if (e.dataTransfer.files.length > 0) {
                this.handleFiles(e.dataTransfer.files);
            }
        });
    }

    handleFiles(fileList) {
        const newFiles = Array.from(fileList);

        if (!this.hasAttribute('multiple')) {
            // Replace behavior for single file
            this._files = [newFiles[0]];
        } else {
            // Append behavior for multiple
            this._files = [...this._files, ...newFiles];
        }

        this.renderPreview();
        this.dispatchEvent(new CustomEvent('change', { detail: { files: this._files } }));
    }

    renderPreview() {
        const container = this.shadowRoot.querySelector('.preview-list');

        container.innerHTML = this._files.map((file, index) => {
            const isImage = file.type.startsWith('image/');
            let previewContent = '';

            if (isImage) {
                // Determine source for image. If it's a File object, use createObjectURL
                const src = URL.createObjectURL(file);
                previewContent = `<img src="${src}" alt="Preview">`;
            } else {
                // Determine icon based on type map or generic
                let icon = 'file';
                if (file.type.includes('pdf')) icon = 'file-pdf';
                else if (file.type.includes('word') || file.name.endsWith('.doc') || file.name.endsWith('.docx')) icon = 'file-word';
                else if (file.type.includes('spreadsheet') || file.name.endsWith('.xls')) icon = 'file-excel';

                previewContent = `<i class="fas fa-${icon}"></i>`;
            }

            return `
                <div class="file-item">
                    <div class="file-preview">
                        ${previewContent}
                    </div>
                    <div class="file-info">
                        <div class="file-name" title="${file.name}">${file.name}</div>
                        <div class="file-size">${this.formatSize(file.size)}</div>
                    </div>
                    <button class="remove-btn" data-index="${index}" type="button">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                </div>
            `;
        }).join('');

        container.querySelectorAll('.remove-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent bubbling if necessary
                const idx = parseInt(e.currentTarget.dataset.index);
                this._files.splice(idx, 1);
                this.renderPreview();
                this.dispatchEvent(new CustomEvent('change', { detail: { files: this._files } }));
            });
        });
    }

    formatSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }
}

customElements.define('fudie-file-upload', FudieFileUpload);
