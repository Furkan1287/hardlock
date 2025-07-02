import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { User, Mail, Lock, Key, LogOut } from 'lucide-react';

const Settings: React.FC = () => {
  const { user, logout } = useAuth();
  const [firstName, setFirstName] = useState(user?.firstName || '');
  const [lastName, setLastName] = useState(user?.lastName || '');
  const [email, setEmail] = useState(user?.email || '');
  const [password, setPassword] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    // TODO: Save profile changes via API
    setTimeout(() => setIsSaving(false), 1000);
  };

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <div>
        <h1 className="text-2xl font-bold text-white">Settings</h1>
        <p className="text-dark-400 mt-1">Manage your profile and account settings</p>
      </div>

      <form className="card p-6 space-y-6" onSubmit={handleSave}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white mb-1">First Name</label>
            <div className="relative">
              <User className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-dark-400" />
              <input
                type="text"
                value={firstName}
                onChange={e => setFirstName(e.target.value)}
                className="input pl-10 w-full"
                placeholder="First name"
              />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-white mb-1">Last Name</label>
            <div className="relative">
              <User className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-dark-400" />
              <input
                type="text"
                value={lastName}
                onChange={e => setLastName(e.target.value)}
                className="input pl-10 w-full"
                placeholder="Last name"
              />
            </div>
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-white mb-1">Email</label>
          <div className="relative">
            <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-dark-400" />
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              className="input pl-10 w-full"
              placeholder="Email address"
            />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-white mb-1">New Password</label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-dark-400" />
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              className="input pl-10 w-full"
              placeholder="New password"
            />
          </div>
        </div>
        <div className="flex justify-end space-x-4">
          <button
            type="button"
            className="btn-secondary flex items-center"
            onClick={logout}
          >
            <LogOut className="h-4 w-4 mr-2" />
            Logout
          </button>
          <button
            type="submit"
            className="btn-primary flex items-center"
            disabled={isSaving}
          >
            <Key className="h-4 w-4 mr-2" />
            {isSaving ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default Settings; 