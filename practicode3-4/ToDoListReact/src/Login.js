import React from 'react';
import AuthForm from './AuthForm';
import service from './service.js';

// דף התחברות - משתמש קיים
function Login() {
  return (
    <AuthForm
      title="login"
      buttonText="התחברות"
      onSubmit={service.login}
      switchText="אין לך משתמש? להרשמה"
      switchHref="/register"
    />
  );
}

export default Login;
