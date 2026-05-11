// TinyMCE initialization and interop functions

window.initTinyMCE = function(height) {
    if (typeof tinymce === 'undefined') {
        console.error('TinyMCE is not loaded');
        return;
    }

    tinymce.init({
        selector: 'textarea',
        height: height || 300,
        menubar: true,
        plugins: [
            'anchor',
            'autolink',
            'charmap',
            'codesample',
            'emoticons',
            'image',
            'insertdatetime',
            'link',
            'lists',
            'media',
            'nonbreaking',
            'preview',
            'searchreplace',
            'table',
            'visualblocks',
            'wordcount'
        ],
        toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image table | align lineheight | bullist numlist indent outdent | emoticons charmap | removeformat | preview code',
        content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px; line-height: 1.6; }',
        license_key: 'gpl',
        setup: function (editor) {
            // Automatically update on change
            editor.on('change', function () {
                // Trigger blur event to notify Blazor binding
                const textarea = editor.getElement();
                textarea.value = editor.getContent();
                textarea.dispatchEvent(new Event('change', { bubbles: true }));
            });
        }
    });
};

window.getTinyMCEContent = function () {
    if (tinymce && tinymce.activeEditor) {
        return tinymce.activeEditor.getContent();
    }
    return '';
};

window.setTinyMCEContent = function (content) {
    if (tinymce && tinymce.activeEditor) {
        tinymce.activeEditor.setContent(content);
    }
};

window.getTinyMCEEditorBySelector = function (selector) {
    if (tinymce) {
        const editor = tinymce.get(selector);
        if (editor) {
            return editor.getContent();
        }
    }
    return '';
};

window.setTinyMCEEditorBySelector = function (selector, content) {
    if (tinymce) {
        const editor = tinymce.get(selector);
        if (editor) {
            editor.setContent(content);
        }
    }
};

// Clean up TinyMCE instances when needed
window.removeTinyMCEEditor = function () {
    if (tinymce && tinymce.editors && tinymce.editors.length > 0) {
        tinymce.remove();
    }
};
