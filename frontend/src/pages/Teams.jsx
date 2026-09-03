import React, { useState, useEffect } from 'react';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { Users, UserPlus, FolderPlus } from 'lucide-react';

export const Teams = () => {
  const { user } = useAuth();
  const [teams, setTeams] = useState([]);
  const [users, setUsers] = useState([]);
  const [newTeamName, setNewTeamName] = useState('');
  const [selectedTeamId, setSelectedTeamId] = useState('');
  const [selectedUserId, setSelectedUserId] = useState('');

  const loadData = async () => {
    const [teamsRes, usersRes] = await Promise.all([
      api.get('/teams'),
      api.get('/users')
    ]);
    setTeams(teamsRes.data);
    setUsers(usersRes.data);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleCreateTeam = async (e) => {
    e.preventDefault();
    if (!newTeamName.trim()) return;
    try {
      await api.post('/teams', { name: newTeamName });
      setNewTeamName('');
      loadData();
    } catch (err) {
      alert(err.response?.data?.message || 'Error creating team');
    }
  };

  const handleAssignMember = async (e) => {
    e.preventDefault();
    if (!selectedTeamId || !selectedUserId) return;
    try {
      await api.post(`/teams/${selectedTeamId}/members`, { userId: Number(selectedUserId) });
      loadData();
      alert('Member assigned successfully');
    } catch (err) {
      alert(err.response?.data?.message || 'Error assigning member');
    }
  };

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <h1 className="text-2xl font-bold text-slate-800 mb-6">Team Management</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        {/* Admin only: Create Team */}
        {user?.role === 'Admin' && (
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <h2 className="text-base font-semibold text-slate-800 flex items-center gap-2 mb-3">
              <FolderPlus className="w-5 h-5 text-indigo-600" /> Create New Team
            </h2>
            <form onSubmit={handleCreateTeam} className="flex gap-2">
              <input 
                type="text" 
                placeholder="Team name" 
                value={newTeamName} 
                onChange={e => setNewTeamName(e.target.value)}
                className="flex-1 px-3 py-2 border rounded-lg text-sm outline-none focus:border-indigo-500" 
              />
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700">
                Create
              </button>
            </form>
          </div>
        )}

        {/* Admin & Manager: Assign Member to Team */}
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
          <h2 className="text-base font-semibold text-slate-800 flex items-center gap-2 mb-3">
            <UserPlus className="w-5 h-5 text-indigo-600" /> Assign User to Team
          </h2>
          <form onSubmit={handleAssignMember} className="flex flex-col sm:flex-row gap-2">
            <select 
              value={selectedTeamId} 
              onChange={e => setSelectedTeamId(e.target.value)}
              className="flex-1 px-3 py-2 border rounded-lg text-sm bg-white"
            >
              <option value="">Select Team</option>
              {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>

            <select 
              value={selectedUserId} 
              onChange={e => setSelectedUserId(e.target.value)}
              className="flex-1 px-3 py-2 border rounded-lg text-sm bg-white"
            >
              <option value="">Select User</option>
              {users.map(u => <option key={u.id} value={u.id}>{u.fullName} ({u.role})</option>)}
            </select>

            <button type="submit" className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700">
              Assign
            </button>
          </form>
        </div>
      </div>

      {/* Teams Grid */}
      <h2 className="text-lg font-bold text-slate-800 mb-4">Teams Overview</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        {teams.map(team => (
          <div key={team.id} className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center gap-2 mb-3">
              <Users className="w-5 h-5 text-indigo-500" />
              <h3 className="font-semibold text-slate-800">{team.name}</h3>
            </div>
            <p className="text-xs text-slate-500 mb-3">Members ({team.members.length}):</p>
            <ul className="space-y-1">
              {team.members.map(m => (
                <li key={m.id} className="text-xs bg-slate-50 px-2 py-1 rounded flex justify-between">
                  <span>{m.fullName}</span>
                  <span className="text-slate-400">{m.role}</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </div>
  );
};