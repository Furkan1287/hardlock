import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext.tsx';
import { 
  Shield, 
  FileText, 
  Upload, 
  Download, 
  AlertTriangle,
  CheckCircle,
  Clock,
  Activity
} from 'lucide-react';
import { AuthContext } from '../../contexts/AuthContext.tsx';

interface DashboardStats {
  totalFiles: number;
  totalSize: number;
  encryptedFiles: number;
  recentActivity: number;
}

interface RecentFile {
  id: string;
  name: string;
  size: number;
  uploadedAt: string;
  isEncrypted: boolean;
}

interface SecurityAlert {
  id: string;
  type: 'warning' | 'error' | 'info';
  message: string;
  timestamp: string;
}

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState<DashboardStats>({
    totalFiles: 0,
    totalSize: 0,
    encryptedFiles: 0,
    recentActivity: 0
  });
  const [recentFiles, setRecentFiles] = useState<RecentFile[]>([]);
  const [alerts, setAlerts] = useState<SecurityAlert[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Simulate loading dashboard data
    const loadDashboardData = async () => {
      setIsLoading(true);
      
      // Simulate API calls
      setTimeout(() => {
        setStats({
          totalFiles: 156,
          totalSize: 2.4 * 1024 * 1024 * 1024, // 2.4 GB
          encryptedFiles: 142,
          recentActivity: 23
        });

        setRecentFiles([
          {
            id: '1',
            name: 'confidential_report.pdf',
            size: 15.2 * 1024 * 1024, // 15.2 MB
            uploadedAt: '2024-01-15T10:30:00Z',
            isEncrypted: true
          },
          {
            id: '2',
            name: 'project_documentation.docx',
            size: 8.7 * 1024 * 1024, // 8.7 MB
            uploadedAt: '2024-01-15T09:15:00Z',
            isEncrypted: true
          },
          {
            id: '3',
            name: 'presentation.pptx',
            size: 25.1 * 1024 * 1024, // 25.1 MB
            uploadedAt: '2024-01-14T16:45:00Z',
            isEncrypted: false
          }
        ]);

        setAlerts([
          {
            id: '1',
            type: 'warning',
            message: '3 files require encryption for compliance',
            timestamp: '2024-01-15T11:00:00Z'
          },
          {
            id: '2',
            type: 'info',
            message: 'Backup completed successfully',
            timestamp: '2024-01-15T10:30:00Z'
          }
        ]);

        setIsLoading(false);
      }, 1000);
    };

    loadDashboardData();
  }, []);

  const formatFileSize = (bytes: number): string => {
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto mb-4"></div>
          <p className="text-gray-400">Loading dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-white mb-2">
                Welcome back, {user?.firstName || 'User'}!
              </h1>
              <p className="text-gray-400">
                Here's what's happening with your secure files today
              </p>
            </div>
            <div className="bg-gradient-to-r from-red-600 to-red-800 p-3 rounded-xl">
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Total Files</p>
                <p className="text-3xl font-bold text-white">{stats.totalFiles}</p>
              </div>
              <div className="bg-blue-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Storage Used</p>
                <p className="text-3xl font-bold text-white">{formatFileSize(stats.totalSize)}</p>
              </div>
              <div className="bg-green-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Encrypted Files</p>
                <p className="text-3xl font-bold text-white">{stats.encryptedFiles}</p>
              </div>
              <div className="bg-red-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Recent Activity</p>
                <p className="text-3xl font-bold text-white">{stats.recentActivity}</p>
              </div>
              <div className="bg-purple-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Recent Files */}
          <div className="lg:col-span-2 bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Recent Files</h2>
              <button className="text-red-400 hover:text-red-300 text-sm font-medium transition-colors duration-200">
                View All
              </button>
            </div>
            <div className="space-y-4">
              {recentFiles.map((file) => (
                <div key={file.id} className="flex items-center justify-between p-4 bg-gray-700/30 rounded-lg border border-gray-600 hover:border-red-500/50 transition-all duration-200">
                  <div className="flex items-center space-x-4">
                    <div className={`p-2 rounded-lg ${file.isEncrypted ? 'bg-red-500/20' : 'bg-yellow-500/20'}`}>
                      <svg className={`h-5 w-5 ${file.isEncrypted ? 'text-red-400' : 'text-yellow-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                    </div>
                    <div>
                      <p className="text-white font-medium">{file.name}</p>
                      <p className="text-gray-400 text-sm">{formatFileSize(file.size)}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-gray-400 text-sm">{formatDate(file.uploadedAt)}</p>
                    {file.isEncrypted && (
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-500/20 text-red-400">
                        Encrypted
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Security Alerts */}
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Security Alerts</h2>
              <button className="text-red-400 hover:text-red-300 text-sm font-medium transition-colors duration-200">
                View All
              </button>
            </div>
            <div className="space-y-4">
              {alerts.map((alert) => (
                <div key={alert.id} className={`p-4 rounded-lg border ${
                  alert.type === 'error' ? 'bg-red-500/10 border-red-500/30' :
                  alert.type === 'warning' ? 'bg-yellow-500/10 border-yellow-500/30' :
                  'bg-blue-500/10 border-blue-500/30'
                }`}>
                  <div className="flex items-start space-x-3">
                    <div className={`p-1 rounded-full ${
                      alert.type === 'error' ? 'bg-red-500/20' :
                      alert.type === 'warning' ? 'bg-yellow-500/20' :
                      'bg-blue-500/20'
                    }`}>
                      <svg className={`h-4 w-4 ${
                        alert.type === 'error' ? 'text-red-400' :
                        alert.type === 'warning' ? 'text-yellow-400' :
                        'text-blue-400'
                      }`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div className="flex-1">
                      <p className="text-white text-sm">{alert.message}</p>
                      <p className="text-gray-400 text-xs mt-1">{formatDate(alert.timestamp)}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-6">Quick Actions</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-red-600 to-red-800 rounded-xl hover:from-red-700 hover:to-red-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
              <span className="text-white font-medium">Upload File</span>
            </button>

            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-blue-600 to-blue-800 rounded-xl hover:from-blue-700 hover:to-blue-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span className="text-white font-medium">Encrypt Files</span>
            </button>

            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-green-600 to-green-800 rounded-xl hover:from-green-700 hover:to-green-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
              <span className="text-white font-medium">Security Scan</span>
            </button>

            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-purple-600 to-purple-800 rounded-xl hover:from-purple-700 hover:to-purple-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span className="text-white font-medium">Settings</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 