window.easyMdeEditors = window.easyMdeEditors || {};

window.initEasyMDE = function (element, editorId, initialValue, placeholder, minHeight, dotNetRef) {
    if (!window.EasyMDE || !element || !editorId) {
        return;
    }

    const existing = window.easyMdeEditors[editorId];
    if (existing && existing.editor) {
        return;
    }

    const editor = new EasyMDE({
        element: element,
        initialValue: initialValue || "",
        placeholder: placeholder || "",
        minHeight: `${minHeight || 180}px`,
        spellChecker: false,
        forceSync: true,
        status: ["lines", "words"],
        toolbar: [
            "bold",
            "italic",
            "heading",
            "|",
            "quote",
            "unordered-list",
            "ordered-list",
            "|",
            "link",
            "image",
            "|",
            "preview",
            "side-by-side",
            "fullscreen",
            "|",
            "guide"
        ]
    });

    editor.codemirror.on("change", function () {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnEasyMdeChanged", editor.value());
        }
    });

    window.easyMdeEditors[editorId] = {
        editor: editor
    };
};

window.setEasyMDEContent = function (editorId, content) {
    const instance = window.easyMdeEditors[editorId];
    if (!instance || !instance.editor) {
        return;
    }

    const newContent = content || "";
    if (instance.editor.value() !== newContent) {
        instance.editor.value(newContent);
    }
};

window.disposeEasyMDE = function (editorId) {
    const instance = window.easyMdeEditors[editorId];
    if (!instance || !instance.editor) {
        return;
    }

    instance.editor.toTextArea();
    delete window.easyMdeEditors[editorId];
};
