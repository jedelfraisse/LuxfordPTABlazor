window.scrollToEditForm = function() {
    var form = document.querySelector('.card.mb-4');
    if (form) {
        form.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

window.scrollToTop = function() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};
