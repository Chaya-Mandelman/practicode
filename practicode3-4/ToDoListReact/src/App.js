import React, { useEffect, useState } from 'react';
import service from './service.js';
import Login from './Login';
import Register from './Register';

// רקע אנימטיבי משותף לכל הדפים
export function Layout({ children, showLogout }) {
  return (
    <>
      <div className="bg">
        <span className="blob b1"></span>
        <span className="blob b2"></span>
        <span className="blob b3"></span>
      </div>
      <main className="shell">
        <div className="brand">
          <div className="logo">
            <span className="tag">&lt;</span>
            <span className="name">todo</span>
            <span className="tag"> /&gt;</span>
          </div>
          {showLogout && <button className="chip" onClick={service.logout}>התנתקות ↩</button>}
        </div>
        {children}
        <p className="foot">built with ♥ &amp; React · practicode</p>
      </main>
    </>
  );
}

function Todos() {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);

  async function getTodos() {
    const todos = await service.getTasks();
    setTodos(todos);
  }

  async function createTodo(e) {
    e.preventDefault();
    if (!newTodo.trim()) return;
    await service.addTask(newTodo);
    setNewTodo("");//clear input
    await getTodos();//refresh tasks list (in order to see the new one)
  }

  async function updateCompleted(todo, isComplete) {
    await service.setCompleted(todo.id, isComplete);
    await getTodos();//refresh tasks list (in order to see the updated one)
  }

  async function deleteTodo(id) {
    await service.deleteTask(id);
    await getTodos();//refresh tasks list
  }

  useEffect(() => {
    getTodos();
  }, []);

  const done = todos.filter(t => t.isComplete).length;
  const percent = todos.length ? Math.round((done / todos.length) * 100) : 0;

  return (
    <Layout showLogout>
      <section className="hero">
        <h1>היום את <span className="glow">כותבת קוד</span><br />וגם סוגרת משימות.</h1>
        <p>כל משימה שנסגרת היא commit קטן לחיים שלך ✨</p>
      </section>

      <section className="card">
        <div className="stats">
          <div className="stat"><b>{todos.length}</b><span>משימות</span></div>
          <div className="stat"><b>{todos.length - done}</b><span>בתהליך</span></div>
          <div className="stat"><b>{percent}%</b><span>הושלמו</span></div>
        </div>
        <div className="progress"><i style={{ width: `${percent}%` }}></i></div>

        <form className="add" onSubmit={createTodo}>
          <input className="field" placeholder="מה הפיצ'ר הבא שלך?" value={newTodo} onChange={(e) => setNewTodo(e.target.value)} />
          <button className="btn" type="submit">+ הוספה</button>
        </form>

        {todos.length === 0 ? (
          <div className="empty">
            <code>while (!done) {"{ dream(); }"}</code>
            <div>הרשימה ריקה. הוסיפי את המשימה הראשונה 🚀</div>
          </div>
        ) : (
          <ul className="list">
            {todos.map(todo => (
              <li className={`item ${todo.isComplete ? "done" : ""}`} key={todo.id}>
                <input className="check" type="checkbox" aria-label="סימון כהושלם" defaultChecked={todo.isComplete} onChange={(e) => updateCompleted(todo, e.target.checked)} />
                <span className="text">{todo.name}</span>
                <button className="del" aria-label="מחיקה" onClick={() => deleteTodo(todo.id)}>×</button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </Layout>
  );
}

// בחירת הדף לפי הכתובת בדפדפן
function App() {
  const path = window.location.pathname;
  if (path === "/login") return <Login />;
  if (path === "/register") return <Register />;
  return <Todos />;
}

export default App;
