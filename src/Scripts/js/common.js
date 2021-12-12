$(function () {
    $(window).on("load", function () {
        history.pushState(null, null, null);
    });
    $(window).on("popstate", function () {
        history.pushState(null, null, null);
    });

    adjust();
    $(document).on('click', '.datarow', function () {
        var no = $('.datarow').index(this);
        if ((no < $('.datarow').length) && (no > 0)) {
            $('.datarow').css("background-color", "#ffffff");
            $('.datarow').attr("flag", "0");
            var flag = $('.datarow').eq(no).attr("flag");
            if (flag == "0") {
                $('.datarow').eq(no).css("background-color", "#6ac5f1");
                $('.datarow').eq(no).attr("flag", "1");
            }
        }
    });
    $("td.no").each(function (i) {
        i = i + 1;
        $(this).text(i);
    });

    const ua = navigator.userAgent;
    if (ua.indexOf('Trident') !== -1) {
        $('#ie-msg').css('display', 'block');
    }
})

$(window).on("popstate", function () {
    history.pushState(null, null, null);
});
   
function tr_default(tblID){
    var vTR = tblID + " tr";
    $(vTR).css("background-color","#ffffff");
}

function tr_click(trID){
    trID.css("background-color","#d1d2cf !important");
}
function adjust(){
    var h = $('.wrapper').height(); //ウィンドウの高さ
    var header_h = 50+15;
    $('#sidebar').css('height', h + header_h); //可変部分の高さを適用
}
function sortUp() {
    var no = 0;
    var i = 0;
    $('.datarow').each(function () {
        if ($(this).attr('flag') == 1) {
            no = i;
            return false;
        }
        i++;
    });
    if (no > 1) {
        let $row = $('.datarow').eq(no).closest("tr");
        let $row_prev = $row.prev("tr");
        if ($row.prev.length) {
            $row.insertBefore($row_prev);
        }
        $("td.no").each(function (i) {
            i = i + 1;
            $(this).text(i);
        });
    }
}
function sortDown() {
    var no = 0;
    var i = 0;
    $('.datarow').each(function () {
        if ($(this).attr('flag') == 1) {
            no = i;
            return false;
        }
        i++;
    });
    let $row = $('.datarow').eq(no).closest("tr");
    let $row_next = $row.next("tr");
    if ($row_next.length) {
        $row.insertAfter($row_next);
    }
    $("td.no").each(function (i) {
        i = i + 1;
        $(this).text(i);
    });

}
function ckDate(strDate) {
    if (!strDate.match(/^\d{4}\-\d{2}\-\d{2}$/)) {
        return false;
    }
    var y = strDate.split("-")[0];
    var m = strDate.split("-")[1] - 1;
    var d = strDate.split("-")[2];
    var date = new Date(y, m, d);
    if (date.getFullYear() != y || date.getMonth() != m || date.getDate() != d) {
        return false;
    }
    return true;
}
function isDate(str) {

    // フォーマット（YYYY/MM/DD）チェック
    var flg = false;
    var datestr = '';
    if (str.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
        datestr = str.replace(/\//g, '');
        flg = true;
    }
    if (str.match(/^\d{4}-\d{2}-\d{2}$/)) {
        datestr = str.replace(/-/g, '');
        flg = true;
    }
    if (str.match(/^\d{4}\d{2}\d{2}$/)) {
        datestr = str;
        flg = true;
    }
    if (!flg) {
        return false;
    }

    var yyyy = datestr.substr(0, 4);
    var mm = datestr.substr(4, 2) - 1; //1月は0から始まる為 -1 する。
    var dd = datestr.substr(6, 2);

    // 月,日の妥当性チェック
    if (mm >= 0 && mm <= 11 && dd >= 1 && dd <= 31) {

        var vDt = new Date(yyyy, mm, dd);

        if (isNaN(vDt)) {
            return false;
        } else if (vDt.getFullYear() == yyyy && vDt.getMonth() == mm && vDt.getDate() == dd) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}