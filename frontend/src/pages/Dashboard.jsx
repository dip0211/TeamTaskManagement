import React, { useEffect, useState, useCallback } from 'react';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { TaskCommentsModal } from '../components/TaskCommentsModal';
import  CreateTaskModal  from '../components/CreateTaskModal';
import { 
  MessageSquare, 
  Calendar, 
  AlertCircle, 
  Plus, 
  Clock, 
  CheckCircle2, 
  ListTodo, 
  FilterX 
} from 'lucide-react';

export const Dashboard = () => {
  const { user } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filterStatus, setFilterStatus] = useState('');
  const [filterPriority, setFilterPriority] = useState('');
  const [filterDeadline, setFilterDeadline] = useState('');
  const [activeTaskModal, setActiveTaskModal] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);

  const fetchTasks = useCallback(async () => {
    setLoading(true);
    try {
      const params = {};
      if (filterStatus) params.status = filterStatus;
      if (filterPriority) params.priority = filterPriority;
      if (filterDeadline) params.deadline = filterDeadline;

      const { data } = await api.get('/tasks', { params });
      setTasks(data);
    } catch (err) {
      console.error('Failed to load tasks', err);
    } finally {
      setLoading(false);
    }
  }, [filterStatus, filterPriority, filterDeadline]);

  useEffect(() => {
    fetchTasks();
  }, [fetchTasks]);

  const handleStatusChange = async (taskId, newStatus) => {
    try {
      await api.put(`/tasks/${taskId}/status`, { status: Number(newStatus) });
      fetchTasks();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to update task status');
    }
  };

  const clearFilters = () => {
    setFilterStatus('');
    setFilterPriority('');
    setFilterDeadline('');
  };

  // Status Metrics
  const todoCount = tasks.filter(t => t.status === 'ToDo').length;
  const inProgressCount = tasks.filter(t => t.status === 'InProgress').length;
  const doneCount = tasks.filter(t => t.status === 'Done').length;

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Top Header & Action */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Team Tasks Dashboard</h1>
          <p className="text-sm text-slate-500">Manage tasks, deadlines, priorities, and collaboration</p>
        </div>

        {(user?.role === 'Admin' || user?.role === 'Manager') && (
          <button 
            onClick={() => setShowCreateModal(true)} 
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg shadow-xs transition"
          >
            <Plus className="w-4 h-4" /> Create Task
          </button>
        )}
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <div className="bg-white border border-slate-200 rounded-xl p-4 flex items-center gap-4 shadow-xs">
          <div className="p-3 bg-slate-100 text-slate-700 rounded-lg">
            <ListTodo className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-medium text-slate-500 uppercase">To Do</p>
            <p className="text-2xl font-bold text-slate-800">{todoCount}</p>
          </div>
        </div>

        <div className="bg-white border border-slate-200 rounded-xl p-4 flex items-center gap-4 shadow-xs">
          <div className="p-3 bg-amber-50 text-amber-600 rounded-lg">
            <Clock className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-medium text-slate-500 uppercase">In Progress</p>
            <p className="text-2xl font-bold text-slate-800">{inProgressCount}</p>
          </div>
        </div>

        <div className="bg-white border border-slate-200 rounded-xl p-4 flex items-center gap-4 shadow-xs">
          <div className="p-3 bg-emerald-50 text-emerald-600 rounded-lg">
            <CheckCircle2 className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-medium text-slate-500 uppercase">Completed</p>
            <p className="text-2xl font-bold text-slate-800">{doneCount}</p>
          </div>
        </div>
      </div>

      {/* Filter Toolbar */}
      <div className="bg-white border border-slate-200 rounded-xl p-4 mb-6 shadow-xs flex flex-wrap items-center gap-3">
        <div className="text-xs font-semibold text-slate-500 uppercase mr-2">Filters:</div>

        <select 
          value={filterStatus} 
          onChange={e => setFilterStatus(e.target.value)} 
          className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs bg-slate-50 outline-none focus:border-indigo-500"
        >
          <option value="">All Statuses</option>
          <option value="1">To Do</option>
          <option value="2">In Progress</option>
          <option value="3">Done</option>
        </select>

        <select 
          value={filterPriority} 
          onChange={e => setFilterPriority(e.target.value)} 
          className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs bg-slate-50 outline-none focus:border-indigo-500"
        >
          <option value="">All Priorities</option>
          <option value="1">Low</option>
          <option value="2">Medium</option>
          <option value="3">High</option>
        </select>

        <div className="flex items-center gap-1.5">
          <span className="text-xs text-slate-400">Due before:</span>
          <input 
            type="date" 
            value={filterDeadline} 
            onChange={e => setFilterDeadline(e.target.value)} 
            className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs bg-slate-50 outline-none focus:border-indigo-500" 
          />
        </div>

        {(filterStatus || filterPriority || filterDeadline) && (
          <button 
            onClick={clearFilters} 
            className="flex items-center gap-1 text-xs text-rose-600 hover:text-rose-800 ml-auto font-medium"
          >
            <FilterX className="w-3.5 h-3.5" /> Clear Filters
          </button>
        )}
      </div>

      {/* Task Grid / Content */}
      {loading ? (
        <div className="text-center py-16 text-slate-400">Loading tasks...</div>
      ) : tasks.length === 0 ? (
        <div className="text-center py-16 bg-white rounded-xl border border-dashed border-slate-300">
          <AlertCircle className="w-10 h-10 text-slate-400 mx-auto mb-2" />
          <p className="text-slate-600 font-medium">No tasks found matching your criteria</p>
          <p className="text-xs text-slate-400 mt-1">Adjust filters or create a new task</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {tasks.map(task => (
            <div key={task.id} className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs flex flex-col justify-between">
              <div>
                {/* Priority & Status Controls */}
                <div className="flex justify-between items-center mb-3">
                  <span className={`text-[11px] font-bold px-2 py-0.5 rounded-full tracking-wide uppercase ${
                    task.priority === 'High' ? 'bg-rose-100 text-rose-700' :
                    task.priority === 'Medium' ? 'bg-amber-100 text-amber-700' : 
                    'bg-emerald-100 text-emerald-700'
                  }`}>
                    {task.priority}
                  </span>

                  <select 
                    value={task.status === 'ToDo' ? 1 : task.status === 'InProgress' ? 2 : 3}
                    onChange={(e) => handleStatusChange(task.id, e.target.value)}
                    className="text-xs font-medium border border-slate-200 rounded-md px-2 py-1 bg-slate-50 outline-none"
                  >
                    <option value="1">To Do</option>
                    <option value="2">In Progress</option>
                    <option value="3">Done</option>
                  </select>
                </div>

                <h3 className="text-base font-semibold text-slate-800 mb-1">{task.title}</h3>
                <p className="text-xs text-slate-500 line-clamp-3 mb-4">{task.description}</p>
              </div>

              <div>
                {/* Assignee Information */}
                <div className="text-xs text-slate-500 mb-3 pb-3 border-b border-slate-100">
                  <span className="text-slate-400">Assignee:</span>{' '}
                  <span className="font-medium text-slate-700">
                    {task.assignedTo ? task.assignedTo.fullName : 'Unassigned'}
                  </span>
                </div>

                {/* Footer metadata */}
                <div className="flex items-center justify-between text-xs text-slate-500">
                  <span className="flex items-center gap-1">
                    <Calendar className="w-3.5 h-3.5 text-slate-400" />
                    {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No Deadline'}
                  </span>

                  <button 
                    onClick={() => setActiveTaskModal(task)} 
                    className="flex items-center gap-1 text-indigo-600 hover:text-indigo-800 font-medium cursor-pointer"
                  >
                    <MessageSquare className="w-3.5 h-3.5" />
                    {task.commentsCount} {task.commentsCount === 1 ? 'comment' : 'comments'}
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Task Creation Modal */}
      {showCreateModal && (
        <CreateTaskModal 
          onClose={() => setShowCreateModal(false)} 
          onTaskCreated={fetchTasks} 
        />
      )}

      {/* Comments Collaboration Modal */}
      {activeTaskModal && (
        <TaskCommentsModal 
          task={activeTaskModal} 
          onClose={() => { setActiveTaskModal(null); fetchTasks(); }} 
        />
      )}
    </div>
  );
};