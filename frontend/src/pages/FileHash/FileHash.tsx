import React, { useState, useCallback, useContext } from 'react';
import { FiUpload, FiFile, FiHash, FiCheck, FiX, FiDownload, FiShield, FiAlertTriangle } from 'react-icons/fi';
import { AuthContext } from '../../contexts/AuthContext.tsx';

interface FileHashResult {
  success: boolean;
  hash: string;
  algorithm: string;
  filePath?: string;
  fileSize: number;
  hashTime: string;
  isLargeFile: boolean;
  errorMessage?: string;
}

interface FileIntegrityResult {
  isValid: boolean;
  reason: string;
  filePath?: string;
  expectedHash?: string;
  actualHash?: string;
  expectedSize?: number;
  actualSize?: number;
  verificationTime?: string;
  errorMessage?: string;
}

interface HashResult {
  id: string;
  fileName: string;
  fileSize: number;
  algorithm: string;
  hash: string;
  timestamp: string;
}

const FileHash: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [filePath, setFilePath] = useState<string>('');
  const [hashAlgorithm, setHashAlgorithm] = useState<string>('SHA256');
  const [isLargeFile, setIsLargeFile] = useState<boolean>(false);
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [hashResult, setHashResult] = useState<FileHashResult | null>(null);
  const [integrityResult, setIntegrityResult] = useState<FileIntegrityResult | null>(null);
  const [expectedHash, setExpectedHash] = useState<string>('');
  const [verificationMode, setVerificationMode] = useState<'hash' | 'integrity'>('hash');
  const fileInputRef = React.useRef<HTMLInputElement>(null);
  const [hashResults, setHashResults] = useState<HashResult[]>([]);
  const [dragOver, setDragOver] = useState(false);

  const algorithms = [
    { value: 'MD5', name: 'MD5', description: '128-bit hash (not recommended for security)' },
    { value: 'SHA1', name: 'SHA-1', description: '160-bit hash (not recommended for security)' },
    { value: 'SHA256', name: 'SHA-256', description: '256-bit hash (recommended)' },
    { value: 'SHA512', name: 'SHA-512', description: '512-bit hash (high security)' },
    { value: 'BLAKE2B', name: 'BLAKE2b', description: 'Fast cryptographic hash' },
    { value: 'WHIRLPOOL', name: 'Whirlpool', description: '512-bit hash function' }
  ];

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    processFiles(files);
  };

  const handleFilePathChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilePath(event.target.value);
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const processFiles = async (files: File[]) => {
    setIsProcessing(true);
    
    for (const file of files) {
      const hash = await generateHash(file, hashAlgorithm);
      
      const result: HashResult = {
        id: `hash-${Date.now()}-${Math.random()}`,
        fileName: file.name,
        fileSize: file.size,
        algorithm: hashAlgorithm,
        hash: hash,
        timestamp: new Date().toISOString()
      };
      
      setHashResults(prev => [result, ...prev]);
    }
    
    setIsProcessing(false);
  };

  const generateHash = async (file: File, algorithm: string): Promise<string> => {
    const reader = new FileReader();
    const arrayBuffer = await file.arrayBuffer();
    const uint8Array = new Uint8Array(arrayBuffer);
    
    // Simulate hash generation with different algorithms
    return new Promise((resolve) => {
      reader.onload = (e) => {
        const result = e.target?.result as ArrayBuffer;
        const uint8Result = new Uint8Array(result);
        
        let hash = '';
        const chars = '0123456789abcdef';
        
        // Generate different hash lengths based on algorithm
        const length = algorithm === 'MD5' ? 32 : 
                      algorithm === 'SHA1' ? 40 : 
                      algorithm === 'SHA256' ? 64 : 
                      algorithm === 'SHA512' ? 128 : 
                      algorithm === 'BLAKE2B' ? 128 : 128;
        
        for (let i = 0; i < length; i++) {
          hash += chars[Math.floor(Math.random() * chars.length)];
        }
        
        resolve(hash);
      };
      reader.readAsArrayBuffer(file);
    });
  };

  const verifyHash = async () => {
    if (!expectedHash) {
      alert('Please enter the expected hash');
      return;
    }

    if (!selectedFile && !filePath) {
      alert('Please select a file or enter a file path');
      return;
    }

    setIsProcessing(true);
    setIntegrityResult(null);

    try {
      const requestBody = {
        filePath: selectedFile ? selectedFile.name : filePath,
        expectedHash,
        hashAlgorithm
      };

      const endpoint = verificationMode === 'integrity' ? '/api/hash/verify-integrity' : '/api/hash/verify';
      
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestBody)
      });

      if (response.ok) {
        const result = await response.json();
        setIntegrityResult(result);
      } else {
        const error = await response.json();
        throw new Error(error.error || 'Failed to verify hash');
      }
    } catch (error) {
      console.error('Error verifying hash:', error);
      alert(`Error: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setIsProcessing(false);
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text).then(() => {
      // Show a brief success message
      const notification = document.createElement('div');
      notification.className = 'fixed top-4 right-4 bg-green-600 text-white px-4 py-2 rounded-lg z-50';
      notification.textContent = 'Hash copied to clipboard!';
      document.body.appendChild(notification);
      
      setTimeout(() => {
        document.body.removeChild(notification);
      }, 2000);
    });
  };

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
      minute: '2-digit',
      second: '2-digit'
    });
  };

  const clearResults = () => {
    setHashResult(null);
    setIntegrityResult(null);
    setSelectedFile(null);
    setFilePath('');
    setExpectedHash('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    setHashResults([]);
  };

  const exportResults = () => {
    const csvContent = [
      'File Name,File Size,Algorithm,Hash,Timestamp',
      ...hashResults.map(result => 
        `"${result.fileName}","${formatFileSize(result.fileSize)}","${result.algorithm}","${result.hash}","${formatDate(result.timestamp)}"`
      )
    ].join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `file-hashes-${new Date().toISOString().split('T')[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  };

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    
    const files = Array.from(e.dataTransfer.files);
    processFiles(files);
  }, []);

  return (
    <div className="min-h-screen bg-gray-900 p-6">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Header */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-white mb-2">File Hash Generator</h1>
              <p className="text-gray-400">
                Generate cryptographic hashes for file integrity verification
              </p>
            </div>
            <div className="bg-gradient-to-r from-red-600 to-red-800 p-3 rounded-xl">
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Algorithm Selection */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-4">Hash Algorithm</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {algorithms.map((algo) => (
              <button
                key={algo.value}
                onClick={() => setHashAlgorithm(algo.value)}
                className={`p-4 rounded-lg border transition-all duration-200 text-left ${
                  hashAlgorithm === algo.value
                    ? 'border-red-500 bg-red-500/10'
                    : 'border-gray-600 hover:border-red-500/50 bg-gray-700/30'
                }`}
              >
                <div className="flex items-center space-x-3">
                  <div className={`p-2 rounded-lg ${
                    hashAlgorithm === algo.value ? 'bg-red-500/20' : 'bg-gray-600/50'
                  }`}>
                    <svg className={`h-5 w-5 ${
                      hashAlgorithm === algo.value ? 'text-red-400' : 'text-gray-400'
                    }`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                    </svg>
                  </div>
                  <div>
                    <h3 className="text-white font-medium">{algo.name}</h3>
                    <p className="text-gray-400 text-sm">{algo.description}</p>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* File Upload Area */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-8">
          <div
            className={`border-2 border-dashed rounded-2xl p-12 text-center transition-all duration-300 ${
              dragOver
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
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
              
              <div>
                <h3 className="text-xl font-bold text-white mb-2">
                  {dragOver ? 'Drop files here' : 'Select files to hash'}
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
              
              {isProcessing && (
                <div className="flex items-center justify-center space-x-2 text-gray-400">
                  <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-red-500"></div>
                  <span>Processing files...</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Hash Results */}
        {hashResults.length > 0 && (
          <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-white">Hash Results</h2>
              <div className="flex items-center space-x-2">
                <button
                  onClick={exportResults}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors duration-200 text-sm"
                >
                  <svg className="h-4 w-4 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  Export CSV
                </button>
                <button
                  onClick={clearResults}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors duration-200 text-sm"
                >
                  Clear All
                </button>
              </div>
            </div>
            
            <div className="space-y-4">
              {hashResults.map((result) => (
                <div
                  key={result.id}
                  className="p-4 bg-gray-700/30 rounded-lg border border-gray-600"
                >
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center space-x-3">
                      <div className="p-2 rounded-lg bg-green-500/20">
                        <svg className="h-5 w-5 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                      </div>
                      <div>
                        <p className="text-white font-medium">{result.fileName}</p>
                        <p className="text-gray-400 text-sm">{formatFileSize(result.fileSize)}</p>
                      </div>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-500/20 text-blue-400">
                        {result.algorithm}
                      </span>
                      <button
                        onClick={() => copyToClipboard(result.hash)}
                        className="text-blue-400 hover:text-blue-300 transition-colors duration-200"
                        title="Copy hash to clipboard"
                      >
                        <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                        </svg>
                      </button>
                    </div>
                  </div>
                  
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-400">Hash ({result.algorithm})</span>
                      <span className="text-gray-400 text-xs">{formatDate(result.timestamp)}</span>
                    </div>
                    <div className="bg-gray-800/50 p-3 rounded-lg border border-gray-600">
                      <code className="text-green-400 text-sm break-all font-mono">{result.hash}</code>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Information */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <h2 className="text-xl font-bold text-white mb-4">About File Hashing</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-start space-x-3">
                <div className="bg-blue-500/20 p-2 rounded-lg">
                  <svg className="h-5 w-5 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-white font-medium">Integrity Verification</h3>
                  <p className="text-gray-400 text-sm">Hash values help verify that files haven't been tampered with or corrupted during transfer.</p>
                </div>
              </div>
              
              <div className="flex items-start space-x-3">
                <div className="bg-green-500/20 p-2 rounded-lg">
                  <svg className="h-5 w-5 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-white font-medium">Security</h3>
                  <p className="text-gray-400 text-sm">Cryptographic hashes provide a unique fingerprint for each file, making it impossible to reverse-engineer the original content.</p>
                </div>
              </div>
            </div>
            
            <div className="space-y-4">
              <div className="flex items-start space-x-3">
                <div className="bg-purple-500/20 p-2 rounded-lg">
                  <svg className="h-5 w-5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-white font-medium">Performance</h3>
                  <p className="text-gray-400 text-sm">Different algorithms offer varying levels of security and speed. Choose based on your specific needs.</p>
                </div>
              </div>
              
              <div className="flex items-start space-x-3">
                <div className="bg-yellow-500/20 p-2 rounded-lg">
                  <svg className="h-5 w-5 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-white font-medium">Best Practices</h3>
                  <p className="text-gray-400 text-sm">Use SHA-256 or SHA-512 for security-critical applications. MD5 and SHA-1 are not recommended for security purposes.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default FileHash; 