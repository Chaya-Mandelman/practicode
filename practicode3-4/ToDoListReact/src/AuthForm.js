import React, { useState } from 'react';

// טופס משותף לדף ההתחברות ולדף ההרשמה
function AuthForm({ title, buttonText, onSubmit, switchText, switchHref }) {
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
    <section className="todoapp">
      <header className="header">
        <h1>{title}</h1>
        <form onSubmit={handleSubmit}>
          <input className="new-todo" placeholder="שם משתמש" value={username}
            onChange={(e) => setUsername(e.target.value)} autoFocus />
          <input className="new-todo" type="password" placeholder="סיסמה" value={password}
            onChange={(e) => setPassword(e.target.value)} />
          <button type="submit" style={{ width: "100%", padding: "16px", fontSize: "20px", cursor: "pointer" }}>
            {buttonText}
          </button>
        </form>
      </header>
      {error && <p style={{ color: "#c0392b", textAlign: "center", padding: "8px" }}>{error}</p>}
      <p style={{ textAlign: "center", padding: "16px" }}>
        <a href={switchHref}>{switchText}</a>
      </p>
    </section>
  );
}

export default AuthForm;
