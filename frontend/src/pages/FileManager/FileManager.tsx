import React, { useState, useEffect, useContext } from 'react';
import { 
  FolderOpen, 
  File, 
  Download, 
  Trash2, 
  Share, 
  Lock, 
  Search,
  Filter,
  Grid,
  List
} from 'lucide-react';
import { AuthContext } from '../../contexts/AuthContext.tsx';

interface FileItem {
  id: string;
  name: string;
  size: number;
  type: string;
  uploadedAt: string;
  isEncrypted: boolean;
  isShared: boolean;
  permissions: string[];
}

const FileManager: React.FC = () => {
  const [files, setFiles] = useState<FileItem[]>([]);
  const [filteredFiles, setFilteredFiles] = useState<FileItem[]>([]);
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState<'name' | 'size' | 'date'>('date');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('list');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Simulate loading files
    const loadFiles = async () => {
      setIsLoading(true);
      
      setTimeout(() => {
        const mockFiles: FileItem[] = [
          {
            id: '1',
            name: 'confidential_report.pdf',
            size: 15.2 * 1024 * 1024,
            type: 'pdf',
            uploadedAt: '2024-01-15T10:30:00Z',
            isEncrypted: true,
            isShared: false,
            permissions: ['read', 'write']
          },
          {
            id: '2',
            name: 'project_documentation.docx',
            size: 8.7 * 1024 * 1024,
            type: 'docx',
            uploadedAt: '2024-01-15T09:15:00Z',
            isEncrypted: true,
            isShared: true,
            permissions: ['read']
          },
          {
            id: '3',
            name: 'presentation.pptx',
            size: 25.1 * 1024 * 1024,
            type: 'pptx',
            uploadedAt: '2024-01-14T16:45:00Z',
            isEncrypted: false,
            isShared: false,
            permissions: ['read', 'write', 'delete']
          },
          {
            id: '4',
            name: 'financial_data.xlsx',
            size: 12.8 * 1024 * 1024,
            type: 'xlsx',
            uploadedAt: '2024-01-14T14:20:00Z',
            isEncrypted: true,
            isShared: false,
            permissions: ['read']
          },
          {
            id: '5',
            name: 'contract_agreement.pdf',
            size: 5.3 * 1024 * 1024,
            type: 'pdf',
            uploadedAt: '2024-01-13T11:10:00Z',
            isEncrypted: true,
            isShared: true,
            permissions: ['read', 'write']
          }
        ];
        
        setFiles(mockFiles);
        setFilteredFiles(mockFiles);
        setIsLoading(false);
      }, 1000);
    };

    loadFiles();
  }, []);

  useEffect(() => {
    let filtered = files.filter(file =>
      file.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    // Sort files
    filtered.sort((a, b) => {
      let aValue: string | number;
      let bValue: string | number;

      switch (sortBy) {
        case 'name':
          aValue = a.name;
          bValue = b.name;
          break;
        case 'size':
          aValue = a.size;
          bValue = b.size;
          break;
        case 'date':
          aValue = new Date(a.uploadedAt).getTime();
          bValue = new Date(b.uploadedAt).getTime();
          break;
        default:
          aValue = a.name;
          bValue = b.name;
      }

      if (sortOrder === 'asc') {
        return aValue > bValue ? 1 : -1;
      } else {
        return aValue < bValue ? 1 : -1;
      }
    });

    setFilteredFiles(filtered);
  }, [files, searchTerm, sortBy, sortOrder]);

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

  const getFileIcon = (type: string) => {
    const iconMap: { [key: string]: string } = {
      pdf: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      docx: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      pptx: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      xlsx: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
    };
    return iconMap[type] || 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z';
  };

  const handleFileSelect = (fileId: string) => {
    setSelectedFiles(prev =>
      prev.includes(fileId)
        ? prev.filter(id => id !== fileId)
        : [...prev, fileId]
    );
  };

  const handleSelectAll = () => {
    if (selectedFiles.length === filteredFiles.length) {
      setSelectedFiles([]);
    } else {
      setSelectedFiles(filteredFiles.map(file => file.id));
    }
  };

  const handleDeleteFiles = () => {
    if (selectedFiles.length === 0) return;
    
    setFiles(prev => prev.filter(file => !selectedFiles.includes(file.id)));
    setSelectedFiles([]);
  };

  const handleEncryptFiles = () => {
    if (selectedFiles.length === 0) return;
    
    setFiles(prev => prev.map(file =>
      selectedFiles.includes(file.id)
        ? { ...file, isEncrypted: true }
        : file
    ));
    setSelectedFiles([]);
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto mb-4"></div>
          <p className="text-gray-400">Loading files...</p>
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
              <h1 className="text-3xl font-bold text-white mb-2">File Manager</h1>
              <p className="text-gray-400">
                Manage your secure files and folders
              </p>
            </div>
            <div className="flex items-center space-x-4">
              <button className="bg-gradient-to-r from-red-600 to-red-800 text-white px-6 py-3 rounded-lg font-medium hover:from-red-700 hover:to-red-900 transition-all duration-200 transform hover:scale-105">
                <svg className="h-5 w-5 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                Upload Files
              </button>
            </div>
          </div>
        </div>

        {/* Controls */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
            <div className="flex items-center space-x-4">
              <div className="relative">
                <input
                  type="text"
                  placeholder="Search files..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10 pr-4 py-2 bg-gray-700/50 border border-gray-600 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all duration-200"
                />
                <svg className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>

              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value as 'name' | 'size' | 'date')}
                className="px-4 py-2 bg-gray-700/50 border border-gray-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all duration-200"
              >
                <option value="name">Name</option>
                <option value="size">Size</option>
                <option value="date">Date</option>
              </select>

              <button
                onClick={() => setSortOrder(prev => prev === 'asc' ? 'desc' : 'asc')}
                className="p-2 bg-gray-700/50 border border-gray-600 rounded-lg text-gray-400 hover:text-white hover:border-red-500 transition-all duration-200"
              >
                <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
                </svg>
              </button>
            </div>

            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => setViewMode('list')}
                  className={`p-2 rounded-lg transition-all duration-200 ${
                    viewMode === 'list'
                      ? 'bg-red-500/20 text-red-400 border border-red-500/30'
                      : 'bg-gray-700/50 text-gray-400 border border-gray-600 hover:text-white hover:border-red-500'
                  }`}
                >
                  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
                  </svg>
                </button>
                <button
                  onClick={() => setViewMode('grid')}
                  className={`p-2 rounded-lg transition-all duration-200 ${
                    viewMode === 'grid'
                      ? 'bg-red-500/20 text-red-400 border border-red-500/30'
                      : 'bg-gray-700/50 text-gray-400 border border-gray-600 hover:text-white hover:border-red-500'
                  }`}
                >
                  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
                  </svg>
                </button>
              </div>
            </div>
          </div>

          {selectedFiles.length > 0 && (
            <div className="mt-4 flex items-center justify-between p-4 bg-red-500/10 border border-red-500/30 rounded-lg">
              <span className="text-white">
                {selectedFiles.length} file(s) selected
              </span>
              <div className="flex items-center space-x-2">
                <button
                  onClick={handleEncryptFiles}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors duration-200"
                >
                  Encrypt
                </button>
                <button
                  onClick={handleDeleteFiles}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors duration-200"
                >
                  Delete
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Files List */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl border border-gray-700 p-6">
          {viewMode === 'list' ? (
            <div className="space-y-2">
              {/* Header */}
              <div className="flex items-center p-4 bg-gray-700/30 rounded-lg border border-gray-600">
                <input
                  type="checkbox"
                  checked={selectedFiles.length === filteredFiles.length && filteredFiles.length > 0}
                  onChange={handleSelectAll}
                  className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
                />
                <div className="ml-4 flex-1 grid grid-cols-12 gap-4 text-sm font-medium text-gray-300">
                  <div className="col-span-4">Name</div>
                  <div className="col-span-2">Size</div>
                  <div className="col-span-2">Type</div>
                  <div className="col-span-2">Date</div>
                  <div className="col-span-2">Status</div>
                </div>
              </div>

              {/* Files */}
              {filteredFiles.map((file) => (
                <div
                  key={file.id}
                  className={`flex items-center p-4 rounded-lg border transition-all duration-200 hover:border-red-500/50 ${
                    selectedFiles.includes(file.id)
                      ? 'bg-red-500/10 border-red-500/30'
                      : 'bg-gray-700/30 border-gray-600'
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={selectedFiles.includes(file.id)}
                    onChange={() => handleFileSelect(file.id)}
                    className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
                  />
                  <div className="ml-4 flex-1 grid grid-cols-12 gap-4 items-center">
                    <div className="col-span-4 flex items-center space-x-3">
                      <div className={`p-2 rounded-lg ${file.isEncrypted ? 'bg-red-500/20' : 'bg-blue-500/20'}`}>
                        <svg className={`h-5 w-5 ${file.isEncrypted ? 'text-red-400' : 'text-blue-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={getFileIcon(file.type)} />
                        </svg>
                      </div>
                      <div>
                        <p className="text-white font-medium">{file.name}</p>
                        {file.isShared && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-500/20 text-green-400">
                            Shared
                          </span>
                        )}
                      </div>
                    </div>
                    <div className="col-span-2 text-gray-300">{formatFileSize(file.size)}</div>
                    <div className="col-span-2 text-gray-300 uppercase">{file.type}</div>
                    <div className="col-span-2 text-gray-300">{formatDate(file.uploadedAt)}</div>
                    <div className="col-span-2">
                      {file.isEncrypted ? (
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-500/20 text-red-400">
                          Encrypted
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-yellow-500/20 text-yellow-400">
                          Unencrypted
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {filteredFiles.map((file) => (
                <div
                  key={file.id}
                  className={`p-4 rounded-lg border transition-all duration-200 hover:border-red-500/50 cursor-pointer ${
                    selectedFiles.includes(file.id)
                      ? 'bg-red-500/10 border-red-500/30'
                      : 'bg-gray-700/30 border-gray-600'
                  }`}
                  onClick={() => handleFileSelect(file.id)}
                >
                  <div className="flex items-center justify-between mb-3">
                    <input
                      type="checkbox"
                      checked={selectedFiles.includes(file.id)}
                      onChange={(e) => {
                        e.stopPropagation();
                        handleFileSelect(file.id);
                      }}
                      className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-600 rounded bg-gray-700"
                    />
                    <div className={`p-2 rounded-lg ${file.isEncrypted ? 'bg-red-500/20' : 'bg-blue-500/20'}`}>
                      <svg className={`h-5 w-5 ${file.isEncrypted ? 'text-red-400' : 'text-blue-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={getFileIcon(file.type)} />
                      </svg>
                    </div>
                  </div>
                  <div>
                    <p className="text-white font-medium text-sm mb-2 truncate">{file.name}</p>
                    <p className="text-gray-400 text-xs mb-2">{formatFileSize(file.size)}</p>
                    <p className="text-gray-400 text-xs mb-3">{formatDate(file.uploadedAt)}</p>
                    <div className="flex items-center space-x-2">
                      {file.isEncrypted && (
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-500/20 text-red-400">
                          Encrypted
                        </span>
                      )}
                      {file.isShared && (
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-500/20 text-green-400">
                          Shared
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {filteredFiles.length === 0 && (
            <div className="text-center py-12">
              <svg className="h-12 w-12 text-gray-500 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <p className="text-gray-400">No files found</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default FileManager; 