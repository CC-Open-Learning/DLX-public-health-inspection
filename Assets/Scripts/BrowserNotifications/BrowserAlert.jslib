mergeInto(LibraryManager.library, {
    Alert: function (str) {
        window.alert(UTF8ToString(str));
    }
});