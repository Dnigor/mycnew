/// <reference path="../Src/knockout.validation.js" />

/************************************************
* This is an example localization page. All of these
* messages are the default messages for ko.validation
* 
* Currently ko.validation only does a single parameter replacement
* on your message (indicated by the {0}).
*
* The parameter that you provide in your validation extender
* is what is passed to your message to do the {0} replacement.
*
* eg: myProperty.extend({ minLength: 5 });
* ... will provide a message of "Please enter at least 5 characters"
* when validated
*
* This message replacement obviously only works with primitives
* such as numbers and strings. We do not stringify complex objects 
* or anything like that currently.
*/

ko.validation.localize({
    required:  'VALIDATION_REQUIRED',  //'This field is required.',
    min:       'VALIDATION_MIN',       //'Please enter a value greater than or equal to {0}.',
    max:       'VALIDATION_MAX',       //'Please enter a value less than or equal to {0}.',
    minLength: 'VALIDATION_MINLENGTH', //'Please enter at least {0} characters.',
    maxLength: 'VALIDATION_MAXLENGTH', //'Please enter no more than {0} characters.',
    pattern:   'VALIDATION_PATTERN',   //'Please check this value.',
    step:      'VALIDATION_STEP',      //'The value must increment by {0}',
    email:     'VALIDATION_EMAIL',     //'This is not a proper email address',
    date:      'VALIDATION_DATE',      //'Please enter a proper date',
    dateISO:   'VALIDATION_DATEISO',   //'Please enter a proper date',
    number:    'VALIDATION_NUMBER',    //'Please enter a number',
    digit:     'VALIDATION_DIGIT',     //'Please enter a digit',
    phoneUS:   'VALIDATION_PHONEUS',   //'Please specify a valid phone number',
    equal:     'VALIDATION_EQUAL',     //'Values must equal',
    notEqual:  'VALIDATION_NOTEQUAL',  //'Please choose another value.',
    unique:    'VALIDATION_UNIQUE',    //'Please make sure the value is unique.'
});