/* Die Validierung mittels jQuery Validation akzeptiert die Verwendung des Komma als Dezimal-Separator nicht, weswegen die 
   betreffenden JS-Funktionen mit diesem Script überschrieben werden. Es muss in jedem Fall nach jQuery.Validate.js eingebunden 
   werden.
   Geändert wird die Validierung für Inputs des Type Number und des Type Range.
*/
$.validator.methods.range = function (value, element, param) {
    var globalizedValue = value.replace(",", ".");
    return this.optional(element) || (globalizedValue >= param[0] && globalizedValue <= param[1]);
}

$.validator.methods.number = function (value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
}