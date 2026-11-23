import React from 'react';
import AuthForm from './AuthForm';
import service from './service.js';

// דף התחברות - משתמש קיים
function Login() {
  return (
    <AuthForm
      title="ברוכה"
      highlight="השבה."
      subtitle="המשימות שלך מחכות לך. בואי נמשיך מאיפה שעצרת."
      buttonText="התחברות ←"
      onSubmit={service.login}
      switchText="עוד אין לך משתמש?"
      switchLink="להרשמה"
      switchHref="/register"
    />
  );
}

export default Login;
