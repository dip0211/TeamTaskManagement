import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, CheckSquare, Users } from 'lucide-react';

export const Navbar = () => {
  const { user, logout } = useAuth();
  if (!user) return null;

  return (
    <nav className="bg-slate-900 text-white px-6 py-3 flex justify-between items-center shadow-md">
      <div className="flex items-center space-x-6">
        <span className="text-xl font-bold flex items-center gap-2 text-indigo-400">
          <CheckSquare className="w-6 h-6" /> TaskManager
        </span>
        <Link to="/dashboard" className="text-sm hover:text-indigo-300">Dashboard</Link>
        {(user.role === 'Admin' || user.role === 'Manager') && (
          <Link to="/teams" className="text-sm hover:text-indigo-300 flex items-center gap-1">
            <Users className="w-4 h-4" /> Teams
          </Link>
        )}
      </div>
      <div className="flex items-center space-x-4">
        <div className="text-right">
          <p className="text-sm font-semibold">{user.fullName}</p>
          <span className="text-xs bg-indigo-800 text-indigo-200 px-2 py-0.5 rounded-full">{user.role}</span>
        </div>
        <button onClick={logout} className="p-2 hover:bg-slate-800 rounded-full text-rose-400" title="Sign out">
          <LogOut className="w-5 h-5" />
        </button>
      </div>
    </nav>
  );
};