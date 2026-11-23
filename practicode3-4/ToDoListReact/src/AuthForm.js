import React, { useState } from 'react';
import { Layout } from './App';

// טופס משותף לדף ההתחברות ולדף ההרשמה
function AuthForm({ title, highlight, subtitle, buttonText, onSubmit, switchText, switchLink, switchHref }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      await onSubmit(username, password);
      window.location.href = "/"; // הצליח -> חזרה לרשימת המשימות
    } catch (err) {
      const status = err.response?.status;
      if (status === 401) setError("שם משתמש או סיסמה שגויים");
      else if (status === 409) setError("שם המשתמש כבר תפוס");
      else if (status === 400) setError("יש למלא שם משתמש וסיסמה");
      else setError("אירעה שגיאה, נסו שוב");
    }
  }

  return (
    <Layout>
      <section className="hero">
        <h1>{title} <span className="glow">{highlight}</span></h1>
        <p>{subtitle}</p>
      </section>
      <section className="card">
        <form className="auth" onSubmit={handleSubmit}>
          <input className="field" placeholder="שם משתמש" value={username}
            onChange={(e) => setUsername(e.target.value)} autoFocus />
          <input className="field" type="password" placeholder="סיסמה" value={password}
            onChange={(e) => setPassword(e.target.value)} />
          <button className="btn full" type="submit">{buttonText}</button>
          {error && <p className="error">{error}</p>}
        </form>
        <p className="switch">{switchText} <a href={switchHref}>{switchLink}</a></p>
      </section>
    </Layout>
  );
}

export default AuthForm;
