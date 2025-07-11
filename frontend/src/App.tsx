import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from 'react-query';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './contexts/AuthContext.tsx';
import { ThemeProvider } from './contexts/ThemeContext.tsx';
import Layout from './components/Layout/Layout.tsx';
import Login from './pages/Auth/Login.tsx';
import Register from './pages/Auth/Register.tsx';
import Dashboard from './pages/Dashboard/Dashboard.tsx';
import FileManager from './pages/FileManager/FileManager.tsx';
import FileUpload from './pages/FileUpload/FileUpload.tsx';
import FileHash from './pages/FileHash/FileHash.tsx';
import SecurityCenter from './pages/SecurityCenter/SecurityCenter.tsx';
import Settings from './pages/Settings/Settings.tsx';
import ProtectedRoute from './components/Auth/ProtectedRoute.tsx';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <div className="App">
            <Routes>
              {/* Public Routes */}
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              
              {/* Protected Routes */}
              <Route path="/" element={
                <ProtectedRoute>
                  <Layout />
                </ProtectedRoute>
              }>
                <Route index element={<Dashboard />} />
                <Route path="files" element={<FileManager />} />
                <Route path="upload" element={<FileUpload />} />
                <Route path="hash" element={<FileHash />} />
                <Route path="security" element={<SecurityCenter />} />
                <Route path="settings" element={<Settings />} />
              </Route>
            </Routes>
            
            <Toaster
              position="top-right"
              toastOptions={{
                duration: 4000,
                style: {
                  background: '#1e293b',
                  color: '#f8fafc',
                  border: '1px solid #334155',
                },
                success: {
                  iconTheme: {
                    primary: '#22c55e',
                    secondary: '#f8fafc',
                  },
                },
                error: {
                  iconTheme: {
                    primary: '#ef4444',
                    secondary: '#f8fafc',
                  },
                },
              }}
            />
          </div>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App; 