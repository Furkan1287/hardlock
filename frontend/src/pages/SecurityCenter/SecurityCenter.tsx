import React, { useState, useEffect, useContext } from 'react';
import { Shield, Lock, Key, Globe, Trash2, UserCheck, AlertTriangle, Clock, Calendar } from 'lucide-react';
import { AuthContext } from '../../contexts/AuthContext.tsx';

interface SecurityEvent {
  id: string;
  type: 'login' | 'file_access' | 'encryption' | 'threat' | 'backup';
  severity: 'low' | 'medium' | 'high' | 'critical';
  description: string;
  timestamp: string;
  source: string;
  resolved: boolean;
}

interface SecurityMetrics {
  totalThreats: number;
  blockedAttempts: number;
  encryptionRate: number;
  lastBackup: string;
  securityScore: number;
}

interface ActiveSession {
  id: string;
  device: string;
  location: string;
  ipAddress: string;
  lastActivity: string;
  isCurrent: boolean;
}

const SecurityCenter: React.FC = () => {
  const [timelockFiles, setTimelockFiles] = useState([
    {
      id: '1',
      fileName: 'confidential_contract.pdf',
      unlockAt: '2024-12-31T23:59:59',
      blockNumber: 20000000,
      timelockType: 'hybrid',
      status: 'locked'
    },
    {
      id: '2',
      fileName: 'will_document.docx',
      unlockAt: '2025-06-15T12:00:00',
      blockNumber: null,
      timelockType: 'timestamp',
      status: 'locked'
    }
  ]);

  const [geoFencedFiles, setGeoFencedFiles] = useState([
    {
      id: '1',
      fileName: 'company_secrets.pdf',
      allowedCountries: ['Turkey', 'Germany'],
      allowedCities: ['Istanbul', 'Berlin'],
      geoFencingType: 'hybrid',
      status: 'active'
    },
    {
      id: '2',
      fileName: 'local_documents.docx',
      allowedCountries: ['Turkey'],
      allowedCities: ['Istanbul'],
      geoFencingType: 'city',
      status: 'active'
    }
  ]);

  const [events, setEvents] = useState<SecurityEvent[]>([]);
  const [metrics, setMetrics] = useState<SecurityMetrics>({
    totalThreats: 0,
    blockedAttempts: 0,
    encryptionRate: 0,
    lastBackup: '',
    securityScore: 0
  });
  const [sessions, setSessions] = useState<ActiveSession[]>([]);
  const [selectedFilter, setSelectedFilter] = useState<'all' | 'threats' | 'access' | 'encryption'>('all');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Simulate loading security data
    const loadSecurityData = async () => {
      setIsLoading(true);
      
      setTimeout(() => {
        setMetrics({
          totalThreats: 3,
          blockedAttempts: 12,
          encryptionRate: 94.5,
          lastBackup: '2024-01-15T10:30:00Z',
          securityScore: 98
        });

        setEvents([
          {
            id: '1',
            type: 'threat',
            severity: 'high',
            description: 'Suspicious login attempt from unknown IP',
            timestamp: '2024-01-15T11:45:00Z',
            source: '192.168.1.100',
            resolved: false
          },
          {
            id: '2',
            type: 'file_access',
            severity: 'medium',
            description: 'Unauthorized access attempt to confidential files',
            timestamp: '2024-01-15T10:30:00Z',
            source: 'User: john.doe@company.com',
            resolved: true
          },
          {
            id: '3',
            type: 'encryption',
            severity: 'low',
            description: 'File encryption completed successfully',
            timestamp: '2024-01-15T09:15:00Z',
            source: 'System',
            resolved: true
          },
          {
            id: '4',
            type: 'backup',
            severity: 'low',
            description: 'Automated backup completed',
            timestamp: '2024-01-15T08:00:00Z',
            source: 'System',
            resolved: true
          },
          {
            id: '5',
            type: 'login',
            severity: 'medium',
            description: 'New device login detected',
            timestamp: '2024-01-14T16:20:00Z',
            source: 'Mobile Device - iPhone',
            resolved: true
          }
        ]);

        setSessions([
          {
            id: '1',
            device: 'MacBook Pro',
            location: 'New York, NY',
            ipAddress: '192.168.1.50',
            lastActivity: '2024-01-15T11:50:00Z',
            isCurrent: true
          },
          {
            id: '2',
            device: 'iPhone 15',
            location: 'New York, NY',
            ipAddress: '192.168.1.51',
            lastActivity: '2024-01-15T10:30:00Z',
            isCurrent: false
          },
          {
            id: '3',
            device: 'Windows PC',
            location: 'San Francisco, CA',
            ipAddress: '203.0.113.10',
            lastActivity: '2024-01-14T18:45:00Z',
            isCurrent: false
          }
        ]);

        setIsLoading(false);
      }, 1000);
    };

    loadSecurityData();
  }, []);

  const getFilteredEvents = () => {
    if (selectedFilter === 'all') return events;
    return events.filter(event => {
      switch (selectedFilter) {
        case 'threats':
          return event.type === 'threat';
        case 'access':
          return event.type === 'file_access' || event.type === 'login';
        case 'encryption':
          return event.type === 'encryption' || event.type === 'backup';
        default:
          return true;
      }
    });
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical':
        return 'bg-red-500/20 text-red-400 border-red-500/30';
      case 'high':
        return 'bg-orange-500/20 text-orange-400 border-orange-500/30';
      case 'medium':
        return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30';
      case 'low':
        return 'bg-green-500/20 text-green-400 border-green-500/30';
      default:
        return 'bg-gray-500/20 text-gray-400 border-gray-500/30';
    }
  };

  const getEventIcon = (type: string) => {
    const iconMap: { [key: string]: string } = {
      login: 'M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1',
      file_access: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      encryption: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z',
      threat: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z',
      backup: 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z'
    };
    return iconMap[type] || 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z';
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleResolveEvent = (eventId: string) => {
    setEvents(prev => prev.map(event =>
      event.id === eventId ? { ...event, resolved: true } : event
    ));
  };

  const handleTerminateSession = (sessionId: string) => {
    setSessions(prev => prev.filter(session => session.id !== sessionId));
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto mb-4"></div>
          <p className="text-gray-400">Loading security data...</p>
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
              <h1 className="text-3xl font-bold text-white mb-2">Security Center</h1>
              <p className="text-gray-400">
                Monitor and manage your security status
              </p>
            </div>
            <div className="bg-gradient-to-r from-red-600 to-red-800 p-3 rounded-xl">
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Security Metrics */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Security Score</p>
                <p className="text-3xl font-bold text-white">{metrics.securityScore}%</p>
              </div>
              <div className="bg-green-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Total Threats</p>
                <p className="text-3xl font-bold text-white">{metrics.totalThreats}</p>
              </div>
              <div className="bg-red-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Blocked Attempts</p>
                <p className="text-3xl font-bold text-white">{metrics.blockedAttempts}</p>
              </div>
              <div className="bg-blue-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Encryption Rate</p>
                <p className="text-3xl font-bold text-white">{metrics.encryptionRate}%</p>
              </div>
              <div className="bg-purple-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6 hover:border-red-500/50 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-medium">Last Backup</p>
                <p className="text-lg font-bold text-white">{formatDate(metrics.lastBackup)}</p>
              </div>
              <div className="bg-green-500/20 p-3 rounded-xl">
                <svg className="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Security Events */}
          <div className="lg:col-span-2 bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Security Events</h2>
              <div className="flex items-center space-x-2">
                <select
                  value={selectedFilter}
                  onChange={(e) => setSelectedFilter(e.target.value as any)}
                  className="px-3 py-1 bg-gray-700/50 border border-gray-600 rounded-lg text-white text-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all duration-200"
                >
                  <option value="all">All Events</option>
                  <option value="threats">Threats</option>
                  <option value="access">Access</option>
                  <option value="encryption">Encryption</option>
                </select>
              </div>
            </div>
            <div className="space-y-4">
              {getFilteredEvents().map((event) => (
                <div
                  key={event.id}
                  className={`p-4 rounded-lg border transition-all duration-200 ${
                    event.resolved ? 'bg-gray-700/30 border-gray-600' : 'bg-red-500/10 border-red-500/30'
                  }`}
                >
                  <div className="flex items-start space-x-3">
                    <div className={`p-2 rounded-lg ${getSeverityColor(event.severity)}`}>
                      <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={getEventIcon(event.type)} />
                      </svg>
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center justify-between">
                        <p className="text-white font-medium">{event.description}</p>
                        <div className="flex items-center space-x-2">
                          <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getSeverityColor(event.severity)}`}>
                            {event.severity}
                          </span>
                          {!event.resolved && (
                            <button
                              onClick={() => handleResolveEvent(event.id)}
                              className="px-3 py-1 bg-green-600 text-white rounded-lg text-xs hover:bg-green-700 transition-colors duration-200"
                            >
                              Resolve
                            </button>
                          )}
                        </div>
                      </div>
                      <p className="text-gray-400 text-sm mt-1">Source: {event.source}</p>
                      <p className="text-gray-400 text-xs mt-1">{formatDate(event.timestamp)}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Active Sessions */}
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Active Sessions</h2>
              <button className="text-red-400 hover:text-red-300 text-sm font-medium transition-colors duration-200">
                View All
              </button>
            </div>
            <div className="space-y-4">
              {sessions.map((session) => (
                <div key={session.id} className="p-4 bg-gray-700/30 rounded-lg border border-gray-600">
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center space-x-2">
                      <div className={`p-1 rounded-full ${session.isCurrent ? 'bg-green-500/20' : 'bg-blue-500/20'}`}>
                        <svg className={`h-4 w-4 ${session.isCurrent ? 'text-green-400' : 'text-blue-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <span className="text-white font-medium">{session.device}</span>
                      {session.isCurrent && (
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-500/20 text-green-400">
                          Current
                        </span>
                      )}
                    </div>
                    {!session.isCurrent && (
                      <button
                        onClick={() => handleTerminateSession(session.id)}
                        className="text-red-400 hover:text-red-300 text-sm transition-colors duration-200"
                      >
                        Terminate
                      </button>
                    )}
                  </div>
                  <p className="text-gray-400 text-sm">{session.location}</p>
                  <p className="text-gray-400 text-xs">{session.ipAddress}</p>
                  <p className="text-gray-400 text-xs mt-1">Last activity: {formatDate(session.lastActivity)}</p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-6">Security Actions</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-red-600 to-red-800 rounded-xl hover:from-red-700 hover:to-red-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
              <span className="text-white font-medium">Security Scan</span>
            </button>

            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-blue-600 to-blue-800 rounded-xl hover:from-blue-700 hover:to-blue-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span className="text-white font-medium">Encrypt All</span>
            </button>

            <button className="flex items-center justify-center space-x-3 p-4 bg-gradient-to-r from-green-600 to-green-800 rounded-xl hover:from-green-700 hover:to-green-900 transition-all duration-200 transform hover:scale-105">
              <svg className="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span className="text-white font-medium">Backup Now</span>
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

export default SecurityCenter; 