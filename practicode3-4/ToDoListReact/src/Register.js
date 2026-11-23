import React from 'react';
import AuthForm from './AuthForm';
import service from './service.js';

// דף הרשמה - משתמש חדש
function Register() {
  return (
    <AuthForm
      title="הצטרפי"
      highlight="לצוות."
      subtitle="פותחים משתמש תוך שנייה ומתחילים לסגור משימות."
      buttonText="יצירת משתמש ←"
      onSubmit={service.register}
      switchText="כבר יש לך משתמש?"
      switchLink="להתחברות"
      switchHref="/login"
    />
  );
}

export default Register;
