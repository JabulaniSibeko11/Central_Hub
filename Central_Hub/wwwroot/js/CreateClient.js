
            // Email domain validation
    document.getElementById('admin_email').addEventListener('blur', function() {
                const email = this.value;
    const domain = document.querySelector('[name="EmailDomain"]').value;

    if (email && domain) {
                    const emailDomain = email.split('@')[1];
    if (emailDomain !== domain) {
        this.setCustomValidity('Email must match company domain (@' + domain + ')');
    this.reportValidity();
                    } else {
        this.setCustomValidity('');
                    }
                }
            });

    // Auto-format phone number
    document.getElementById('admin_phone').addEventListener('input', function(e) {
        let value = e.target.value.replace(/\D/g, '');
    if (value.startsWith('27')) {
        value = value.substring(2);
                } else if (value.startsWith('0')) {
        value = value.substring(1);
                }
                
                if (value.length > 0) {
        e.target.value = '+27 ' + value.match(/.{1,2}/g)?.join(' ').substring(0, 15) || value;
                }
            });

    // Clean email domain input
    document.querySelector('[name="EmailDomain"]').addEventListener('input', function(e) {
        e.target.value = e.target.value.replace(/[^a-zA-Z0-9.-]/g, '').toLowerCase();
            });
