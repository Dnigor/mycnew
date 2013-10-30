describe('EmailValidation', function () {

    var config = {
        validationUrl: '',
        socialNetworksConfig: []
    };

    app.subsidiary = 'xx';

    it('should allow alphanumeric in local part', function () {
        var model = new myc.Customer({ profileQuestionGroups: [] }, config);
        var emailAddress = 'qwertyQWERTY0123456789@mail.com';

        model.emailAddress(emailAddress);
        var isValid = model.validationState().emailAddress.isValid();

        expect(isValid).toBeTruthy();
    });

    it('should allow _, -, !, #, %, &, \', +, /, ., ~, $,| in local part', function () {
        var model = new myc.Customer({ profileQuestionGroups: [] }, config);
        var emailAddress = "_-!#%&'+/.$|@mail.com";

        model.emailAddress(emailAddress);
        var isValid = model.validationState().emailAddress.isValid();

        expect(isValid).toBeTruthy();
    });

    it('should allow only alphanumeric and periods in domain part', function () {
        var model = new myc.Customer({ profileQuestionGroups: [] }, config);
        var emailAddress = 'test@qwertyQWERTY0123456789.com';

        model.emailAddress(emailAddress);
        var isValid = model.validationState().emailAddress.isValid();

        expect(isValid).toBeTruthy();

        var invalidCharacters = ['_', '-', '!', '#', '%', '&', '\'', '+', '/', '.', '~', '$', '|', '{', '}', '^'];
        
        $.each(invalidCharacters, function (index, value) {
            emailAddress = 'test@' + value + '.com' ;
            model.emailAddress(emailAddress);
            isValid = model.validationState().emailAddress.isValid();
            expect(isValid).toBeFalsy();
        });


    });

});