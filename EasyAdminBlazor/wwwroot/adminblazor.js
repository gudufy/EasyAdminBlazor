var EasyAdminBlazorJS = {
    sidebarToggle: function () {
        var bd = $(document.body)
        if (!bd.hasClass('sidebar-open') && !bd.hasClass('sidebar-collapse') || bd.hasClass('sidebar-open'))
            bd.addClass('sidebar-collapse').removeClass('sidebar-open')
        else
            bd.removeClass('sidebar-collapse').addClass('sidebar-open')
    },
    modalShow: function (id, dotnetRef) {
        var ele = $('#' + id);
        ele.modal('show');
        ele.on('hidden.bs.modal', e => {
            if (id != e.target.id) return
            dotnetRef.invokeMethodAsync('ModalOnClose')
        })
    },
    setCookie: function (name, value, expireDays) {
        var cookie = decodeURIComponent(name) + "=" + decodeURIComponent(value);
        if (expireDays > 0) {
            var d = new Date();
            d.setTime(d.getTime() + (expireDays * 24 * 60 * 60 * 1000));
            cookie += ";expires=" + d.toUTCString();
        }
        cookie += ";path=/";
        document.cookie = cookie;
    }
};

window.callback = null;
window.filecallback = function (linkUrl) {
    if (window.callback) window.callback(linkUrl);
    document.getElementById("fileDialog").style.display = "none";
};
window.closefilepicker = function () {
    document.getElementById("fileDialog").style.display = "none";
};
window.openfilepicker = function (cb, value, meta) {
    window.callback = cb;
    document.getElementById("fileDialog").style.display = "block";
};
window.setpickervalue = function (e, value) {
    var pre = e.previousElementSibling;
    pre.value = value;
    var event = new Event('change');
    pre.dispatchEvent(event);
}
window.admin = {
    license_key: 'gpl',
    height: 400,
    skin_url: '/lib/tinymce/skins/ui/oxide',
    language_url: '/lib/tinymce/langs/zh-Hans.js',
    language: 'zh-Hans',
    file_picker_types: 'file image media',
    file_picker_callback: window.openfilepicker,
    verify_html: false,
    relative_urls: false,
    remove_script_host: true,
    //document_base_url: '',
    menubar: false,
    toolbar_mode: 'wrap',
    pagebreak_split_block: true,
    plugins: 'lists link image media table code wordcount pagebreak preview fullscreen',
    toolbar:
        'undo redo codesample bold italic underline strikethrough link alignleft aligncenter alignright alignjustify \
              bullist numlist outdent indent removeformat forecolor backcolor |formatselect fontselect fontsizeselect | \
              blocks fontfamily fontsize pagebreak lists image media table preview | code fullscreen',
    content_css: '/lib/bootstrap/css/bootstrap.min.css,/lib/fontawesome/css/all.min.css,/css/trevlo.css'
}
