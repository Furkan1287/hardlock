import React, { useContext } from 'react';
import { Bell, Search, Moon, Sun, LogOut } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext.tsx';
import { useTheme } from '../../contexts/ThemeContext.tsx';
import { AuthContext } from '../../contexts/AuthContext.tsx';

const Header: React.FC = () => {
  const { logout } = useAuth();
  const { theme, toggleTheme } = useTheme();

  return (
    <header className="bg-dark-900 border-b border-dark-800 px-8 py-5 shadow-inner-glow animate-fade-in">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-6">
          <div className="relative w-96">
            <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-danger-500" />
            <input
              type="text"
              placeholder="Search files, users, or settings..."
              className="pl-12 pr-4 py-3 bg-dark-800 border border-dark-700 rounded-xl text-white placeholder-dark-400 focus:outline-none focus:ring-2 focus:ring-danger-500 focus:border-transparent w-full transition-all duration-200"
            />
          </div>
        </div>
        <div className="flex items-center space-x-6">
          <button
            onClick={toggleTheme}
            className="p-3 text-danger-500 hover:text-white hover:bg-dark-800 rounded-xl transition-colors duration-200"
          >
            {theme === 'dark' ? <Sun className="w-6 h-6" /> : <Moon className="w-6 h-6" />}
          </button>
          <button className="relative p-3 text-danger-500 hover:text-white hover:bg-dark-800 rounded-xl transition-colors duration-200 animate-pulse-glow">
            <Bell className="w-6 h-6" />
            <span className="absolute top-2 right-2 w-3 h-3 bg-danger-500 rounded-full border-2 border-dark-900 animate-ping"></span>
          </button>
          <button
            onClick={logout}
            className="flex items-center space-x-2 px-4 py-3 text-danger-400 hover:text-white hover:bg-danger-700/80 rounded-xl transition-colors duration-200"
          >
            <LogOut className="w-5 h-5" />
            <span className="text-base font-semibold">Logout</span>
          </button>
        </div>
      </div>
    </header>
  );
};

export default Header; 