import React from 'react';
import AuthForm from './AuthForm';
import service from './service.js';

// דף הרשמה - משתמש חדש
function Register() {
  return (
    <AuthForm
      title="register"
      buttonText="הרשמה"
      onSubmit={service.register}
      switchText="כבר יש לך משתמש? להתחברות"
      switchHref="/login"
    />
  );
}

export default Register;
