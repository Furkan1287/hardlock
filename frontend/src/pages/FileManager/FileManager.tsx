import React, { useState } from 'react';
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

const FileManager: React.FC = () => {
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [searchTerm, setSearchTerm] = useState('');

  const files = [
    {
      id: 1,
      name: 'Project_Documentation.pdf',
      size: '2.4 MB',
      type: 'pdf',
      lastModified: '2024-01-15',
      isEncrypted: true,
      isShared: false,
    },
    {
      id: 2,
      name: 'Financial_Report.xlsx',
      size: '1.8 MB',
      type: 'excel',
      lastModified: '2024-01-14',
      isEncrypted: true,
      isShared: true,
    },
    {
      id: 3,
      name: 'Presentation.pptx',
      size: '5.2 MB',
      type: 'powerpoint',
      lastModified: '2024-01-13',
      isEncrypted: true,
      isShared: false,
    },
    {
      id: 4,
      name: 'Code_Repository.zip',
      size: '15.7 MB',
      type: 'archive',
      lastModified: '2024-01-12',
      isEncrypted: true,
      isShared: true,
    },
  ];

  const getFileIcon = (type: string) => {
    switch (type) {
      case 'pdf':
        return '📄';
      case 'excel':
        return '📊';
      case 'powerpoint':
        return '📈';
      case 'archive':
        return '📦';
      default:
        return '📄';
    }
  };

  const filteredFiles = files.filter(file =>
    file.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">File Manager</h1>
          <p className="text-dark-400 mt-1">Manage your encrypted files securely</p>
        </div>
        <div className="flex items-center space-x-4">
          <button className="btn-primary">
            <FolderOpen className="h-4 w-4 mr-2" />
            New Folder
          </button>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="card p-4">
        <div className="flex items-center space-x-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-dark-400" />
            <input
              type="text"
              placeholder="Search files..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input pl-10 w-full"
            />
          </div>
          <button className="btn-secondary">
            <Filter className="h-4 w-4 mr-2" />
            Filter
          </button>
          <div className="flex items-center space-x-2">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 rounded-lg ${
                viewMode === 'grid'
                  ? 'bg-primary-600 text-white'
                  : 'text-dark-400 hover:text-white hover:bg-dark-700'
              }`}
            >
              <Grid className="h-4 w-4" />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={`p-2 rounded-lg ${
                viewMode === 'list'
                  ? 'bg-primary-600 text-white'
                  : 'text-dark-400 hover:text-white hover:bg-dark-700'
              }`}
            >
              <List className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Files Grid/List */}
      {viewMode === 'grid' ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredFiles.map((file) => (
            <div key={file.id} className="card-hover p-4 cursor-pointer">
              <div className="flex items-center justify-between mb-3">
                <span className="text-2xl">{getFileIcon(file.type)}</span>
                <div className="flex items-center space-x-1">
                  {file.isEncrypted && (
                    <Lock className="h-4 w-4 text-success-500" />
                  )}
                  {file.isShared && (
                    <Share className="h-4 w-4 text-primary-500" />
                  )}
                </div>
              </div>
              <h3 className="font-medium text-white truncate">{file.name}</h3>
              <p className="text-sm text-dark-400">{file.size}</p>
              <p className="text-xs text-dark-400">{file.lastModified}</p>
              <div className="flex items-center justify-between mt-3 pt-3 border-t border-dark-700">
                <button className="text-primary-500 hover:text-primary-400">
                  <Download className="h-4 w-4" />
                </button>
                <button className="text-primary-500 hover:text-primary-400">
                  <Share className="h-4 w-4" />
                </button>
                <button className="text-danger-500 hover:text-danger-400">
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="card">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-dark-700">
              <thead className="bg-dark-800">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-dark-400 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-dark-400 uppercase tracking-wider">
                    Size
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-dark-400 uppercase tracking-wider">
                    Modified
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-dark-400 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-dark-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-dark-900 divide-y divide-dark-700">
                {filteredFiles.map((file) => (
                  <tr key={file.id} className="hover:bg-dark-800">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <span className="text-lg mr-3">{getFileIcon(file.type)}</span>
                        <div>
                          <div className="text-sm font-medium text-white">{file.name}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-dark-400">
                      {file.size}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-dark-400">
                      {file.lastModified}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center space-x-2">
                        {file.isEncrypted && (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-success-100 text-success-800">
                            <Lock className="h-3 w-3 mr-1" />
                            Encrypted
                          </span>
                        )}
                        {file.isShared && (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
                            <Share className="h-3 w-3 mr-1" />
                            Shared
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end space-x-2">
                        <button className="text-primary-500 hover:text-primary-400">
                          <Download className="h-4 w-4" />
                        </button>
                        <button className="text-primary-500 hover:text-primary-400">
                          <Share className="h-4 w-4" />
                        </button>
                        <button className="text-danger-500 hover:text-danger-400">
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {filteredFiles.length === 0 && (
        <div className="text-center py-12">
          <FolderOpen className="mx-auto h-12 w-12 text-dark-400" />
          <h3 className="mt-2 text-sm font-medium text-white">No files found</h3>
          <p className="mt-1 text-sm text-dark-400">
            {searchTerm ? 'Try adjusting your search terms.' : 'Get started by uploading your first file.'}
          </p>
        </div>
      )}
    </div>
  );
};

export default FileManager; 