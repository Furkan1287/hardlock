import React, { useState } from 'react';
import { Shield, Lock, Key, Globe, Trash2, UserCheck, AlertTriangle, Clock, Calendar } from 'lucide-react';

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

  const features = [
    {
      icon: <Shield className="h-6 w-6 text-primary-500" />,
      title: 'AES-256-GCM Encryption',
      description: 'All files are encrypted with advanced AES-256-GCM before storage.'
    },
    {
      icon: <Key className="h-6 w-6 text-success-500" />,
      title: 'Biometric Key Derivation',
      description: 'Optionally derive encryption keys from biometric data for extra security.'
    },
    {
      icon: <Lock className="h-6 w-6 text-warning-500" />,
      title: 'Sharded Encryption',
      description: 'Files are split into shards and encrypted separately for maximum resilience.'
    },
    {
      icon: <Globe className="h-6 w-6 text-dark-400" />,
      title: 'Geographic Locking',
      description: 'Restrict file access to specific countries or regions.'
    },
    {
      icon: <UserCheck className="h-6 w-6 text-primary-400" />,
      title: 'Multi-User Access Control',
      description: 'Granular permissions for teams and organizations.'
    },
    {
      icon: <Trash2 className="h-6 w-6 text-danger-500" />,
      title: 'Self-Destruct Mechanisms',
      description: 'Files can be set to self-destruct after a certain time or failed login attempts.'
    },
    {
      icon: <AlertTriangle className="h-6 w-6 text-danger-400" />,
      title: 'Audit & Alerts',
      description: 'All access and actions are logged and monitored for suspicious activity.'
    },
    {
      icon: <Globe className="h-6 w-6 text-purple-500" />,
      title: 'Darknet Backup',
      description: 'Files are distributed across Tor network using IPFS for ultimate resilience.'
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-white">Security Center</h1>
        <p className="text-dark-400 mt-1">Review and manage your security settings and features</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {features.map((feature, idx) => (
          <div key={idx} className="card-hover p-6 flex flex-col items-start space-y-3">
            {feature.icon}
            <h3 className="text-lg font-semibold text-white">{feature.title}</h3>
            <p className="text-sm text-dark-400">{feature.description}</p>
          </div>
        ))}
      </div>

      <div className="card p-6 mt-8">
        <h3 className="text-lg font-medium text-white mb-4">Security Actions</h3>
        <div className="flex flex-wrap gap-4">
          <button className="btn-primary">Run Security Scan</button>
          <button className="btn-warning">Enable Self-Destruct</button>
          <button className="btn-success">Download Audit Log</button>
          <button className="btn-purple">Backup to Darknet</button>
        </div>
      </div>

      {/* Timelock Files Management */}
      <div className="card p-6">
        <div className="flex items-center space-x-3 mb-6">
          <Clock className="h-6 w-6 text-primary-500" />
          <h3 className="text-lg font-medium text-white">Time Lock Files</h3>
        </div>
        
        {timelockFiles.length === 0 ? (
          <div className="text-center py-8">
            <Calendar className="h-12 w-12 text-dark-400 mx-auto mb-4" />
            <p className="text-dark-400">No timelock files found</p>
          </div>
        ) : (
          <div className="space-y-4">
            {timelockFiles.map((file) => (
              <div key={file.id} className="flex items-center justify-between p-4 bg-dark-800 rounded-lg">
                <div className="flex items-center space-x-4">
                  <div className="p-2 bg-primary-500 bg-opacity-20 rounded-lg">
                    <Clock className="h-5 w-5 text-primary-500" />
                  </div>
                  <div>
                    <h4 className="font-medium text-white">{file.fileName}</h4>
                    <div className="flex items-center space-x-4 text-sm text-dark-400">
                      <span className="capitalize">{file.timelockType}</span>
                      {file.unlockAt && (
                        <span>Unlocks: {new Date(file.unlockAt).toLocaleDateString()}</span>
                      )}
                      {file.blockNumber && (
                        <span>Block: {file.blockNumber.toLocaleString()}</span>
                      )}
                    </div>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    file.status === 'locked' 
                      ? 'bg-warning-900 text-warning-300' 
                      : 'bg-success-900 text-success-300'
                  }`}>
                    {file.status}
                  </span>
                  <button className="text-dark-400 hover:text-white">
                    <Calendar className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Geo-Fenced Files Management */}
      <div className="card p-6">
        <div className="flex items-center space-x-3 mb-6">
          <Globe className="h-6 w-6 text-info-500" />
          <h3 className="text-lg font-medium text-white">Geo-Fenced Files</h3>
        </div>
        
        {geoFencedFiles.length === 0 ? (
          <div className="text-center py-8">
            <Globe className="h-12 w-12 text-dark-400 mx-auto mb-4" />
            <p className="text-dark-400">No geo-fenced files found</p>
          </div>
        ) : (
          <div className="space-y-4">
            {geoFencedFiles.map((file) => (
              <div key={file.id} className="flex items-center justify-between p-4 bg-dark-800 rounded-lg">
                <div className="flex items-center space-x-4">
                  <div className="p-2 bg-info-500 bg-opacity-20 rounded-lg">
                    <Globe className="h-5 w-5 text-info-500" />
                  </div>
                  <div>
                    <h4 className="font-medium text-white">{file.fileName}</h4>
                    <div className="flex items-center space-x-4 text-sm text-dark-400">
                      <span className="capitalize">{file.geoFencingType}</span>
                      {file.allowedCountries?.length > 0 && (
                        <span>Countries: {file.allowedCountries.join(', ')}</span>
                      )}
                      {file.allowedCities?.length > 0 && (
                        <span>Cities: {file.allowedCities.join(', ')}</span>
                      )}
                    </div>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    file.status === 'active' 
                      ? 'bg-info-900 text-info-300' 
                      : 'bg-warning-900 text-warning-300'
                  }`}>
                    {file.status}
                  </span>
                  <button className="text-dark-400 hover:text-white">
                    <Globe className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SecurityCenter; 