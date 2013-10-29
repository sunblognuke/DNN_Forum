/* init vmd when document loaded */
jQuery(document).ready(function() {
    jQuery('textarea.Forum_QuickReplyBox').wmd({
        "preview": true,
        "helpLink": "http://daringfireball.net/projects/markdown/",
        "helpHoverTitle": "Markdown Help"
    });
});