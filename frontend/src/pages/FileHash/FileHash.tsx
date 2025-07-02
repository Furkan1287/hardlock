import React, { useState, useRef } from 'react';
import { FiUpload, FiFile, FiHash, FiCheck, FiX, FiDownload, FiShield, FiAlertTriangle } from 'react-icons/fi';

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
  const fileInputRef = useRef<HTMLInputElement>(null);

  const algorithms = [
    { value: 'MD5', label: 'MD5 (128-bit)' },
    { value: 'SHA1', label: 'SHA1 (160-bit)' },
    { value: 'SHA256', label: 'SHA256 (256-bit) - Recommended' },
    { value: 'SHA384', label: 'SHA384 (384-bit)' },
    { value: 'SHA512', label: 'SHA512 (512-bit)' }
  ];

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setFilePath('');
    }
  };

  const handleFilePathChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilePath(event.target.value);
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const hashFile = async () => {
    if (!selectedFile && !filePath) {
      alert('Please select a file or enter a file path');
      return;
    }

    setIsProcessing(true);
    setHashResult(null);
    setIntegrityResult(null);

    try {
      let requestBody: any = {
        hashAlgorithm,
        isLargeFile
      };

      if (selectedFile) {
        const fileData = await selectedFile.arrayBuffer();
        requestBody.fileData = Array.from(new Uint8Array(fileData));
      } else if (filePath) {
        requestBody.filePath = filePath;
      }

      const response = await fetch('/api/hash/file', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestBody)
      });

      if (response.ok) {
        const result = await response.json();
        setHashResult(result);
      } else {
        const error = await response.json();
        throw new Error(error.error || 'Failed to hash file');
      }
    } catch (error) {
      console.error('Error hashing file:', error);
      alert(`Error: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setIsProcessing(false);
    }
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
    navigator.clipboard.writeText(text);
    alert('Hash copied to clipboard!');
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-4xl mx-auto">
        <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
          <div className="flex items-center mb-6">
            <FiHash className="text-3xl text-blue-600 mr-3" />
            <div>
              <h1 className="text-2xl font-bold text-gray-900">File Hash & Integrity</h1>
              <p className="text-gray-600">Hash files and verify their integrity</p>
            </div>
          </div>

          {/* File Selection */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            {/* File Upload */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Select File</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Upload File
                  </label>
                  <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-blue-400 transition-colors">
                    <input
                      ref={fileInputRef}
                      type="file"
                      onChange={handleFileSelect}
                      className="hidden"
                      accept="*/*"
                    />
                    <FiUpload className="mx-auto h-12 w-12 text-gray-400 mb-4" />
                    <button
                      onClick={() => fileInputRef.current?.click()}
                      className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors"
                    >
                      Choose File
                    </button>
                    <p className="text-sm text-gray-500 mt-2">
                      or drag and drop
                    </p>
                  </div>
                  {selectedFile && (
                    <div className="mt-2 p-3 bg-green-50 border border-green-200 rounded-md">
                      <div className="flex items-center">
                        <FiFile className="text-green-600 mr-2" />
                        <span className="text-sm text-green-800">
                          {selectedFile.name} ({formatFileSize(selectedFile.size)})
                        </span>
                      </div>
                    </div>
                  )}
                </div>

                <div className="text-center text-gray-500">OR</div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    File Path
                  </label>
                  <input
                    type="text"
                    value={filePath}
                    onChange={handleFilePathChange}
                    placeholder="/path/to/your/file"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
            </div>

            {/* Hash Options */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Hash Options</h3>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Hash Algorithm
                </label>
                <select
                  value={hashAlgorithm}
                  onChange={(e) => setHashAlgorithm(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {algorithms.map((algo) => (
                    <option key={algo.value} value={algo.value}>
                      {algo.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="largeFile"
                  checked={isLargeFile}
                  onChange={(e) => setIsLargeFile(e.target.checked)}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="largeFile" className="ml-2 text-sm text-gray-700">
                  Large file (stream processing)
                </label>
              </div>

              <div className="pt-4">
                <button
                  onClick={hashFile}
                  disabled={isProcessing || (!selectedFile && !filePath)}
                  className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isProcessing ? 'Processing...' : 'Hash File'}
                </button>
              </div>
            </div>
          </div>

          {/* Hash Verification */}
          <div className="border-t pt-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Verify Hash</h3>
            
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Expected Hash
                </label>
                <input
                  type="text"
                  value={expectedHash}
                  onChange={(e) => setExpectedHash(e.target.value)}
                  placeholder="Enter hash to verify"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Verification Mode
                </label>
                <select
                  value={verificationMode}
                  onChange={(e) => setVerificationMode(e.target.value as 'hash' | 'integrity')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="hash">Hash Only</option>
                  <option value="integrity">Full Integrity</option>
                </select>
              </div>

              <div className="flex items-end">
                <button
                  onClick={verifyHash}
                  disabled={isProcessing || !expectedHash}
                  className="w-full bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                  {isProcessing ? 'Verifying...' : 'Verify Hash'}
                </button>
              </div>
            </div>
          </div>

          {/* Clear Results */}
          <div className="flex justify-end">
            <button
              onClick={clearResults}
              className="text-gray-500 hover:text-gray-700 text-sm"
            >
              Clear All
            </button>
          </div>
        </div>

        {/* Results */}
        {hashResult && (
          <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
            <div className="flex items-center mb-4">
              <FiHash className="text-2xl text-green-600 mr-3" />
              <h2 className="text-xl font-semibold text-gray-900">Hash Result</h2>
            </div>

            {hashResult.success ? (
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Hash</label>
                    <div className="flex items-center space-x-2">
                      <code className="flex-1 bg-gray-100 p-2 rounded text-sm font-mono break-all">
                        {hashResult.hash}
                      </code>
                      <button
                        onClick={() => copyToClipboard(hashResult.hash)}
                        className="text-blue-600 hover:text-blue-800"
                        title="Copy hash"
                      >
                        <FiDownload className="h-4 w-4" />
                      </button>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Algorithm</label>
                    <div className="bg-gray-100 p-2 rounded text-sm">
                      {hashResult.algorithm}
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">File Size</label>
                    <div className="bg-gray-100 p-2 rounded text-sm">
                      {formatFileSize(hashResult.fileSize)}
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Hash Time</label>
                    <div className="bg-gray-100 p-2 rounded text-sm">
                      {new Date(hashResult.hashTime).toLocaleString()}
                    </div>
                  </div>
                </div>

                {hashResult.isLargeFile && (
                  <div className="bg-blue-50 border border-blue-200 rounded-md p-3">
                    <div className="flex items-center">
                      <FiAlertTriangle className="text-blue-600 mr-2" />
                      <span className="text-sm text-blue-800">
                        Large file processed using streaming
                      </span>
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="bg-red-50 border border-red-200 rounded-md p-4">
                <div className="flex items-center">
                  <FiX className="text-red-600 mr-2" />
                  <span className="text-red-800">
                    Error: {hashResult.errorMessage}
                  </span>
                </div>
              </div>
            )}
          </div>
        )}

        {integrityResult && (
          <div className="bg-white rounded-lg shadow-lg p-6">
            <div className="flex items-center mb-4">
              <FiShield className={`text-2xl mr-3 ${integrityResult.isValid ? 'text-green-600' : 'text-red-600'}`} />
              <h2 className="text-xl font-semibold text-gray-900">Integrity Verification</h2>
            </div>

            <div className={`p-4 rounded-md ${integrityResult.isValid ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'}`}>
              <div className="flex items-center mb-3">
                {integrityResult.isValid ? (
                  <FiCheck className="text-green-600 mr-2" />
                ) : (
                  <FiX className="text-red-600 mr-2" />
                )}
                <span className={`font-medium ${integrityResult.isValid ? 'text-green-800' : 'text-red-800'}`}>
                  {integrityResult.reason}
                </span>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                {integrityResult.expectedHash && (
                  <div>
                    <label className="block font-medium text-gray-700 mb-1">Expected Hash</label>
                    <code className="bg-gray-100 p-2 rounded font-mono break-all">
                      {integrityResult.expectedHash}
                    </code>
                  </div>
                )}

                {integrityResult.actualHash && (
                  <div>
                    <label className="block font-medium text-gray-700 mb-1">Actual Hash</label>
                    <code className="bg-gray-100 p-2 rounded font-mono break-all">
                      {integrityResult.actualHash}
                    </code>
                  </div>
                )}

                {integrityResult.expectedSize && (
                  <div>
                    <label className="block font-medium text-gray-700 mb-1">Expected Size</label>
                    <div className="bg-gray-100 p-2 rounded">
                      {formatFileSize(integrityResult.expectedSize)}
                    </div>
                  </div>
                )}

                {integrityResult.actualSize && (
                  <div>
                    <label className="block font-medium text-gray-700 mb-1">Actual Size</label>
                    <div className="bg-gray-100 p-2 rounded">
                      {formatFileSize(integrityResult.actualSize)}
                    </div>
                  </div>
                )}

                {integrityResult.verificationTime && (
                  <div>
                    <label className="block font-medium text-gray-700 mb-1">Verification Time</label>
                    <div className="bg-gray-100 p-2 rounded">
                      {new Date(integrityResult.verificationTime).toLocaleString()}
                    </div>
                  </div>
                )}
              </div>

              {integrityResult.errorMessage && (
                <div className="mt-3 p-3 bg-red-100 border border-red-300 rounded">
                  <span className="text-red-800 text-sm">
                    Error: {integrityResult.errorMessage}
                  </span>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default FileHash; 