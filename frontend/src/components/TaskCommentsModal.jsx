import React, { useState, useEffect } from 'react';
import { api } from '../api/axiosClient';
import { X, Send } from 'lucide-react';

export const TaskCommentsModal = ({ task, onClose }) => {
  const [comments, setComments] = useState([]);
  const [content, setContent] = useState('');

  const fetchComments = async () => {
    try {
      const { data } = await api.get(`/tasks/${task.id}/comments`);
      setComments(data);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchComments();
  }, [task.id]);

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!content.trim()) return;
    try {
      await api.post(`/tasks/${task.id}/comments`, { content });
      setContent('');
      fetchComments();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
      <div className="bg-white max-w-lg w-full rounded-xl shadow-xl flex flex-col max-h-[85vh]">
        <div className="p-4 border-b flex justify-between items-center">
          <h3 className="font-semibold text-slate-800">Comments — {task.title}</h3>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600"><X className="w-5 h-5" /></button>
        </div>

        <div className="p-4 overflow-y-auto flex-1 space-y-3">
          {comments.length === 0 ? (
            <p className="text-xs text-center text-slate-400 py-6">No comments yet. Start the conversation!</p>
          ) : (
            comments.map(c => (
              <div key={c.id} className="bg-slate-50 p-3 rounded-lg border border-slate-100">
                <div className="flex justify-between items-center mb-1">
                  <span className="text-xs font-semibold text-indigo-700">{c.author.fullName}</span>
                  <span className="text-[10px] text-slate-400">{new Date(c.createdAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</span>
                </div>
                <p className="text-xs text-slate-700">{c.content}</p>
              </div>
            ))
          )}
        </div>

        <form onSubmit={handleAddComment} className="p-3 border-t flex gap-2">
          <input 
            type="text" 
            placeholder="Write a comment..." 
            value={content} 
            onChange={e => setContent(e.target.value)}
            className="flex-1 text-xs border rounded-lg px-3 py-2 outline-none focus:border-indigo-500"
          />
          <button type="submit" className="bg-indigo-600 text-white p-2 rounded-lg hover:bg-indigo-700">
            <Send className="w-4 h-4" />
          </button>
        </form>
      </div>
    </div>
  );
};