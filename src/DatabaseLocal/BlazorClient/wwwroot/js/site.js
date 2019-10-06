// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


window.blazorJsFunctions = {
    getImgString: function (id) {
        var file = document.getElementById(id).files[0];
        var img = document.getElementById("imghide" + id);
        var imgReader = new FileReader();
        imgReader.onloadend = function () {
            img.value = imgReader.result;
            //console.log('Base64 Format', imgReader.result);
        }
        this.e = imgReader.onloadend;
        imgReader.readAsDataURL(file);
    },
    addB: function (id) {
        var textarea = document.getElementById(id);
        textarea.value += "b|b|";
        console.log(textarea.value)
    },
    log: function (s) {
        console.log(s)
    },
    init: function () {
        $('body').scrollspy({ target: '#list-example' });
    },
    getHideString: function (id) {
        var img = document.getElementById("imghide" + id);
        this.log(img.value);
        return img.value;
    },
    warn: function () {
        alert("请先登录！")
    },
    device: function () {
        var windowWidth = $(window).width();
        if (windowWidth<500) {
            document.getElementById('alicenav').hidden = true;
            document.getElementById('preview').classList.remove('col-8');
            document.getElementById('preview').classList.add('col-10');
            console.log(document.getElementById('alicenav').accessKey);
        }


    }
};