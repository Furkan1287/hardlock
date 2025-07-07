import React, { useContext } from 'react';
import { NavLink } from 'react-router-dom';
import { 
  Home, 
  FolderOpen, 
  Upload, 
  Hash,
  Shield, 
  Settings,
  Lock,
  FileText,
  Activity
} from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext.tsx';
import { ThemeContext } from '../../contexts/ThemeContext.tsx';

const Sidebar: React.FC = () => {
  const { user } = useAuth();

  const navigation = [
    { name: 'Dashboard', href: '/', icon: Home },
    { name: 'Files', href: '/files', icon: FolderOpen },
    { name: 'Upload', href: '/upload', icon: Upload },
    { name: 'File Hash', href: '/hash', icon: Hash },
    { name: 'Security Center', href: '/security', icon: Shield },
    { name: 'Settings', href: '/settings', icon: Settings },
  ];

  return (
    <div className="w-64 bg-dark-900 border-r border-dark-800 min-h-screen flex flex-col justify-between animate-fade-in">
      <div className="p-8 pb-2 flex flex-col items-center">
        <img src="/HARDLOCKlogo.png" alt="HARDLOCK Logo" className="w-20 h-20 object-contain rounded-2xl shadow-glow mb-2 animate-pulse-glow" />
        <h1 className="text-2xl font-extrabold text-danger-500 tracking-widest mb-2">HARDLOCK</h1>
      </div>
      <nav className="mt-4 px-3 flex-1">
        <div className="space-y-1">
          {navigation.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.name}
                to={item.href}
                className={({ isActive }) =>
                  `group flex items-center px-4 py-3 text-base font-semibold rounded-xl transition-all duration-200 shadow-none ${
                    isActive
                      ? 'bg-danger-700/80 text-white shadow-glow border-l-4 border-danger-500'
                      : 'text-dark-300 hover:text-danger-400 hover:bg-dark-800/80 hover:shadow-inner-glow'
                  }`
                }
              >
                <Icon className="mr-4 w-6 h-6" />
                {item.name}
              </NavLink>
            );
          })}
        </div>
      </nav>
      <div className="w-full p-5 border-t border-dark-800 glass flex items-center space-x-4 animate-slide-in">
        <div className="w-10 h-10 bg-danger-600 rounded-full flex items-center justify-center font-bold text-lg text-white shadow-glow">
          {user?.firstName?.[0]}{user?.lastName?.[0]}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-base font-semibold text-white truncate">{user?.firstName} {user?.lastName}</p>
          <p className="text-xs text-danger-400 truncate">{user?.email}</p>
        </div>
      </div>
    </div>
  );
};

export default Sidebar; 