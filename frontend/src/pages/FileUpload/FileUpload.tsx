import React, { useState, useCallback, useContext } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, Lock, Shield, File, X, CheckCircle } from 'lucide-react';
import { AuthContext } from '../../contexts/AuthContext.tsx';

interface UploadedFile {
  id: string;
  name: string;
  size: number;
  progress: number;
  status: 'uploading' | 'completed' | 'error';
  error?: string;
}

const FileUpload: React.FC = () => {
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
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
  const [isDragOver, setIsDragOver] = useState(false);
  const [isUploading, setIsUploading] = useState(false);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    handleFiles(acceptedFiles);
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

  const removeFile = (fileId: string) => {
    setUploadedFiles(prev => prev.filter(f => f.id !== fileId));
    setUploadProgress(prev => {
      const newProgress = { ...prev };
      Object.keys(newProgress).forEach(key => {
        if (key.startsWith(fileId)) {
          delete newProgress[key];
        }
      });
      return newProgress;
    });
  };

  const formatFileSize = (bytes: number): string => {
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
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

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    
    const files = Array.from(e.dataTransfer.files);
    handleFiles(files);
  }, []);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    handleFiles(files);
  };

  const handleFiles = (files: File[]) => {
    setIsUploading(true);
    
    files.forEach((file, index) => {
      const fileId = `file-${Date.now()}-${index}`;
      const uploadedFile: UploadedFile = {
        id: fileId,
        name: file.name,
        size: file.size,
        progress: 0,
        status: 'uploading'
      };

      setUploadedFiles(prev => [...prev, uploadedFile]);

      // Simulate file upload progress
      const interval = setInterval(() => {
        setUploadedFiles(prev => prev.map(f => {
          if (f.id === fileId) {
            const newProgress = Math.min(f.progress + Math.random() * 20, 100);
            const newStatus = newProgress === 100 ? 'completed' : 'uploading';
            return { ...f, progress: newProgress, status: newStatus };
          }
          return f;
        }));
      }, 200);

      // Complete upload after 3-5 seconds
      setTimeout(() => {
        clearInterval(interval);
        setUploadedFiles(prev => prev.map(f => {
          if (f.id === fileId && f.status === 'uploading') {
            return { ...f, progress: 100, status: 'completed' };
          }
          return f;
        }));
        
        // Check if all uploads are complete
        setTimeout(() => {
          const allCompleted = uploadedFiles.every(f => f.status === 'completed');
          if (allCompleted) {
            setIsUploading(false);
          }
        }, 1000);
      }, 3000 + Math.random() * 2000);
    });
  };

  const retryUpload = (fileId: string) => {
    setUploadedFiles(prev => prev.map(f => {
      if (f.id === fileId) {
        return { ...f, progress: 0, status: 'uploading', error: undefined };
      }
      return f;
    }));
    
    // Simulate retry
    setTimeout(() => {
      setUploadedFiles(prev => prev.map(f => {
        if (f.id === fileId) {
          return { ...f, progress: 100, status: 'completed' };
        }
        return f;
      }));
    }, 2000);
  };

  return (
    <div className="min-h-screen bg-gray-900 p-6">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Header */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-white mb-2">File Upload</h1>
              <p className="text-gray-400">
                Upload and encrypt your files securely
              </p>
            </div>
            <div className="bg-gradient-to-r from-red-600 to-red-800 p-3 rounded-xl">
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
            </div>
          </div>
        </div>

        {/* Upload Area */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-8">
          <div
            className={`border-2 border-dashed rounded-2xl p-12 text-center transition-all duration-300 ${
              isDragOver
                ? 'border-red-500 bg-red-500/10'
                : 'border-gray-600 hover:border-red-500/50'
            }`}
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
          >
            <div className="space-y-4">
              <div className="mx-auto w-16 h-16 bg-gradient-to-r from-red-600 to-red-800 rounded-full flex items-center justify-center">
                <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
              </div>
              
              <div>
                <h3 className="text-xl font-bold text-white mb-2">
                  {isDragOver ? 'Drop files here' : 'Upload your files'}
                </h3>
                <p className="text-gray-400 mb-6">
                  Drag and drop files here, or click to browse
                </p>
                
                <label className="bg-gradient-to-r from-red-600 to-red-800 text-white px-6 py-3 rounded-lg font-medium hover:from-red-700 hover:to-red-900 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 focus:ring-offset-gray-800 transition-all duration-200 transform hover:scale-105 cursor-pointer inline-block">
                  <svg className="h-5 w-5 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                  </svg>
                  Choose Files
                  <input
                    type="file"
                    multiple
                    onChange={handleFileSelect}
                    className="hidden"
                    accept="*/*"
                  />
                </label>
              </div>
              
              <p className="text-gray-500 text-sm">
                Maximum file size: 100MB • Supported formats: All
              </p>
            </div>
          </div>
        </div>

        {/* Upload Options */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-4">Upload Options</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="flex items-center space-x-3 p-4 bg-gray-700/30 rounded-lg border border-gray-600">
              <input
                type="checkbox"
                id="encrypt"
                defaultChecked
                className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
              />
              <label htmlFor="encrypt" className="text-white font-medium">
                Encrypt files
              </label>
            </div>
            
            <div className="flex items-center space-x-3 p-4 bg-gray-700/30 rounded-lg border border-gray-600">
              <input
                type="checkbox"
                id="compress"
                className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
              />
              <label htmlFor="compress" className="text-white font-medium">
                Compress files
              </label>
            </div>
            
            <div className="flex items-center space-x-3 p-4 bg-gray-700/30 rounded-lg border border-gray-600">
              <input
                type="checkbox"
                id="backup"
                defaultChecked
                className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
              />
              <label htmlFor="backup" className="text-white font-medium">
                Create backup
              </label>
            </div>
          </div>
        </div>

        {/* Upload Progress */}
        {uploadedFiles.length > 0 && (
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Upload Progress</h2>
              <div className="flex items-center space-x-2">
                <span className="text-gray-400 text-sm">
                  {uploadedFiles.filter(f => f.status === 'completed').length} of {uploadedFiles.length} completed
                </span>
                {isUploading && (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-red-500"></div>
                )}
              </div>
            </div>
            
            <div className="space-y-4">
              {uploadedFiles.map((file) => (
                <div
                  key={file.id}
                  className="p-4 bg-gray-700/30 rounded-lg border border-gray-600"
                >
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center space-x-3">
                      <div className={`p-2 rounded-lg ${
                        file.status === 'completed' ? 'bg-green-500/20' :
                        file.status === 'error' ? 'bg-red-500/20' :
                        'bg-blue-500/20'
                      }`}>
                        <svg className={`h-5 w-5 ${
                          file.status === 'completed' ? 'text-green-400' :
                          file.status === 'error' ? 'text-red-400' :
                          'text-blue-400'
                        }`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                      </div>
                      <div>
                        <p className="text-white font-medium">{file.name}</p>
                        <p className="text-gray-400 text-sm">{formatFileSize(file.size)}</p>
                      </div>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      {file.status === 'error' && (
                        <button
                          onClick={() => retryUpload(file.id)}
                          className="px-3 py-1 bg-blue-600 text-white rounded-lg text-xs hover:bg-blue-700 transition-colors duration-200"
                        >
                          Retry
                        </button>
                      )}
                      <button
                        onClick={() => removeFile(file.id)}
                        className="text-red-400 hover:text-red-300 transition-colors duration-200"
                      >
                        <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </div>
                  </div>
                  
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-400">Progress</span>
                      <span className="text-white">{Math.round(file.progress)}%</span>
                    </div>
                    <div className="w-full bg-gray-600 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full transition-all duration-300 ${
                          file.status === 'completed' ? 'bg-green-500' :
                          file.status === 'error' ? 'bg-red-500' :
                          'bg-blue-500'
                        }`}
                        style={{ width: `${file.progress}%` }}
                      ></div>
                    </div>
                    {file.status === 'error' && file.error && (
                      <p className="text-red-400 text-sm">{file.error}</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Upload Tips */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-4">Upload Tips</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-start space-x-3">
              <div className="bg-blue-500/20 p-2 rounded-lg">
                <svg className="h-5 w-5 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div>
                <h3 className="text-white font-medium">File Size</h3>
                <p className="text-gray-400 text-sm">Keep files under 100MB for faster uploads</p>
              </div>
            </div>
            
            <div className="flex items-start space-x-3">
              <div className="bg-green-500/20 p-2 rounded-lg">
                <svg className="h-5 w-5 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </div>
              <div>
                <h3 className="text-white font-medium">Encryption</h3>
                <p className="text-gray-400 text-sm">All files are automatically encrypted for security</p>
              </div>
            </div>
            
            <div className="flex items-start space-x-3">
              <div className="bg-purple-500/20 p-2 rounded-lg">
                <svg className="h-5 w-5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div>
                <h3 className="text-white font-medium">Backup</h3>
                <p className="text-gray-400 text-sm">Automatic backups ensure your files are safe</p>
              </div>
            </div>
            
            <div className="flex items-start space-x-3">
              <div className="bg-yellow-500/20 p-2 rounded-lg">
                <svg className="h-5 w-5 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div>
                <h3 className="text-white font-medium">Speed</h3>
                <p className="text-gray-400 text-sm">Upload speed depends on your internet connection</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default FileUpload; 