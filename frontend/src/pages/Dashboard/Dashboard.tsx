import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
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

const Dashboard: React.FC = () => {
  const { user } = useAuth();

  const stats = [
    {
      name: 'Total Files',
      value: '1,234',
      change: '+12%',
      changeType: 'positive',
      icon: FileText,
    },
    {
      name: 'Storage Used',
      value: '2.4 GB',
      change: '+8%',
      changeType: 'positive',
      icon: Upload,
    },
    {
      name: 'Security Score',
      value: '98%',
      change: '+2%',
      changeType: 'positive',
      icon: Shield,
    },
    {
      name: 'Active Sessions',
      value: '3',
      change: '-1',
      changeType: 'negative',
      icon: Activity,
    },
  ];

  const recentActivity = [
    {
      id: 1,
      type: 'upload',
      description: 'Document.pdf uploaded',
      time: '2 minutes ago',
      icon: Upload,
      status: 'success',
    },
    {
      id: 2,
      type: 'download',
      description: 'Report.xlsx downloaded',
      time: '15 minutes ago',
      icon: Download,
      status: 'success',
    },
    {
      id: 3,
      type: 'security',
      description: 'Security scan completed',
      time: '1 hour ago',
      icon: Shield,
      status: 'success',
    },
    {
      id: 4,
      type: 'warning',
      description: 'Suspicious login attempt detected',
      time: '2 hours ago',
      icon: AlertTriangle,
      status: 'warning',
    },
  ];

  const securityAlerts = [
    {
      id: 1,
      title: 'Multiple failed login attempts',
      description: '3 failed login attempts detected from IP 192.168.1.100',
      severity: 'high',
      time: '5 minutes ago',
    },
    {
      id: 2,
      title: 'New device login',
      description: 'Login detected from new device in New York, NY',
      severity: 'medium',
      time: '1 hour ago',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Welcome Section */}
      <div className="bg-gradient-primary rounded-lg p-6 text-white">
        <h1 className="text-2xl font-bold">
          Welcome back, {user?.firstName}!
        </h1>
        <p className="text-primary-100 mt-2">
          Your files are secure and protected with military-grade encryption.
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div key={stat.name} className="card p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <Icon className="h-8 w-8 text-primary-500" />
                </div>
                <div className="ml-4 flex-1">
                  <p className="text-sm font-medium text-dark-400">{stat.name}</p>
                  <p className="text-2xl font-bold text-white">{stat.value}</p>
                </div>
              </div>
              <div className="mt-4">
                <span
                  className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    stat.changeType === 'positive'
                      ? 'bg-success-100 text-success-800'
                      : 'bg-danger-100 text-danger-800'
                  }`}
                >
                  {stat.change}
                </span>
                <span className="text-xs text-dark-400 ml-2">from last month</span>
              </div>
            </div>
          );
        })}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Activity */}
        <div className="card">
          <div className="p-6 border-b border-dark-700">
            <h3 className="text-lg font-medium text-white">Recent Activity</h3>
          </div>
          <div className="p-6">
            <div className="space-y-4">
              {recentActivity.map((activity) => {
                const Icon = activity.icon;
                return (
                  <div key={activity.id} className="flex items-center space-x-3">
                    <div className={`p-2 rounded-lg ${
                      activity.status === 'success' 
                        ? 'bg-success-100 text-success-600' 
                        : 'bg-warning-100 text-warning-600'
                    }`}>
                      <Icon className="h-4 w-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-sm font-medium text-white">{activity.description}</p>
                      <p className="text-xs text-dark-400">{activity.time}</p>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Security Alerts */}
        <div className="card">
          <div className="p-6 border-b border-dark-700">
            <h3 className="text-lg font-medium text-white">Security Alerts</h3>
          </div>
          <div className="p-6">
            <div className="space-y-4">
              {securityAlerts.map((alert) => (
                <div key={alert.id} className="border-l-4 border-warning-500 pl-4">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-sm font-medium text-white">{alert.title}</p>
                      <p className="text-xs text-dark-400 mt-1">{alert.description}</p>
                      <p className="text-xs text-dark-400 mt-1">{alert.time}</p>
                    </div>
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      alert.severity === 'high' 
                        ? 'bg-danger-100 text-danger-800' 
                        : 'bg-warning-100 text-warning-800'
                    }`}>
                      {alert.severity}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <div className="p-6 border-b border-dark-700">
          <h3 className="text-lg font-medium text-white">Quick Actions</h3>
        </div>
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button className="flex items-center justify-center space-x-2 p-4 border border-dark-600 rounded-lg hover:border-primary-500 hover:bg-primary-50 hover:bg-opacity-5 transition-colors duration-200">
              <Upload className="h-5 w-5 text-primary-500" />
              <span className="text-white">Upload Files</span>
            </button>
            <button className="flex items-center justify-center space-x-2 p-4 border border-dark-600 rounded-lg hover:border-primary-500 hover:bg-primary-50 hover:bg-opacity-5 transition-colors duration-200">
              <Shield className="h-5 w-5 text-primary-500" />
              <span className="text-white">Security Scan</span>
            </button>
            <button className="flex items-center justify-center space-x-2 p-4 border border-dark-600 rounded-lg hover:border-primary-500 hover:bg-primary-50 hover:bg-opacity-5 transition-colors duration-200">
              <Activity className="h-5 w-5 text-primary-500" />
              <span className="text-white">View Reports</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 