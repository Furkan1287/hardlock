import React, { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, Lock, Shield, File, X, CheckCircle } from 'lucide-react';

const FileUpload: React.FC = () => {
  const [uploadedFiles, setUploadedFiles] = useState<File[]>([]);
  const [uploadProgress, setUploadProgress] = useState<{ [key: string]: number }>({});
  const [encryptionLevel, setEncryptionLevel] = useState<'standard' | 'military' | 'quantum'>('standard');
  const [enableTimelock, setEnableTimelock] = useState(false);
  const [timelockType, setTimelockType] = useState<'timestamp' | 'block' | 'hybrid'>('timestamp');
  const [unlockAt, setUnlockAt] = useState('');
  const [blockNumber, setBlockNumber] = useState('');
  const [enableGeoFencing, setEnableGeoFencing] = useState(false);
  const [geoFencingType, setGeoFencingType] = useState<'country' | 'city' | 'radius' | 'polygon'>('country');
  const [selectedCountries, setSelectedCountries] = useState<string[]>([]);
  const [selectedCities, setSelectedCities] = useState<string[]>([]);
  const [radiusCenter, setRadiusCenter] = useState({ lat: '', lng: '', radius: '1000' });
  const [enableHashVerification, setEnableHashVerification] = useState(true);
  const [hashAlgorithm, setHashAlgorithm] = useState('SHA256');

  const onDrop = useCallback((acceptedFiles: File[]) => {
    setUploadedFiles(prev => [...prev, ...acceptedFiles]);
    
    // Simulate upload progress
    acceptedFiles.forEach(file => {
      setUploadProgress(prev => ({ ...prev, [file.name]: 0 }));
      
      const interval = setInterval(() => {
        setUploadProgress(prev => {
          const current = prev[file.name] || 0;
          if (current >= 100) {
            clearInterval(interval);
            return prev;
          }
          return { ...prev, [file.name]: current + 10 };
        });
      }, 200);
    });
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/pdf': ['.pdf'],
      'application/msword': ['.doc'],
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx'],
      'application/vnd.ms-excel': ['.xls'],
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-powerpoint': ['.ppt'],
      'application/vnd.openxmlformats-officedocument.presentationml.presentation': ['.pptx'],
      'text/plain': ['.txt'],
      'image/*': ['.jpg', '.jpeg', '.png', '.gif'],
      'video/*': ['.mp4', '.avi', '.mov'],
      'audio/*': ['.mp3', '.wav', '.flac'],
    },
    maxSize: 100 * 1024 * 1024, // 100MB
  });

  const removeFile = (fileName: string) => {
    setUploadedFiles(prev => prev.filter(file => file.name !== fileName));
    setUploadProgress(prev => {
      const newProgress = { ...prev };
      delete newProgress[fileName];
      return newProgress;
    });
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getEncryptionInfo = (level: string) => {
    switch (level) {
      case 'standard':
        return { name: 'AES-256-GCM', description: 'Standard military-grade encryption' };
      case 'military':
        return { name: 'AES-256-GCM + Sharding', description: 'Enhanced with file sharding' };
      case 'quantum':
        return { name: 'Post-Quantum + AES-256', description: 'Quantum-resistant encryption' };
      default:
        return { name: 'AES-256-GCM', description: 'Standard encryption' };
    }
  };

  const encryptionInfo = getEncryptionInfo(encryptionLevel);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-white">Upload Files</h1>
        <p className="text-dark-400 mt-1">Securely upload and encrypt your files</p>
      </div>

      {/* Encryption Level Selection */}
      <div className="card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Encryption Level</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button
            onClick={() => setEncryptionLevel('standard')}
            className={`p-4 border rounded-lg text-left transition-colors duration-200 ${
              encryptionLevel === 'standard'
                ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                : 'border-dark-600 hover:border-dark-500'
            }`}
          >
            <div className="flex items-center space-x-3">
              <Lock className="h-6 w-6 text-primary-500" />
              <div>
                <h4 className="font-medium text-white">Standard</h4>
                <p className="text-sm text-dark-400">AES-256-GCM</p>
              </div>
            </div>
          </button>

          <button
            onClick={() => setEncryptionLevel('military')}
            className={`p-4 border rounded-lg text-left transition-colors duration-200 ${
              encryptionLevel === 'military'
                ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                : 'border-dark-600 hover:border-dark-500'
            }`}
          >
            <div className="flex items-center space-x-3">
              <Shield className="h-6 w-6 text-warning-500" />
              <div>
                <h4 className="font-medium text-white">Military</h4>
                <p className="text-sm text-dark-400">AES-256 + Sharding</p>
              </div>
            </div>
          </button>

          <button
            onClick={() => setEncryptionLevel('quantum')}
            className={`p-4 border rounded-lg text-left transition-colors duration-200 ${
              encryptionLevel === 'quantum'
                ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                : 'border-dark-600 hover:border-dark-500'
            }`}
          >
            <div className="flex items-center space-x-3">
              <Shield className="h-6 w-6 text-success-500" />
              <div>
                <h4 className="font-medium text-white">Quantum</h4>
                <p className="text-sm text-dark-400">Post-Quantum + AES</p>
              </div>
            </div>
          </button>
        </div>
        <div className="mt-4 p-3 bg-dark-800 rounded-lg">
          <p className="text-sm text-white">
            <strong>Selected:</strong> {encryptionInfo.name} - {encryptionInfo.description}
          </p>
        </div>
      </div>

      {/* Timelock Settings */}
      <div className="card p-6">
        <div className="flex items-center space-x-3 mb-4">
          <input
            type="checkbox"
            id="enableTimelock"
            checked={enableTimelock}
            onChange={(e) => setEnableTimelock(e.target.checked)}
            className="w-4 h-4 text-primary-600 bg-dark-700 border-dark-600 rounded focus:ring-primary-500"
          />
          <label htmlFor="enableTimelock" className="text-lg font-medium text-white">
            Enable Time Lock Decryption
          </label>
        </div>
        
        {enableTimelock && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-white mb-2">Timelock Type</label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <button
                  type="button"
                  onClick={() => setTimelockType('timestamp')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    timelockType === 'timestamp'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Timestamp</div>
                    <div className="text-dark-400">Unlock at specific date/time</div>
                  </div>
                </button>
                
                <button
                  type="button"
                  onClick={() => setTimelockType('block')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    timelockType === 'block'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Block Number</div>
                    <div className="text-dark-400">Unlock at Ethereum block</div>
                  </div>
                </button>
                
                <button
                  type="button"
                  onClick={() => setTimelockType('hybrid')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    timelockType === 'hybrid'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Hybrid</div>
                    <div className="text-dark-400">Both timestamp and block</div>
                  </div>
                </button>
              </div>
            </div>

            {(timelockType === 'timestamp' || timelockType === 'hybrid') && (
              <div>
                <label htmlFor="unlockAt" className="block text-sm font-medium text-white mb-2">
                  Unlock Date & Time
                </label>
                <input
                  type="datetime-local"
                  id="unlockAt"
                  value={unlockAt}
                  onChange={(e) => setUnlockAt(e.target.value)}
                  className="w-full px-3 py-2 bg-dark-700 border border-dark-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  min={new Date().toISOString().slice(0, 16)}
                />
              </div>
            )}

            {(timelockType === 'block' || timelockType === 'hybrid') && (
              <div>
                <label htmlFor="blockNumber" className="block text-sm font-medium text-white mb-2">
                  Ethereum Block Number
                </label>
                <input
                  type="number"
                  id="blockNumber"
                  value={blockNumber}
                  onChange={(e) => setBlockNumber(e.target.value)}
                  placeholder="e.g., 20000000"
                  className="w-full px-3 py-2 bg-dark-700 border border-dark-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  min="0"
                />
                <p className="text-xs text-dark-400 mt-1">
                  Current block: ~{Math.floor(Date.now() / 12000)} (estimated)
                </p>
              </div>
            )}

            <div className="p-3 bg-warning-900 bg-opacity-20 border border-warning-700 rounded-lg">
              <p className="text-sm text-warning-300">
                <strong>⚠️ Important:</strong> Files with timelock cannot be decrypted until the specified time/block is reached. 
                Make sure to save the timelock private key securely.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Geo-Fencing Settings */}
      <div className="card p-6">
        <div className="flex items-center space-x-3 mb-4">
          <input
            type="checkbox"
            id="enableGeoFencing"
            checked={enableGeoFencing}
            onChange={(e) => setEnableGeoFencing(e.target.checked)}
            className="w-4 h-4 text-primary-600 bg-dark-700 border-dark-600 rounded focus:ring-primary-500"
          />
          <label htmlFor="enableGeoFencing" className="text-lg font-medium text-white">
            Enable Geographic Locking
          </label>
        </div>
        
        {enableGeoFencing && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-white mb-2">Geo-Fencing Type</label>
              <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                <button
                  type="button"
                  onClick={() => setGeoFencingType('country')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    geoFencingType === 'country'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Country</div>
                    <div className="text-dark-400">Restrict by country</div>
                  </div>
                </button>
                
                <button
                  type="button"
                  onClick={() => setGeoFencingType('city')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    geoFencingType === 'city'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">City</div>
                    <div className="text-dark-400">Restrict by city</div>
                  </div>
                </button>
                
                <button
                  type="button"
                  onClick={() => setGeoFencingType('radius')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    geoFencingType === 'radius'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Radius</div>
                    <div className="text-dark-400">Restrict by distance</div>
                  </div>
                </button>
                
                <button
                  type="button"
                  onClick={() => setGeoFencingType('polygon')}
                  className={`p-3 border rounded-lg text-left transition-colors duration-200 ${
                    geoFencingType === 'polygon'
                      ? 'border-primary-500 bg-primary-50 bg-opacity-10'
                      : 'border-dark-600 hover:border-dark-500'
                  }`}
                >
                  <div className="text-sm">
                    <div className="font-medium text-white">Polygon</div>
                    <div className="text-dark-400">Custom region</div>
                  </div>
                </button>
              </div>
            </div>

            {geoFencingType === 'country' && (
              <div>
                <label className="block text-sm font-medium text-white mb-2">Allowed Countries</label>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                  {['Turkey', 'Germany', 'USA', 'UK', 'France', 'Italy', 'Spain', 'Netherlands'].map(country => (
                    <label key={country} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={selectedCountries.includes(country)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSelectedCountries([...selectedCountries, country]);
                          } else {
                            setSelectedCountries(selectedCountries.filter(c => c !== country));
                          }
                        }}
                        className="w-4 h-4 text-primary-600 bg-dark-700 border-dark-600 rounded"
                      />
                      <span className="text-sm text-white">{country}</span>
                    </label>
                  ))}
                </div>
              </div>
            )}

            {geoFencingType === 'city' && (
              <div>
                <label className="block text-sm font-medium text-white mb-2">Allowed Cities</label>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                  {['Istanbul', 'Ankara', 'Izmir', 'Bursa', 'Antalya', 'Adana', 'Konya', 'Gaziantep'].map(city => (
                    <label key={city} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={selectedCities.includes(city)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSelectedCities([...selectedCities, city]);
                          } else {
                            setSelectedCities(selectedCities.filter(c => c !== city));
                          }
                        }}
                        className="w-4 h-4 text-primary-600 bg-dark-700 border-dark-600 rounded"
                      />
                      <span className="text-sm text-white">{city}</span>
                    </label>
                  ))}
                </div>
              </div>
            )}

            {geoFencingType === 'radius' && (
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Latitude</label>
                  <input
                    type="number"
                    step="any"
                    value={radiusCenter.lat}
                    onChange={(e) => setRadiusCenter({...radiusCenter, lat: e.target.value})}
                    placeholder="41.0082"
                    className="w-full px-3 py-2 bg-dark-700 border border-dark-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Longitude</label>
                  <input
                    type="number"
                    step="any"
                    value={radiusCenter.lng}
                    onChange={(e) => setRadiusCenter({...radiusCenter, lng: e.target.value})}
                    placeholder="28.9784"
                    className="w-full px-3 py-2 bg-dark-700 border border-dark-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Radius (meters)</label>
                  <input
                    type="number"
                    value={radiusCenter.radius}
                    onChange={(e) => setRadiusCenter({...radiusCenter, radius: e.target.value})}
                    className="w-full px-3 py-2 bg-dark-700 border border-dark-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
              </div>
            )}

            <div className="p-3 bg-info-900 bg-opacity-20 border border-info-700 rounded-lg">
              <p className="text-sm text-info-300">
                <strong>📍 Location Lock:</strong> Files will only be accessible from the specified geographic locations. 
                Users outside these areas will be denied access.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Upload Zone */}
      <div className="card p-6">
        <div
          {...getRootProps()}
          className={`upload-zone ${isDragActive ? 'dragover' : ''}`}
        >
          <input {...getInputProps()} />
          <Upload className="mx-auto h-12 w-12 text-dark-400 mb-4" />
          {isDragActive ? (
            <p className="text-lg text-white">Drop the files here...</p>
          ) : (
            <div>
              <p className="text-lg text-white mb-2">Drag & drop files here, or click to select</p>
              <p className="text-sm text-dark-400">
                Supports PDF, DOC, XLS, PPT, images, videos, and more (max 100MB per file)
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Uploaded Files */}
      {uploadedFiles.length > 0 && (
        <div className="card">
          <div className="p-6 border-b border-dark-700">
            <h3 className="text-lg font-medium text-white">Uploading Files</h3>
          </div>
          <div className="p-6">
            <div className="space-y-4">
              {uploadedFiles.map((file) => (
                <div key={file.name} className="flex items-center space-x-4 p-4 bg-dark-800 rounded-lg">
                  <File className="h-8 w-8 text-primary-500" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-white truncate">{file.name}</p>
                    <p className="text-xs text-dark-400">{formatFileSize(file.size)}</p>
                  </div>
                  <div className="flex items-center space-x-4">
                    <div className="flex-1 w-32">
                      <div className="w-full bg-dark-700 rounded-full h-2">
                        <div
                          className="bg-primary-600 h-2 rounded-full transition-all duration-300"
                          style={{ width: `${uploadProgress[file.name] || 0}%` }}
                        />
                      </div>
                      <p className="text-xs text-dark-400 mt-1">
                        {uploadProgress[file.name] || 0}%
                      </p>
                    </div>
                    {uploadProgress[file.name] === 100 ? (
                      <CheckCircle className="h-5 w-5 text-success-500" />
                    ) : (
                      <button
                        onClick={() => removeFile(file.name)}
                        className="text-dark-400 hover:text-danger-500"
                      >
                        <X className="h-5 w-5" />
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Upload Button */}
      {uploadedFiles.length > 0 && (
        <div className="flex justify-end">
          <button className="btn-primary">
            <Lock className="h-4 w-4 mr-2" />
            Encrypt & Upload {uploadedFiles.length} File{uploadedFiles.length !== 1 ? 's' : ''}
          </button>
        </div>
      )}
    </div>
  );
};

export default FileUpload; 