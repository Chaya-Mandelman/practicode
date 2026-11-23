import axios from 'axios';

// כתובת ה-API כברירת מחדל (Config Defaults)
axios.defaults.baseURL = process.env.REACT_APP_API_URL || "https://practicode34-mhas.onrender.com";

// מוסיף את הטוקן לכל בקשה שיוצאת
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor שתופס שגיאות ב-response ורושם ללוג
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("API error:", error.response?.status, error.message);

    // 401 = לא מחובר / הטוקן פג תוקף -> מעבר לדף לוגין
    // (חוץ מבקשות של לוגין/הרשמה עצמן, כדי שאפשר יהיה להציג הודעת שגיאה)
    const url = error.config?.url || "";
    const isAuthRequest = url.includes("/login") || url.includes("/register");
    if (error.response?.status === 401 && !isAuthRequest) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }

    return Promise.reject(error);
  }
);

export default {
  login: async (username, password) => {
    const result = await axios.post(`/login`, { username, password });
    localStorage.setItem("token", result.data.token);
  },

  register: async (username, password) => {
    const result = await axios.post(`/register`, { username, password });
    localStorage.setItem("token", result.data.token);
  },

  logout: () => {
    localStorage.removeItem("token");
    window.location.href = "/login";
  },

  getTasks: async () => {
    const result = await axios.get(`/items`);
    return result.data;
  },

  addTask: async (name) => {
    const result = await axios.post(`/items`, { name, isComplete: false });
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    const result = await axios.put(`/items/${id}`, { isComplete });
    return result.data;
  },

  deleteTask: async (id) => {
    await axios.delete(`/items/${id}`);
  }
};
