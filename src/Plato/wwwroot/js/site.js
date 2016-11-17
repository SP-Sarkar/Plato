﻿
if (typeof jQuery === "undefined") {
    throw new Error("Plato requires jQuery");
}

/* Plato
 *
 * @type Object
 * @description $.AdminLTE is the main object for the template's app.
 *              It's used for implementing functions and options related
 *              to the template. Keeping everything wrapped in an object
 *              prevents conflict with other plugins and is a better
 *              way to organize our code.
 */
$.Plato = {};


/* ------------------
 * - Options -
 * ------------------
 * The next block of code defines various global
 * application settings. This can be extended via the 
 * PlatoOptions object on the window object
 */
$.Plato.options = {
    // bootstrap tooltips
    BSToolTipEnabled: true,
    BSToolTipSelector: "[data-toggle='tooltip']",
    
    MagnificSelector: "[data-toggle='dialog']",

};


/* ------------------
 * - Implementation -
 * ------------------
 * The next block of code implements AdminLTE's
 * functions and plugins as specified by the
 * options above.
 */
$(function() {

    "use strict";

    // fix for IE page transitions
    $("body").removeClass("hold-transition");
    
    //Extend options if external options exist
    if (typeof window.PlatoOptions !== "undefined") {
        $.extend(true, $.Plato.options, window.PlatoOptions);
    }

    // access to options
    var o = $.Plato.options;
    
    // initialize bootstrap tooltips
    if (o.BSToolTipEnabled) {
        var tips = $("body").tooltip({ selector: o.BSToolTipSelector });
        tips.on('show.bs.tooltip', function () {
            // only one tooltip should ever be open at a time
            $('.tooltip').not(this).tooltip('hide');
        });
    }


    // initialize magnific popup 
    $(o.MagnificSelector).magnificPopup({
          type: 'inline',
          fixedContentPos: false,
          fixedBgPos: true,
          overflowY: 'auto',
          closeBtnInside: true,
          preloader: false,
          midClick: true,
          removalDelay: 300,
          mainClass: 'plato-mfp-zoom-in'
     });


});
